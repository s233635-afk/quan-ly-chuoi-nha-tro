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
    public class FrmPaymentManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo hóa đơn/khách/phòng/hình thức...";

        private readonly AdminDataBLL _bll;
        private readonly int? _invoiceId;
        private readonly string _invoiceNumber;
        private readonly int? _branchId;
        private HashSet<int> _allowedBranchIds;

        private DataTable _rawTable;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboMethod;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Label _lblCount;
        private Label _lblTotal;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        public FrmPaymentManager(AdminDataBLL bll, int? invoiceId = null, string invoiceNumber = null, int? branchId = null)
        {
            _bll = bll;
            _invoiceId = invoiceId;
            _invoiceNumber = invoiceNumber;
            _branchId = branchId;
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = _invoiceId.HasValue ? $"Thanh toán - {_invoiceNumber ?? _invoiceId.Value.ToString()}" : "Quản lý thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1400;
            Height = 750;
            BackColor = Color.FromArgb(240, 242, 245);
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
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();

            _txtSearch = new TextBox { Width = 280 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _txtSearch.GotFocus += (s, e) =>
            {
                if (_txtSearch.Text == SearchPlaceholder)
                {
                    _txtSearch.Text = string.Empty;
                    _txtSearch.ForeColor = Color.Black;
                }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = SearchPlaceholder;
                    _txtSearch.ForeColor = Color.Gray;
                }
            };

            _cboMethod = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboMethod.Items.AddRange(new object[] { "Tất cả", "Cash", "Transfer", "QR", "Check" });
            _cboMethod.SelectedIndex = 0;
            _cboMethod.SelectedIndexChanged += (s, e) => ApplyFilter();

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 120 };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 120 };
            _dtFrom.ValueChanged += (s, e) => ApplyFilter();
            _dtTo.ValueChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };
            _lblTotal = new Label { AutoSize = true, Text = "Tổng thu: 0", ForeColor = Color.FromArgb(70, 70, 70) };

            _btnAdd = MakeButton("Thu tiền", Color.FromArgb(46, 125, 50), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnRefresh = MakeButton("Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadDataAsync());

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
            actions.Controls.Add(_btnRefresh);

            var searchHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var pnlSearch = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Height = 34,
                Width = 320,
                Padding = new Padding(10, 7, 10, 7)
            };
            _txtSearch.BorderStyle = BorderStyle.None;
            _txtSearch.Parent = pnlSearch;
            _txtSearch.Location = new Point(2, 6);
            _txtSearch.Width = pnlSearch.Width - 16;
            pnlSearch.Resize += (s, e) => _txtSearch.Width = pnlSearch.Width - 16;
            _txtSearch.Text = SearchPlaceholder;
            _txtSearch.ForeColor = Color.Gray;

            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(0, 9), ForeColor = Color.FromArgb(70, 70, 70) };
            pnlSearch.Location = new Point(lblSearch.Right + 6, 10);

            var lblMethod = new Label { Text = "Hình thức:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblMethod.Location = new Point(pnlSearch.Right + 14, 9);
            _cboMethod.Location = new Point(lblMethod.Right + 6, 6);

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblFrom.Location = new Point(_cboMethod.Right + 14, 9);
            _dtFrom.Location = new Point(lblFrom.Right + 6, 6);

            var lblTo = new Label { Text = "Đến:", AutoSize = true, ForeColor = Color.FromArgb(70, 70, 70) };
            lblTo.Location = new Point(_dtFrom.Right + 10, 9);
            _dtTo.Location = new Point(lblTo.Right + 6, 6);

            searchHost.Controls.Add(lblSearch);
            searchHost.Controls.Add(pnlSearch);
            searchHost.Controls.Add(lblMethod);
            searchHost.Controls.Add(_cboMethod);
            searchHost.Controls.Add(lblFrom);
            searchHost.Controls.Add(_dtFrom);
            searchHost.Controls.Add(lblTo);
            searchHost.Controls.Add(_dtTo);
            searchHost.Resize += (s, e) =>
            {
                pnlSearch.Location = new Point(lblSearch.Right + 6, 10);
                lblMethod.Location = new Point(pnlSearch.Right + 14, 9);
                _cboMethod.Location = new Point(lblMethod.Right + 6, 6);
                lblFrom.Location = new Point(_cboMethod.Right + 14, 9);
                _dtFrom.Location = new Point(lblFrom.Right + 6, 6);
                lblTo.Location = new Point(_dtFrom.Right + 10, 9);
                _dtTo.Location = new Point(lblTo.Right + 6, 6);
            };

            var summary = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = Color.Transparent };
            _lblCount.Location = new Point(0, 6);
            _lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblTotal.Location = new Point(0, 28);
            summary.Controls.Add(_lblCount);
            summary.Controls.Add(_lblTotal);

            top.Controls.Add(searchHost);
            top.Controls.Add(summary);
            top.Controls.Add(actions);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BackColor };
            gridHost.Controls.Add(_grid);

            Controls.Add(gridHost);
            Controls.Add(top);

            Load += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _rawTable = _invoiceId.HasValue
                    ? await _bll.GetPaymentsByInvoiceAsync(_invoiceId.Value)
                    : await _bll.GetPaymentsViewAsync();

                if (_invoiceId.HasValue)
                    await EnrichPaymentsAsync(_rawTable);

                if (_branchId.HasValue)
                {
                    _rawTable = FilterByBranch(_rawTable, _branchId);
                }
                else
                {
                    await EnsureAllowedBranchScopeAsync();
                    _rawTable = AdminBranchScope.FilterByBranchIds(_rawTable, _allowedBranchIds);
                }

                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadDataAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private async Task EnrichPaymentsAsync(DataTable payments)
        {
            if (payments == null) return;
            if (!payments.Columns.Contains("InvoiceId")) return;
            if (payments.Columns.Contains("InvoiceNumber") && payments.Columns.Contains("TenantName") && payments.Columns.Contains("RoomNumber") && payments.Columns.Contains("BranchId"))
                return;

            var inv = await _bll.GetInvoicesViewAsync();
            var invMap = inv.AsEnumerable()
                .Where(r => inv.Columns.Contains("InvoiceId") && r["InvoiceId"] != DBNull.Value)
                .GroupBy(r => Convert.ToInt32(r["InvoiceId"]))
                .ToDictionary(g => g.Key, g => g.First());

            if (!payments.Columns.Contains("InvoiceNumber"))
                payments.Columns.Add("InvoiceNumber", typeof(string));
            if (!payments.Columns.Contains("TenantName"))
                payments.Columns.Add("TenantName", typeof(string));
            if (!payments.Columns.Contains("RoomNumber"))
                payments.Columns.Add("RoomNumber", typeof(string));
            if (!payments.Columns.Contains("BranchId"))
                payments.Columns.Add("BranchId", typeof(int));

            foreach (DataRow p in payments.Rows)
            {
                if (!int.TryParse(p["InvoiceId"]?.ToString(), out var invoiceId)) continue;
                if (!invMap.TryGetValue(invoiceId, out var ir)) continue;

                p["InvoiceNumber"] = inv.Columns.Contains("InvoiceNumber") ? ir["InvoiceNumber"]?.ToString() : invoiceId.ToString();
                p["TenantName"] = inv.Columns.Contains("TenantName") ? ir["TenantName"]?.ToString() : ir["TenantId"]?.ToString();
                p["RoomNumber"] = inv.Columns.Contains("RoomNumber") ? ir["RoomNumber"]?.ToString() : ir["RoomId"]?.ToString();
                if (inv.Columns.Contains("BranchId") && int.TryParse(ir["BranchId"]?.ToString(), out var b))
                    p["BranchId"] = b;
            }
        }

        private void ApplyGridPresentation()
        {
            SetHeader("PaymentId", "ID");
            SetHeader("InvoiceNumber", "Số HĐ");
            SetHeader("TenantName", "Khách thuê");
            SetHeader("RoomNumber", "Phòng");
            SetHeader("PaymentDate", "Ngày thu");
            SetHeader("PaymentAmount", "Số tiền");
            SetHeader("PaymentMethod", "Hình thức");
            SetHeader("TransactionReference", "Mã GD");
            SetHeader("Notes", "Ghi chú");
            SetHeader("CreatedDate", "Tạo lúc");

            HideIfExists("InvoiceId");
            HideIfExists("TenantId");
            HideIfExists("RoomId");
            HideIfExists("BranchId");

            FormatMoney("PaymentAmount");
            FormatDate("PaymentDate");
            FormatDate("CreatedDate");

            SetDisplayOrder(
                "PaymentId",
                "InvoiceNumber",
                "TenantName",
                "RoomNumber",
                "PaymentDate",
                "PaymentAmount",
                "PaymentMethod",
                "TransactionReference",
                "Notes",
                "CreatedDate"
            );
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            string method = _cboMethod.SelectedItem?.ToString();
            bool filterMethod = !string.IsNullOrWhiteSpace(method) && method != "Tất cả";

            DateTime? from = _dtFrom.Checked ? (DateTime?)_dtFrom.Value.Date : null;
            DateTime? to = _dtTo.Checked ? (DateTime?)_dtTo.Value.Date : null;

            IEnumerable<DataRow> rows = _rawTable.AsEnumerable();

            if (from.HasValue && _rawTable.Columns.Contains("PaymentDate"))
                rows = rows.Where(r => DateTime.TryParse(r["PaymentDate"]?.ToString(), out var d) && d.Date >= from.Value);

            if (to.HasValue && _rawTable.Columns.Contains("PaymentDate"))
                rows = rows.Where(r => DateTime.TryParse(r["PaymentDate"]?.ToString(), out var d) && d.Date <= to.Value);

            if (filterMethod && _rawTable.Columns.Contains("PaymentMethod"))
                rows = rows.Where(r => string.Equals(r["PaymentMethod"]?.ToString(), method, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                rows = rows.Where(r =>
                    Contains(r, "InvoiceNumber", keyword) ||
                    Contains(r, "TenantName", keyword) ||
                    Contains(r, "RoomNumber", keyword) ||
                    Contains(r, "PaymentMethod", keyword) ||
                    Contains(r, "TransactionReference", keyword) ||
                    Contains(r, "Notes", keyword) ||
                    Contains(r, "PaymentId", keyword)
                );
            }

            var filtered = _rawTable.Clone();
            foreach (var r in rows)
                filtered.ImportRow(r);

            _grid.DataSource = filtered;
            ApplyGridPresentation();

            decimal total = 0m;
            if (filtered.Columns.Contains("PaymentAmount"))
            {
                foreach (DataRow r in filtered.Rows)
                {
                    if (decimal.TryParse(r["PaymentAmount"]?.ToString(), out var v))
                        total += v;
                }
            }

            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            _lblTotal.Text = $"Tổng thu: {total:N0}";
        }

        private async Task AddNewAsync()
        {
            try
            {
                DataRow invoiceRow;
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
                    {
                        await LoadDataAsync();
                        AdminEvents.NotifyDataChanged();
                    }
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

            if (_branchId.HasValue && invTable.Columns.Contains("BranchId"))
                selectable = selectable.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var b) && b == _branchId.Value).ToList();
            else if (invTable.Columns.Contains("BranchId"))
            {
                await EnsureAllowedBranchScopeAsync();
                if (_allowedBranchIds != null && _allowedBranchIds.Count > 0)
                    selectable = selectable.Where(r => int.TryParse(r["BranchId"]?.ToString(), out var b) && _allowedBranchIds.Contains(b)).ToList();
            }

            if (selectable.Count == 0) return null;

            using (var dlg = new InvoicePickerDialog(selectable.CopyToDataTable()))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return null;
                return dlg.SelectedRow;
            }
        }

        private async Task EditSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int paymentId = row.Table.Columns.Contains("PaymentId") ? Convert.ToInt32(row["PaymentId"]) : 0;
            if (paymentId <= 0)
            {
                MessageBox.Show("Không xác định được PaymentId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var rawRow = FindRawByPaymentId(paymentId);
            if (rawRow == null || !rawRow.Table.Columns.Contains("InvoiceId"))
            {
                MessageBox.Show("Không tìm thấy dữ liệu gốc để sửa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int invoiceId = Convert.ToInt32(rawRow["InvoiceId"]);
            var invTable = await _bll.GetInvoicesViewAsync();
            var invoiceRow = invTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["InvoiceId"]) == invoiceId);
            if (invoiceRow == null)
            {
                MessageBox.Show("Không tìm thấy hóa đơn để hiển thị.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var frm = new FrmPaymentEditor(_bll, invoiceRow, rawRow))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadDataAsync();
                    AdminEvents.NotifyDataChanged();
                }
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
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EnsureAllowedBranchScopeAsync()
        {
            if (_branchId.HasValue) return;
            if (_allowedBranchIds != null && _allowedBranchIds.Count > 0) return;

            try
            {
                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                _allowedBranchIds = AdminBranchScope.GetAllowedBranchIds(branches);
            }
            catch
            {
                _allowedBranchIds = new HashSet<int>();
            }
        }

        private DataRow FindRawByPaymentId(int paymentId)
        {
            if (_rawTable == null || !_rawTable.Columns.Contains("PaymentId")) return null;
            return _rawTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["PaymentId"]) == paymentId);
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

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            if (_grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private static bool Contains(DataRow row, string column, string keywordLower)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return false;
            var v = row[column];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keywordLower);
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            return decimal.TryParse(v.ToString(), out var parsed) ? parsed : 0m;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 96,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void SetHeader(string columnName, string headerText)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].HeaderText = headerText;
        }

        private void HideIfExists(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].Visible = false;
        }

        private void FormatMoney(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "N0";
        }

        private void FormatDate(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void SetDisplayOrder(params string[] order)
        {
            int index = 0;
            foreach (var name in order)
            {
                if (_grid.Columns.Contains(name))
                {
                    _grid.Columns[name].DisplayIndex = index;
                    index++;
                }
            }
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
                ClientSize = new Size(620, 150);
                BackColor = Color.White;

                _cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 580, Location = new Point(20, 20) };
                _cbo.DataSource = _table;
                _cbo.DisplayMember = _table.Columns.Contains("InvoiceDisplay") ? "InvoiceDisplay" : _table.Columns[0].ColumnName;
                _cbo.ValueMember = _table.Columns.Contains("InvoiceId") ? "InvoiceId" : _table.Columns[0].ColumnName;

                _btnOk = new Button { Text = "Chọn", Width = 100, Height = 32, Location = new Point(400, 90) };
                _btnCancel = new Button { Text = "Hủy", Width = 100, Height = 32, Location = new Point(510, 90) };

                _btnOk.FlatStyle = FlatStyle.Flat;
                _btnOk.FlatAppearance.BorderSize = 0;
                _btnOk.BackColor = Color.FromArgb(0, 122, 204);
                _btnOk.ForeColor = Color.White;
                _btnCancel.FlatStyle = FlatStyle.Flat;
                _btnCancel.FlatAppearance.BorderSize = 1;

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

                AcceptButton = _btnOk;
                CancelButton = _btnCancel;
            }
        }
    }
}
