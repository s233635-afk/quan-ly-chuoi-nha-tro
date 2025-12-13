using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmInvoiceManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo hóa đơn/khách/phòng...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _branchId;
        private DataTable _table;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private Label _lblCount;
        private Label _lblSummary;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnPay, _btnPayments, _btnGenerate, _btnExport, _btnRefresh;

        public FrmInvoiceManager() : this(null)
        {
        }

        public FrmInvoiceManager(int? branchId)
        {
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Hóa đơn & Thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1300;
            Height = 700;
            BackColor = UiKit.AppBackground;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _grid.DoubleClick += (s, e) => EditSelected();
            _grid.SelectionChanged += (s, e) => UpdateSummary();
            UiKit.StyleGrid(_grid);

            _txtSearch = new TextBox { Width = 280 };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 320, SearchPlaceholder, ApplyFilter);

            _cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Issued", "PartialPaid", "Paid", "Overdue" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };
            _lblSummary = new Label { AutoSize = true, Text = "Tổng tiền: 0 | Đã thu: 0 | Còn nợ: 0", ForeColor = UiKit.MutedText };

            _btnAdd = UiKit.MakeButton("Thêm", UiKit.Primary, (s, e) => AddNew(), 92);
            _btnEdit = UiKit.MakeButton("Sửa", UiKit.Primary, (s, e) => EditSelected(), 92);
            _btnDelete = UiKit.MakeButton("Xóa", UiKit.Danger, async (s, e) => await DeleteSelectedAsync(), 92);
            _btnPay = UiKit.MakeButton("Thu tiền", UiKit.Success, (s, e) => PaySelected(), 100);
            _btnPayments = UiKit.MakeButton("DS thanh toán", UiKit.Primary, (s, e) => ShowPayments(), 120);
            _btnGenerate = UiKit.MakeButton("Tạo hóa đơn tháng", UiKit.Warning, async (s, e) => await GenerateMonthlyAsync(), 150);
            _btnExport = UiKit.MakeButton("Xuất CSV", UiKit.Purple, (s, e) => ExportCsv(), 100);
            _btnRefresh = UiKit.MakeButton("Tải lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 92);

            var top = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12, 10, 12, 10), BackColor = Color.White };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnAdd);
            actions.Controls.Add(_btnEdit);
            actions.Controls.Add(_btnDelete);
            actions.Controls.Add(_btnPay);
            actions.Controls.Add(_btnPayments);
            actions.Controls.Add(_btnGenerate);
            actions.Controls.Add(_btnExport);
            actions.Controls.Add(_btnRefresh);

            var filterHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(0, 9), ForeColor = UiKit.MutedText };
            pnlSearch.Location = new Point(lblSearch.Right + 6, 10);

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = UiKit.MutedText };
            lblStatus.Location = new Point(pnlSearch.Right + 14, 9);
            _cboStatus.Location = new Point(lblStatus.Right + 6, 6);

            filterHost.Controls.Add(lblSearch);
            filterHost.Controls.Add(pnlSearch);
            filterHost.Controls.Add(lblStatus);
            filterHost.Controls.Add(_cboStatus);
            filterHost.Resize += (s, e) =>
            {
                pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
                lblStatus.Location = new Point(pnlSearch.Right + 14, 9);
                _cboStatus.Location = new Point(lblStatus.Right + 6, 6);
            };

            var summary = new Panel { Dock = DockStyle.Right, Width = 420, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 6);
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblSummary.Location = new Point(0, 28);
            summary.Controls.Add(_lblCount);
            summary.Controls.Add(_lblSummary);

            top.Controls.Add(filterHost);
            top.Controls.Add(summary);
            top.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);
            Load += async (s, e) => await LoadDataAsync();
        }

        private static void Place(Control ctl, Control parent, ref int x, int y)
        {
            ctl.Location = new Point(x, y);
            parent.Controls.Add(ctl);
            x += ctl.Width + 8;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _table = await _bll.GetInvoicesViewAsync();
                _table = FilterByBranch(_table, _branchId);
                _grid.DataSource = _table;
                ApplyFilter();
                AutoFormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataTable FilterByBranch(DataTable dt, int? branchId)
        {
            if (dt == null) return dt;
            if (!branchId.HasValue) return dt;
            if (!dt.Columns.Contains("BranchId")) return dt;

            var filtered = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (int.TryParse(r["BranchId"]?.ToString(), out var b) && b == branchId.Value)
                    filtered.ImportRow(r);
            }
            return filtered;
        }

        private void AutoFormatGrid()
        {
            if (_grid.Columns.Contains("TotalAmount"))
                _grid.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
            if (_grid.Columns.Contains("PaidAmount"))
                _grid.Columns["PaidAmount"].DefaultCellStyle.Format = "N0";
            if (_grid.Columns.Contains("RemainingAmount"))
                _grid.Columns["RemainingAmount"].DefaultCellStyle.Format = "N0";
        }

        private void ApplyFilter()
        {
            if (_table == null) return;
            var raw = (_txtSearch.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            var keyword = raw.Replace("'", "''");
            var status = _cboStatus.SelectedItem?.ToString();

            var filters = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kwFilters = new System.Collections.Generic.List<string>();
                if (_table.Columns.Contains("InvoiceNumber")) kwFilters.Add($"InvoiceNumber LIKE '%{keyword}%'");
                if (_table.Columns.Contains("TenantName")) kwFilters.Add($"TenantName LIKE '%{keyword}%'");
                if (_table.Columns.Contains("RoomNumber")) kwFilters.Add($"RoomNumber LIKE '%{keyword}%'");
                if (_table.Columns.Contains("InvoiceId")) kwFilters.Add($"Convert(InvoiceId, 'System.String') LIKE '%{keyword}%'");
                if (_table.Columns.Contains("TenantId")) kwFilters.Add($"Convert(TenantId, 'System.String') LIKE '%{keyword}%'");
                if (_table.Columns.Contains("RoomId")) kwFilters.Add($"Convert(RoomId, 'System.String') LIKE '%{keyword}%'");
                if (kwFilters.Count > 0) filters.Add("(" + string.Join(" OR ", kwFilters) + ")");
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Tất cả" && _table.Columns.Contains("Status"))
            {
                string escapedStatus = status.Replace("'", "''");
                filters.Add("Status = '" + escapedStatus + "'");
            }

            _table.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : string.Empty;
            _lblCount.Text = $"Tổng: {_table.DefaultView.Count}";
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (_table == null) return;
            decimal total = 0m, paid = 0m, remaining = 0m;
            foreach (DataRowView view in _table.DefaultView)
            {
                var row = view.Row;
                total += ReadDecimal(row, "TotalAmount");
                paid += ReadDecimal(row, "PaidAmount");
                remaining += ReadDecimal(row, "RemainingAmount");
            }

            _lblSummary.Text = $"Tổng tiền: {total:N0} | Đã thu: {paid:N0} | Còn nợ: {remaining:N0}";
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

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }

        private void AddNew()
        {
            using (var frm = new FrmInvoiceEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    _ = LoadDataAsync();
            }
        }

        private void EditSelected()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmInvoiceEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    _ = LoadDataAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            decimal paid = ReadDecimal(row, "PaidAmount");
            bool hasPayment = paid > 0;

            string msg = hasPayment
                ? $"Hóa đơn ID {invoiceId} đã có thanh toán. Xóa cả lịch sử thanh toán?"
                : $"Xóa hóa đơn ID {invoiceId}?";

            var confirm = MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                await _bll.DeleteInvoiceAsync(invoiceId, deletePaymentsFirst: hasPayment);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PaySelected()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (remaining <= 0)
            {
                MessageBox.Show("Hóa đơn đã thanh toán đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmPaymentEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    _ = LoadDataAsync();
            }
        }

        private void ShowPayments()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                using (var frm = new FrmPaymentManager(_bll))
                    frm.ShowDialog(this);
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            string invoiceNo = row.Table.Columns.Contains("InvoiceNumber") ? row["InvoiceNumber"]?.ToString() : invoiceId.ToString();
            using (var frm = new FrmPaymentManager(_bll, invoiceId, invoiceNo))
                frm.ShowDialog(this);
        }

        private async System.Threading.Tasks.Task GenerateMonthlyAsync()
        {
            using (var dlg = new MonthlyInvoiceDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    int created = await _bll.GenerateMonthlyInvoicesAsync(dlg.SelectedYear, dlg.SelectedMonth, DateTime.Today, dlg.DueDay);
                    await LoadDataAsync();
                    MessageBox.Show($"Đã tạo {created} hóa đơn cho {dlg.SelectedMonth:00}/{dlg.SelectedYear}.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tạo hóa đơn tháng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportCsv()
        {
            if (_table == null || _table.DefaultView.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                OverwritePrompt = true
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    WriteCsvFromView(_table.DefaultView, sfd.FileName);
                    MessageBox.Show("Đã xuất CSV: " + sfd.FileName, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void WriteCsvFromView(DataView view, string path)
        {
            var sb = new StringBuilder();
            var cols = view.Table.Columns.Cast<DataColumn>().ToArray();

            sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(c.ColumnName))));
            foreach (DataRowView rv in view)
            {
                sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(rv.Row[c]))));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(object value)
        {
            string s = value == null || value == DBNull.Value ? "" : value.ToString();
            bool needQuote = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
            s = s.Replace("\"", "\"\"");
            return needQuote ? "\"" + s + "\"" : s;
        }

        private class MonthlyInvoiceDialog : Form
        {
            private NumericUpDown _numYear;
            private NumericUpDown _numMonth;
            private NumericUpDown _numDueDay;
            private Button _btnOk;
            private Button _btnCancel;

            public int SelectedYear => (int)_numYear.Value;
            public int SelectedMonth => (int)_numMonth.Value;
            public int? DueDay => _numDueDay.Value > 0 ? (int?)_numDueDay.Value : null;

            public MonthlyInvoiceDialog()
            {
                Text = "Tạo hóa đơn tháng";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(360, 180);

                var now = DateTime.Today;
                _numYear = new NumericUpDown { Minimum = 2000, Maximum = 2100, Value = now.Year, Location = new Point(130, 20), Width = 180 };
                _numMonth = new NumericUpDown { Minimum = 1, Maximum = 12, Value = now.Month, Location = new Point(130, 55), Width = 180 };
                _numDueDay = new NumericUpDown { Minimum = 0, Maximum = 31, Value = 10, Location = new Point(130, 90), Width = 180 };

                Controls.Add(new Label { Text = "Năm", AutoSize = true, Location = new Point(20, 24) });
                Controls.Add(_numYear);
                Controls.Add(new Label { Text = "Tháng", AutoSize = true, Location = new Point(20, 59) });
                Controls.Add(_numMonth);
                Controls.Add(new Label { Text = "Hạn (ngày, 0=auto)", AutoSize = true, Location = new Point(20, 94) });
                Controls.Add(_numDueDay);

                _btnOk = new Button { Text = "Tạo", Width = 100, Location = new Point(130, 130) };
                _btnCancel = new Button { Text = "Hủy", Width = 100, Location = new Point(240, 130) };
                _btnOk.Click += (s, e) => DialogResult = DialogResult.OK;
                _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);
            }
        }
    }
}
