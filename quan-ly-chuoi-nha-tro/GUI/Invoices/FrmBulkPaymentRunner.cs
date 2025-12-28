using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmBulkPaymentRunner : Form
    {
        private readonly AdminDataBLL _bll;
        private DataTable _invoices;

        private DataGridView _grid;
        private Button _btnProcess;
        private Button _btnClose;

        public FrmBulkPaymentRunner(AdminDataBLL bll)
        {
            _bll = bll;
            InitializeComponent();
            Load += async (s, e) => await LoadInvoicesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Thu tiền tất cả";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1000, 540);
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
                Height = 60,
                Padding = new Padding(12),
                BackColor = Color.White
            };

            _btnProcess = new Button
            {
                Text = "Bắt đầu thu tiền",
                Width = 160,
                Height = 36,
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnProcess.FlatAppearance.BorderSize = 0;
            _btnProcess.Click += async (s, e) => await ProcessAllAsync();

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            _btnClose.FlatAppearance.BorderSize = 0;

            footer.Controls.Add(_btnProcess);
            footer.Controls.Add(_btnClose);
            _btnProcess.Location = new Point(footer.ClientSize.Width - 280, 12);
            _btnClose.Location = new Point(footer.ClientSize.Width - 120, 12);
            footer.Resize += (s, e) =>
            {
                _btnProcess.Location = new Point(footer.ClientSize.Width - 280, 12);
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
                var remaining = table.AsEnumerable()
                    .Where(r => ReadDecimal(r, "RemainingAmount") > 0);
                _invoices = remaining.Any() ? remaining.CopyToDataTable() : table.Clone();
                _grid.DataSource = _invoices;
                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
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
            {
                if (_grid.Columns.Contains(name))
                    _grid.Columns[name].Visible = false;
            }
        }

        private async Task ProcessAllAsync()
        {
            if (_invoices == null || _invoices.Rows.Count == 0)
            {
                MessageBox.Show("Không có hóa đơn nào cần thu tiền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var rows = _invoices.AsEnumerable().ToList();
            foreach (var row in rows)
            {
                decimal remaining = ReadDecimal(row, "RemainingAmount");
                if (remaining <= 0) continue;

                using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
                {
                    var result = frm.ShowDialog(this);
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show("Đã dừng lại theo yêu cầu.", "Tạm dừng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                }
            }

            await LoadInvoicesAsync();
            AdminEvents.NotifyDataChanged();
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
