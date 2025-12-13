using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmPaymentManager : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly int? _invoiceId;
        private readonly string _invoiceNumber;

        private DataTable _table;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Label _lblCount;
        private Button _btnAdd, _btnDelete, _btnRefresh;

        public FrmPaymentManager(AdminDataBLL bll, int? invoiceId = null, string invoiceNumber = null)
        {
            _bll = bll;
            _invoiceId = invoiceId;
            _invoiceNumber = invoiceNumber;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = _invoiceId.HasValue ? $"Thanh toán - {_invoiceNumber ?? _invoiceId.Value.ToString()}" : "Thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 650;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };

            _txtSearch = new TextBox { Width = 260 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };

            _btnAdd = new Button { Text = "Thu tiền", Width = 90 };
            _btnDelete = new Button { Text = "Xóa", Width = 80 };
            _btnRefresh = new Button { Text = "Tải lại", Width = 80 };

            _btnAdd.Click += async (s, e) => await AddNewAsync();
            _btnDelete.Click += async (s, e) => await DeleteSelectedAsync();
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var top = new Panel { Dock = DockStyle.Top, Height = 44 };
            _btnAdd.Location = new Point(10, 10);
            _btnDelete.Location = new Point(110, 10);
            _btnRefresh.Location = new Point(200, 10);
            _txtSearch.Location = new Point(300, 10);
            _lblCount.Location = new Point(580, 14);

            top.Controls.Add(_btnAdd);
            top.Controls.Add(_btnDelete);
            top.Controls.Add(_btnRefresh);
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_lblCount);

            Controls.Add(_grid);
            Controls.Add(top);
            Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _table = _invoiceId.HasValue
                    ? await _bll.GetPaymentsByInvoiceAsync(_invoiceId.Value)
                    : await _bll.GetPaymentsViewAsync();
                _grid.DataSource = _table;
                ApplyFilter();
                AutoFormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AutoFormatGrid()
        {
            if (_grid.Columns.Contains("PaymentAmount"))
                _grid.Columns["PaymentAmount"].DefaultCellStyle.Format = "N0";
        }

        private void ApplyFilter()
        {
            if (_table == null) return;
            string keyword = _txtSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(keyword))
            {
                _table.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                var filters = new System.Collections.Generic.List<string>();
                if (_table.Columns.Contains("InvoiceNumber")) filters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_table.Columns.Contains("TenantName")) filters.Add($"TenantName LIKE '%{keyword}%'");
                if (_table.Columns.Contains("RoomNumber")) filters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (_table.Columns.Contains("PaymentMethod")) filters.Add($"PaymentMethod LIKE '%{keyword}%'");
                if (_table.Columns.Contains("TransactionReference")) filters.Add($"TransactionReference LIKE '%{keyword}%'");
                if (_table.Columns.Contains("PaymentId")) filters.Add($"Convert(PaymentId, 'System.String') LIKE '%{keyword}%'");
                if (_table.Columns.Contains("InvoiceId")) filters.Add($"Convert(InvoiceId, 'System.String') LIKE '%{keyword}%'");

                _table.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" OR ", filters) : string.Empty;
            }
            _lblCount.Text = $"Tổng: {_table.DefaultView.Count}";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }

        private async Task AddNewAsync()
        {
            try
            {
                DataRow invoiceRow = null;
                if (_invoiceId.HasValue)
                {
                    var invTable = await _bll.GetInvoicesViewAsync();
                    invoiceRow = invTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["InvoiceId"]) == _invoiceId.Value);
                }
                else
                {
                    invoiceRow = await PickInvoiceAsync();
                }

                if (invoiceRow == null)
                {
                    MessageBox.Show("Không chọn được hóa đơn để thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var frm = new FrmPaymentEditor(_bll, invoiceRow))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thu tiền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<DataRow> PickInvoiceAsync()
        {
            var invTable = await _bll.GetInvoicesViewAsync();
            if (!invTable.Columns.Contains("InvoiceDisplay"))
                invTable.Columns.Add("InvoiceDisplay", typeof(string));

            foreach (DataRow r in invTable.Rows)
            {
                decimal remaining = ReadDecimal(r, "RemainingAmount");
                string invNo = invTable.Columns.Contains("InvoiceNumber") ? r["InvoiceNumber"]?.ToString() : r["InvoiceId"]?.ToString();
                string tenant = invTable.Columns.Contains("TenantName") ? r["TenantName"]?.ToString() : r["TenantId"]?.ToString();
                string room = invTable.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : r["RoomId"]?.ToString();
                r["InvoiceDisplay"] = $"{invNo} | {tenant} | Phòng {room} | Còn: {remaining:N0}";
            }

            var selectable = invTable.AsEnumerable()
                .Where(r => ReadDecimal(r, "RemainingAmount") > 0)
                .ToList();

            if (selectable.Count == 0) return null;

            using (var dlg = new InvoicePickerDialog(selectable.CopyToDataTable()))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return null;
                return dlg.SelectedRow;
            }
        }

        private async Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int paymentId = row.Table.Columns.Contains("PaymentId") ? Convert.ToInt32(row["PaymentId"]) : 0;
            if (paymentId <= 0)
            {
                MessageBox.Show("Không xác định được PaymentId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Xóa thanh toán ID {paymentId}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeletePaymentAsync(paymentId);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return 0m;
        }

        private class InvoicePickerDialog : Form
        {
            private readonly DataTable _table;
            private ComboBox _cbo;
            private Button _btnOk;
            private Button _btnCancel;
            public DataRow SelectedRow { get; private set; }

            public InvoicePickerDialog(DataTable table)
            {
                _table = table;
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                Text = "Chọn hóa đơn";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(620, 140);

                _cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 580, Location = new Point(20, 20) };
                _cbo.DataSource = _table;
                _cbo.DisplayMember = _table.Columns.Contains("InvoiceDisplay") ? "InvoiceDisplay" : _table.Columns[0].ColumnName;
                _cbo.ValueMember = _table.Columns.Contains("InvoiceId") ? "InvoiceId" : _table.Columns[0].ColumnName;

                _btnOk = new Button { Text = "Chọn", Width = 100, Location = new Point(400, 70) };
                _btnCancel = new Button { Text = "Hủy", Width = 100, Location = new Point(510, 70) };

                _btnOk.Click += (s, e) =>
                {
                    if (_cbo.SelectedItem is DataRowView drv)
                        SelectedRow = drv.Row;
                    DialogResult = SelectedRow != null ? DialogResult.OK : DialogResult.Cancel;
                };
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

                Controls.Add(_cbo);
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);
            }
        }
    }
}

