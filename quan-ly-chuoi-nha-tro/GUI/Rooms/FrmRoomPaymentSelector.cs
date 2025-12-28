using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomPaymentSelector : Form
    {
        private readonly AdminDataBLL _bll;
        private DataTable _invoices;

        private DataGridView _grid;
        private Button _btnPay;
        private Button _btnClose;

        public FrmRoomPaymentSelector(AdminDataBLL bll)
        {
            _bll = bll;
            InitializeComponent();
            Load += async (s, e) => await LoadInvoicesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Thu tiền từng phòng";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(920, 500);
            BackColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 10F);

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

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(12),
                BackColor = Color.White
            };

            _btnPay = new Button
            {
                Text = "Thu tiền phòng",
                Width = 140,
                Height = 34,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnPay.FlatAppearance.BorderSize = 0;
            _btnPay.Click += async (s, e) => await PaySelectedAsync();

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            _btnClose.FlatAppearance.BorderSize = 0;

            footer.Controls.Add(_btnPay);
            footer.Controls.Add(_btnClose);
            _btnPay.Location = new Point(footer.Width - 260, 12);
            _btnClose.Location = new Point(footer.Width - 120, 12);
            footer.Resize += (s, e) =>
            {
                _btnPay.Location = new Point(footer.ClientSize.Width - 260, 12);
                _btnClose.Location = new Point(footer.ClientSize.Width - 120, 12);
            };

            Controls.Add(_grid);
            Controls.Add(footer);
        }

        private async Task LoadInvoicesAsync()
        {
            try
            {
                var table = await _bll.GetInvoicesViewAsync();
                var outstanding = table.AsEnumerable()
                    .Where(r => ReadDecimal(r, "RemainingAmount") > 0);

                _invoices = outstanding.Any() ? outstanding.CopyToDataTable() : table.Clone();
                _grid.DataSource = _invoices;
                ApplyGridPresentation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyGridPresentation()
        {
            if (_grid.Columns.Contains("InvoiceNumber")) _grid.Columns["InvoiceNumber"].HeaderText = "Mã hóa đơn";
            if (_grid.Columns.Contains("TenantName")) _grid.Columns["TenantName"].HeaderText = "Khách thuê";
            if (_grid.Columns.Contains("RoomNumber")) _grid.Columns["RoomNumber"].HeaderText = "Phòng";
            if (_grid.Columns.Contains("RemainingAmount"))
            {
                _grid.Columns["RemainingAmount"].HeaderText = "Còn nợ";
                _grid.Columns["RemainingAmount"].DefaultCellStyle.Format = "N0";
            }

            foreach (var name in new[] { "InvoiceId", "TenantId", "RoomId", "BranchId" })
                if (_grid.Columns.Contains(name))
                    _grid.Columns[name].Visible = false;
        }

        private async Task PaySelectedAsync()
        {
            var row = GetSelectedRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hóa đơn để thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal remaining = ReadDecimal(row, "RemainingAmount");
            if (remaining <= 0)
            {
                MessageBox.Show("Hóa đơn này đã được thanh toán đầy đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadInvoicesAsync();
                    AdminEvents.NotifyDataChanged();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private DataRow GetSelectedRow()
        {
            if (_grid.CurrentRow?.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private static decimal ReadDecimal(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return 0m;
            var value = row[column];
            if (value == null || value == DBNull.Value) return 0m;
            if (value is decimal dec) return dec;
            return decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0m;
        }
    }
}
