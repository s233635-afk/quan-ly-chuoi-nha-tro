using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// FrmInvoiceManagerModern - Quản lý hóa đơn hiện đại
    /// UI đẹp, tính năng đầy đủ: lọc nâng cao, thống kê, thanh toán nhanh
    /// </summary>
    public class FrmInvoiceManagerModern : Form
    {
        private AdminDataBLL _bll;
        private DataTable _rawTable;

        private Panel _headerPanel;
        private Panel _toolbarPanel;
        private Panel _filterPanel;
        private Panel _statsPanel;
        private ModernDataGridView _grid;

        private TextBox _txtSearch;
        private ComboBox _cboBranch;
        private ComboBox _cboStatus;
        private ComboBox _cboMonth;
        private ComboBox _cboYear;

        private Label _lblTotalInvoices;
        private Label _lblTotalAmount;
        private Label _lblPaid;
        private Label _lblUnpaid;

        private ModernButton _btnAdd;
        private ModernButton _btnEdit;
        private ModernButton _btnDelete;
        private ModernButton _btnRefresh;
        private ModernButton _btnQuickPay;
        private ModernButton _btnPrintInvoice;

        public FrmInvoiceManagerModern(AdminDataBLL bll)
        {
            _bll = bll;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý hóa đơn";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1600;
            Height = 900;
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;

            // ============ HEADER PANEL ============
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ModernTheme.Colors.Info,
                Padding = new Padding(ModernTheme.Spacing.LG)
            };

            var headerTitle = new Label
            {
                Text = "🧾 QUẢN LÝ HÓA ĐƠN",
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Heading2),
                ForeColor = ModernTheme.Colors.TextInverse,
                AutoSize = true,
                Dock = DockStyle.Left
            };

            var headerDesc = new Label
            {
                Text = "Quản lý tất cả hóa đơn, theo dõi thanh toán và công nợ",
                Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Small),
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize = true,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.BottomLeft
            };

            _headerPanel.Controls.Add(headerDesc);
            _headerPanel.Controls.Add(headerTitle);

            // ============ TOOLBAR PANEL ============
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            _btnAdd = CreateToolButton("➕ Tạo hóa đơn", ModernTheme.Colors.Success);
            _btnAdd.Click += async (s, e) => await AddNewInvoiceAsync();

            _btnEdit = CreateToolButton("✏️ Sửa", ModernTheme.Colors.Primary);
            _btnEdit.Click += async (s, e) => await EditSelectedInvoiceAsync();

            _btnDelete = CreateToolButton("🗑️ Xóa", ModernTheme.Colors.Error);
            _btnDelete.Click += async (s, e) => await DeleteSelectedInvoiceAsync();

            _btnRefresh = CreateToolButton("🔄 Tải lại", ModernTheme.Colors.Secondary);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _btnQuickPay = CreateToolButton("💰 Thanh toán", Color.FromArgb(76, 175, 80));
            _btnQuickPay.Click += async (s, e) => await QuickPayAsync();

            _btnPrintInvoice = CreateToolButton("🖨️ In", Color.FromArgb(127, 127, 127));
            _btnPrintInvoice.Click += (s, e) => PrintInvoice();

            var toolActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Background
            };

            toolActions.Controls.Add(_btnAdd);
            toolActions.Controls.Add(_btnEdit);
            toolActions.Controls.Add(_btnDelete);
            toolActions.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            toolActions.Controls.Add(_btnRefresh);
            toolActions.Controls.Add(_btnQuickPay);
            toolActions.Controls.Add(_btnPrintInvoice);

            _toolbarPanel.Controls.Add(toolActions);

            // ============ FILTER PANEL ============
            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = ModernTheme.Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var lblSearch = new Label { Text = "🔍 Tìm:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _txtSearch = new TextBox { Width = 200, Height = 32 };
            ModernTheme.StyleTextBox(_txtSearch);
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            var lblBranch = new Label { Text = "Chi nhánh:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboBranch = new ComboBox { Width = 150, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboBranch);
            _cboBranch.Items.Add("Tất cả");
            _cboBranch.SelectedIndex = 0;
            _cboBranch.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboStatus = new ComboBox { Width = 150, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboStatus);
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Đã thanh toán", "Còn nợ", "Quá hạn" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblMonth = new Label { Text = "Tháng:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboMonth = new ComboBox { Width = 80, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboMonth);
            for (int i = 1; i <= 12; i++)
                _cboMonth.Items.Add(i.ToString());
            _cboMonth.SelectedIndex = DateTime.Now.Month - 1;
            _cboMonth.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblYear = new Label { Text = "Năm:", AutoSize = true, ForeColor = ModernTheme.Colors.TextPrimary };
            _cboYear = new ComboBox { Width = 100, Height = 32, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cboYear);
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 2; i <= currentYear + 1; i++)
                _cboYear.Items.Add(i.ToString());
            _cboYear.SelectedItem = currentYear.ToString();
            _cboYear.SelectedIndexChanged += (s, e) => ApplyFilter();

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.Surface
            };

            filterFlow.Controls.Add(lblSearch);
            filterFlow.Controls.Add(_txtSearch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblBranch);
            filterFlow.Controls.Add(_cboBranch);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblStatus);
            filterFlow.Controls.Add(_cboStatus);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.MD, AutoSize = false });
            filterFlow.Controls.Add(lblMonth);
            filterFlow.Controls.Add(_cboMonth);
            filterFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.SM, AutoSize = false });
            filterFlow.Controls.Add(lblYear);
            filterFlow.Controls.Add(_cboYear);

            _filterPanel.Controls.Add(filterFlow);

            // ============ STATS PANEL ============
            _statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ModernTheme.Colors.WarningLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(ModernTheme.Spacing.MD)
            };

            var statFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ModernTheme.Colors.WarningLight
            };

            _lblTotalInvoices = CreateStatLabel("📊 Tổng hóa đơn: 0", ModernTheme.Colors.Primary);
            _lblTotalAmount = CreateStatLabel("💰 Tổng tiền: 0 VNĐ", ModernTheme.Colors.Warning);
            _lblPaid = CreateStatLabel("✅ Đã thanh toán: 0 VNĐ", ModernTheme.Colors.Success);
            _lblUnpaid = CreateStatLabel("❌ Còn nợ: 0 VNĐ", ModernTheme.Colors.Error);

            statFlow.Controls.Add(_lblTotalInvoices);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblTotalAmount);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblPaid);
            statFlow.Controls.Add(new Label { Width = ModernTheme.Spacing.LG, AutoSize = false });
            statFlow.Controls.Add(_lblUnpaid);

            _statsPanel.Controls.Add(statFlow);

            // ============ GRID ============
            _grid = new ModernDataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = ModernTheme.Colors.Background,
                BorderStyle = BorderStyle.None
            };
            _grid.DoubleClick += async (s, e) => await EditSelectedInvoiceAsync();

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(ModernTheme.Spacing.MD), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            // ============ ASSEMBLE FORM ============
            Controls.Add(gridHost);
            Controls.Add(_statsPanel);
            Controls.Add(_filterPanel);
            Controls.Add(_toolbarPanel);
            Controls.Add(_headerPanel);

            Load += async (s, e) => await LoadDataAsync();
        }

        private ModernButton CreateToolButton(string text, Color bgColor)
        {
            var btn = new ModernButton
            {
                Text = text,
                BackColor = bgColor,
                ForeColor = ModernTheme.Colors.TextInverse,
                Width = 120,
                Height = 36,
                Margin = new Padding(ModernTheme.Spacing.SM)
            };
            ModernTheme.StyleButton(btn, bgColor);
            return btn;
        }

        private Label CreateStatLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                ForeColor = color,
                AutoSize = true
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _grid.DataSource = null;
                var data = await _bll.GetInvoiceListAsync();
                _rawTable = data ?? new DataTable();

                if (_grid.Columns.Count == 0)
                {
                    _grid.DataSource = _rawTable;
                    ConfigureGridColumns();
                }
                else
                {
                    _grid.DataSource = _rawTable;
                }

                LoadFilterData();
                ApplyFilter();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilterData()
        {
            try
            {
                var branches = _rawTable?.DefaultView?.ToTable(true, new[] { "BranchName" })?.Rows;
                if (branches != null && branches.Count > 0)
                {
                    _cboBranch.Items.Clear();
                    _cboBranch.Items.Add("Tất cả");
                    foreach (DataRow row in branches)
                    {
                        _cboBranch.Items.Add(row["BranchName"]);
                    }
                    _cboBranch.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void ConfigureGridColumns()
        {
            if (_grid.Columns.Count == 0) return;

            var columnsToHide = new[] { "BranchId", "Notes" };
            var columnWidths = new Dictionary<string, int>
            {
                { "InvoiceId", 70 },
                { "InvoiceNumber", 120 },
                { "BranchName", 120 },
                { "TenantName", 130 },
                { "RoomNumber", 80 },
                { "TotalAmount", 100 },
                { "PaidAmount", 100 },
                { "RemainingAmount", 100 },
                { "Status", 100 },
                { "InvoiceDate", 120 },
                { "DueDate", 120 }
            };

            foreach (DataGridViewColumn col in _grid.Columns)
            {
                if (columnsToHide.Contains(col.Name))
                    col.Visible = false;

                if (columnWidths.ContainsKey(col.Name))
                    col.Width = columnWidths[col.Name];
            }
        }

        private void ApplyFilter()
        {
            if (_rawTable == null || _rawTable.Rows.Count == 0)
            {
                _grid.DataSource = _rawTable;
                return;
            }

            var filtered = _rawTable.Copy();
            var query = filtered.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
            {
                string searchTerm = _txtSearch.Text.ToLower();
                query = query.Where(r =>
                    r["InvoiceNumber"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["BranchName"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["TenantName"]?.ToString()?.ToLower()?.Contains(searchTerm) == true ||
                    r["RoomNumber"]?.ToString()?.ToLower()?.Contains(searchTerm) == true
                );
            }

            if (_cboBranch.SelectedIndex > 0)
            {
                string branch = _cboBranch.SelectedItem.ToString();
                query = query.Where(r => r["BranchName"]?.ToString() == branch);
            }

            if (_cboStatus.SelectedIndex > 0)
            {
                string status = _cboStatus.SelectedItem.ToString();
                query = query.Where(r => r["Status"]?.ToString() == status);
            }

            _grid.DataSource = query.CopyToDataTable();
            UpdateStats();
        }

        private void UpdateStats()
        {
            try
            {
                var dt = _grid.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    _lblTotalInvoices.Text = "📊 Tổng hóa đơn: 0";
                    _lblTotalAmount.Text = "💰 Tổng tiền: 0 VNĐ";
                    _lblPaid.Text = "✅ Đã thanh toán: 0 VNĐ";
                    _lblUnpaid.Text = "❌ Còn nợ: 0 VNĐ";
                    return;
                }

                int total = dt.Rows.Count;
                decimal totalAmount = 0, paidAmount = 0, remainingAmount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (decimal.TryParse(row["TotalAmount"]?.ToString(), out decimal amt))
                        totalAmount += amt;
                    if (decimal.TryParse(row["PaidAmount"]?.ToString(), out decimal paid))
                        paidAmount += paid;
                    if (decimal.TryParse(row["RemainingAmount"]?.ToString(), out decimal remaining))
                        remainingAmount += remaining;
                }

                _lblTotalInvoices.Text = $"📊 Tổng hóa đơn: {total}";
                _lblTotalAmount.Text = $"💰 Tổng tiền: {totalAmount:N0} VNĐ";
                _lblPaid.Text = $"✅ Đã thanh toán: {paidAmount:N0} VNĐ";
                _lblUnpaid.Text = $"❌ Còn nợ: {remainingAmount:N0} VNĐ";
            }
            catch { }
        }

        private async Task AddNewInvoiceAsync()
        {
            using (var form = new FrmInvoiceEditor(_bll, null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async Task EditSelectedInvoiceAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int invoiceId = Convert.ToInt32(_grid.SelectedRows[0].Cells["InvoiceId"].Value);
            using (var form = new FrmInvoiceEditor(_bll, invoiceId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async Task DeleteSelectedInvoiceAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn xóa hóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int invoiceId = Convert.ToInt32(_grid.SelectedRows[0].Cells["InvoiceId"].Value);
            try
            {
                await _bll.DeleteInvoiceAsync(invoiceId);
                MessageBox.Show("Xóa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task QuickPayAsync()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int invoiceId = Convert.ToInt32(_grid.SelectedRows[0].Cells["InvoiceId"].Value);
            decimal remainingAmount = Convert.ToDecimal(_grid.SelectedRows[0].Cells["RemainingAmount"].Value);

            using (var form = new FrmPaymentEditor(_bll, invoiceId, null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private void PrintInvoice()
        {
            MessageBox.Show("Tính năng in hóa đơn sắp được bổ sung.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
