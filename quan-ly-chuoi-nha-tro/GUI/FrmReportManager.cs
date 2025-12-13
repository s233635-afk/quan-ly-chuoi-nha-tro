using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmReportManager : Form
    {
        private const string SearchPlaceholder = "Tìm nhanh (mọi cột)...";

        private readonly AdminDataBLL _bll;

        private ComboBox _cboSource;
        private TextBox _txtSearch;
        private DataGridView _grid;
        private Label _lblCount;
        private Button _btnExport;
        private Button _btnRefresh;

        private DataTable _raw;

        public FrmReportManager(AdminDataBLL bll)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Báo cáo & Thống kê";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 700;
            BackColor = UiKit.AppBackground;

            _cboSource = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            _cboSource.Items.AddRange(new object[]
            {
                "Hóa đơn",
                "Thanh toán",
                "Khách thuê",
                "Phòng",
                "Hợp đồng",
                "Đặt cọc",
                "Bảo trì",
                "Tài sản",
                "Thông báo",
                "Cấu hình hệ thống"
            });
            _cboSource.SelectedIndex = 0;
            _cboSource.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

            _txtSearch = new TextBox { Width = 320 };
            var pnlSearch = UiKit.MakeSearchPanel(_txtSearch, 360, SearchPlaceholder, ApplyFilter);

            _btnExport = UiKit.MakeButton("Xuất CSV", UiKit.Purple, (s, e) => ExportCsv(), 100);
            _btnRefresh = UiKit.MakeButton("Tải lại", UiKit.Primary, async (s, e) => await LoadDataAsync(), 92);

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold) };

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
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None
            };
            UiKit.StyleGrid(_grid);

            var top = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12, 10, 12, 10), BackColor = System.Drawing.Color.White };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = System.Drawing.Color.Transparent
            };
            actions.Controls.Add(_btnExport);
            actions.Controls.Add(_btnRefresh);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.Transparent };
            var lblSource = new Label { Text = "Dữ liệu:", AutoSize = true, ForeColor = UiKit.MutedText, Location = new System.Drawing.Point(0, 9) };
            _cboSource.Location = new System.Drawing.Point(lblSource.Right + 6, 6);
            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, ForeColor = UiKit.MutedText };
            lblSearch.Location = new System.Drawing.Point(_cboSource.Right + 14, 9);
            pnlSearch.Location = new System.Drawing.Point(lblSearch.Right + 6, 10);

            filter.Controls.Add(lblSource);
            filter.Controls.Add(_cboSource);
            filter.Controls.Add(lblSearch);
            filter.Controls.Add(pnlSearch);
            filter.Resize += (s, e) =>
            {
                _cboSource.Location = new System.Drawing.Point(lblSource.Right + 6, 6);
                lblSearch.Location = new System.Drawing.Point(_cboSource.Right + 14, 9);
                pnlSearch.Location = new System.Drawing.Point(lblSearch.Right + 6, 10);
            };

            var summary = new Panel { Dock = DockStyle.Left, Width = 150, BackColor = System.Drawing.Color.Transparent };
            _lblCount.Location = new System.Drawing.Point(0, 16);
            summary.Controls.Add(_lblCount);

            top.Controls.Add(filter);
            top.Controls.Add(actions);
            top.Controls.Add(summary);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);

            Load += async (s, e) => await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _raw = await LoadSelectedAsync();
                _grid.DataSource = _raw;
                _lblCount.Text = $"Tổng: {_raw?.Rows.Count ?? 0}";
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private System.Threading.Tasks.Task<DataTable> LoadSelectedAsync()
        {
            var selected = _cboSource.SelectedItem?.ToString() ?? string.Empty;
            switch (selected)
            {
                case "Hóa đơn":
                    return _bll.GetInvoicesViewAsync();
                case "Thanh toán":
                    return _bll.GetPaymentsViewAsync();
                case "Khách thuê":
                    return _bll.GetTenantsAsync();
                case "Phòng":
                    return _bll.GetRoomsAsync();
                case "Hợp đồng":
                    return _bll.GetContractsAsync();
                case "Đặt cọc":
                    return _bll.GetDepositsAsync();
                case "Bảo trì":
                    return _bll.GetMaintenanceAsync();
                case "Tài sản":
                    return _bll.GetAssetsAsync();
                case "Thông báo":
                    return _bll.GetNotificationsAsync();
                case "Cấu hình hệ thống":
                    return _bll.GetSystemSettingsAsync();
                default:
                    return _bll.GetInvoicesViewAsync();
            }
        }

        private void ApplyFilter()
        {
            if (_raw == null)
                return;

            string raw = (_txtSearch.Text ?? string.Empty).Trim();
            if (raw == SearchPlaceholder) raw = string.Empty;
            string keyword = raw.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _grid.DataSource = _raw;
                _lblCount.Text = $"Tổng: {_raw.Rows.Count}";
                return;
            }

            var filtered = _raw.Clone();
            foreach (DataRow row in _raw.Rows)
            {
                if (RowContains(row, keyword))
                    filtered.ImportRow(row);
            }

            _grid.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private static bool RowContains(DataRow row, string keyword)
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                var v = row[c];
                if (v == null || v == DBNull.Value) continue;
                if (v.ToString().ToLowerInvariant().Contains(keyword))
                    return true;
            }
            return false;
        }

        private void ExportCsv()
        {
            var dt = _grid.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    var cols = dt.Columns.Cast<DataColumn>().ToList();
                    sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(c.ColumnName))));
                    foreach (DataRow r in dt.Rows)
                    {
                        sb.AppendLine(string.Join(",", cols.Select(c => EscapeCsv(r[c]?.ToString()))));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                    MessageBox.Show("Đã xuất: " + sfd.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains("\""))
                value = value.Replace("\"", "\"\"");
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
                value = "\"" + value + "\"";
            return value;
        }
    }
}

