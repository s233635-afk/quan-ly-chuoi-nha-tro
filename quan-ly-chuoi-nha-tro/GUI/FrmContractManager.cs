using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmContractManager : Form
    {
        private readonly AdminDataBLL _bll = new AdminDataBLL();
        private readonly int? _branchId;
        private HashSet<int> _allowedBranchIds;

        private DataTable _rawTable;

        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private DateTimePicker _dtFrom;
        private DateTimePicker _dtTo;
        private Label _lblCount;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnMarkDone;
        private Button _btnViewPdf;
        private Button _btnRefresh;

        public FrmContractManager() : this(null)
        {
        }

        public FrmContractManager(int? branchId)
        {
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý Hợp đồng";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1200;
            this.Height = 650;

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

            _txtSearch = new TextBox { Width = 260 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _cboStatus = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboStatus.Items.AddRange(new object[] { "Tất cả", "Active", "Extended", "Terminated", "Expired" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 120 };
            _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 120 };
            _dtFrom.ValueChanged += (s, e) => ApplyFilter();
            _dtTo.ValueChanged += (s, e) => ApplyFilter();

            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0" };

            _btnAdd = MakeButton("Thêm", async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("Sửa", async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("Xóa", async (s, e) => await DeleteSelectedAsync());
            _btnMarkDone = MakeButton("Đã xong", async (s, e) => await MarkDoneAsync());
            _btnMarkDone.BackColor = Color.FromArgb(46, 125, 50);
            _btnViewPdf = MakeButton("Xem PDF", (s, e) => ViewPdf());
            _btnRefresh = MakeButton("Tải lại", async (s, e) => await LoadDataAsync());

            var top = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(10, 10, 10, 10), BackColor = Color.White };

            int x = 10;
            Place(top, _btnAdd, ref x);
            Place(top, _btnEdit, ref x);
            Place(top, _btnDelete, ref x);
            Place(top, _btnMarkDone, ref x);
            Place(top, _btnViewPdf, ref x);
            Place(top, _btnRefresh, ref x);

            var lblSearch = new Label { Text = "Tìm:", AutoSize = true, Location = new Point(x + 10, 16) };
            top.Controls.Add(lblSearch);
            _txtSearch.Location = new Point(lblSearch.Right + 6, 12);
            top.Controls.Add(_txtSearch);

            var lblStatus = new Label { Text = "Trạng thái:", AutoSize = true, Location = new Point(_txtSearch.Right + 12, 16) };
            top.Controls.Add(lblStatus);
            _cboStatus.Location = new Point(lblStatus.Right + 6, 12);
            top.Controls.Add(_cboStatus);

            var lblFrom = new Label { Text = "Từ:", AutoSize = true, Location = new Point(_cboStatus.Right + 12, 16) };
            top.Controls.Add(lblFrom);
            _dtFrom.Location = new Point(lblFrom.Right + 6, 12);
            top.Controls.Add(_dtFrom);

            var lblTo = new Label { Text = "Đến:", AutoSize = true, Location = new Point(_dtFrom.Right + 10, 16) };
            top.Controls.Add(lblTo);
            _dtTo.Location = new Point(lblTo.Right + 6, 12);
            top.Controls.Add(_dtTo);

            _lblCount.Location = new Point(_dtTo.Right + 14, 16);
            top.Controls.Add(_lblCount);

            var hint = new Label
            {
                Text = "Mẹo: double-click để sửa",
                AutoSize = true,
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            top.Controls.Add(hint);
            hint.Location = new Point(this.Width - 210, 16);
            top.Resize += (s, e) =>
            {
                hint.Location = new Point(top.ClientSize.Width - hint.Width - 10, 16);
            };

            this.Controls.Add(_grid);
            this.Controls.Add(top);

            this.Load += async (s, e) => await LoadDataAsync();
        }

        private static Button MakeButton(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 92,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private static void Place(Control parent, Control control, ref int x)
        {
            control.Location = new Point(x, 10);
            parent.Controls.Add(control);
            x += control.Width + 8;
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                await EnsureAllowedBranchScopeAsync();
                _rawTable = await _bll.GetContractsAsync();
                _rawTable = _branchId.HasValue ? FilterByBranch(_rawTable, _branchId) : AdminBranchScope.FilterByBranchIds(_rawTable, _allowedBranchIds);

                await EnrichContractsAsync(_rawTable);
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();

                ApplyFilter();
            }
            catch (Exception ex)
            {
                _rawTable = new DataTable();
                _grid.DataSource = _rawTable;
                ApplyFilter();
                MessageBox.Show("Không kết nối được CSDL để tải hợp đồng.\n\nChi tiết: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task EnrichContractsAsync(DataTable contracts)
        {
            if (contracts == null) return;

            DataTable tenants = null;
            DataTable rooms = null;
            try
            {
                tenants = await _bll.GetTenantsAsync();
                rooms = await _bll.GetRoomsAsync();
                rooms = _branchId.HasValue ? FilterByBranch(rooms, _branchId) : AdminBranchScope.FilterByBranchIds(rooms, _allowedBranchIds);
            }
            catch
            {
                return;
            }

            var tenantMap = new Dictionary<int, string>();
            if (tenants != null && tenants.Columns.Contains("TenantId"))
            {
                foreach (DataRow r in tenants.Rows)
                {
                    if (int.TryParse(r["TenantId"]?.ToString(), out var id))
                    {
                        string name = tenants.Columns.Contains("FullName") ? r["FullName"]?.ToString() : ("Tenant " + id);
                        if (!tenantMap.ContainsKey(id)) tenantMap.Add(id, name);
                    }
                }
            }

            var roomMap = new Dictionary<int, string>();
            if (rooms != null && rooms.Columns.Contains("RoomId"))
            {
                foreach (DataRow r in rooms.Rows)
                {
                    if (int.TryParse(r["RoomId"]?.ToString(), out var id))
                    {
                        string num = rooms.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : ("Phòng " + id);
                        if (!roomMap.ContainsKey(id)) roomMap.Add(id, num);
                    }
                }
            }

            if (!contracts.Columns.Contains("TenantName"))
                contracts.Columns.Add("TenantName", typeof(string));
            if (!contracts.Columns.Contains("RoomNumber"))
                contracts.Columns.Add("RoomNumber", typeof(string));

            foreach (DataRow r in contracts.Rows)
            {
                if (contracts.Columns.Contains("TenantId") && int.TryParse(r["TenantId"]?.ToString(), out var tid) && tenantMap.TryGetValue(tid, out var tname))
                    r["TenantName"] = tname;
                if (contracts.Columns.Contains("RoomId") && int.TryParse(r["RoomId"]?.ToString(), out var rid) && roomMap.TryGetValue(rid, out var rnum))
                    r["RoomNumber"] = rnum;
            }
        }

        private void ApplyGridPresentation()
        {
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                c.HeaderText = c.HeaderText;
            }

            SetHeader("ContractId", "ID");
            SetHeader("ContractNumber", "Số HĐ");
            SetHeader("TenantName", "Khách thuê");
            SetHeader("TenantId", "TenantId");
            SetHeader("RoomNumber", "Phòng");
            SetHeader("RoomId", "RoomId");
            SetHeader("SignDate", "Ngày ký");
            SetHeader("StartDate", "Bắt đầu");
            SetHeader("EndDate", "Kết thúc");
            SetHeader("RentalPrice", "Giá thuê");
            SetHeader("DepositRequired", "Tiền cọc");
            SetHeader("Status", "Trạng thái");
            SetHeader("ContractPdfPath", "File PDF");
            SetHeader("Terms", "Điều khoản");
            SetHeader("CreatedDate", "Tạo lúc");
            SetHeader("UpdatedDate", "Cập nhật");

            HideIfExists("TenantId");
            HideIfExists("RoomId");
            HideIfExists("BranchId");

            FormatMoney("RentalPrice");
            FormatMoney("DepositRequired");

            SetDisplayOrder(
                "ContractNumber",
                "TenantName",
                "RoomNumber",
                "StartDate",
                "EndDate",
                "RentalPrice",
                "DepositRequired",
                "Status",
                "SignDate",
                "ContractPdfPath",
                "Terms",
                "ContractId",
                "CreatedDate",
                "UpdatedDate"
            );
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

        private void FormatMoney(string columnName)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].DefaultCellStyle.Format = "N0";
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            if (_grid.CurrentRow.DataBoundItem is DataRowView drv) return drv.Row;
            return null;
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string keyword = (_txtSearch.Text ?? string.Empty).Trim();
            bool hasKeyword = !string.IsNullOrWhiteSpace(keyword);
            string status = _cboStatus.SelectedItem?.ToString();
            bool filterStatus = !string.IsNullOrWhiteSpace(status) && status != "Tất cả";

            DateTime? from = _dtFrom.Checked ? (DateTime?)_dtFrom.Value.Date : null;
            DateTime? to = _dtTo.Checked ? (DateTime?)_dtTo.Value.Date : null;

            IEnumerable<DataRow> rows = _rawTable.AsEnumerable();

            if (filterStatus && _rawTable.Columns.Contains("Status"))
                rows = rows.Where(r => string.Equals(r["Status"]?.ToString(), status, StringComparison.OrdinalIgnoreCase));

            if (from.HasValue && _rawTable.Columns.Contains("StartDate"))
                rows = rows.Where(r => DateTime.TryParse(r["StartDate"]?.ToString(), out var d) && d.Date >= from.Value);

            if (to.HasValue && _rawTable.Columns.Contains("EndDate"))
                rows = rows.Where(r => DateTime.TryParse(r["EndDate"]?.ToString(), out var d) && d.Date <= to.Value);

            if (hasKeyword)
            {
                string kw = keyword.ToLowerInvariant();
                rows = rows.Where(r => RowContains(r, kw));
            }

            var filtered = _rawTable.Clone();
            foreach (var r in rows)
                filtered.ImportRow(r);

            _grid.DataSource = filtered;
            ApplyGridPresentation();
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
        }

        private bool RowContains(DataRow row, string keyword)
        {
            foreach (DataColumn c in row.Table.Columns)
            {
                if (c.DataType == typeof(byte[])) continue;
                string text = row[c]?.ToString();
                if (!string.IsNullOrEmpty(text) && text.ToLowerInvariant().Contains(keyword))
                    return true;
            }
            return false;
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmContractEditor(_bll, null))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedAsync()
        {
            var selected = GetCurrentRow();
            if (selected == null)
            {
                MessageBox.Show("Chọn một dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmContractEditor(_bll, selected))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = row.Table.Columns.Contains("ContractId") ? Convert.ToInt32(row["ContractId"]) : 0;
            if (MessageBox.Show($"Xóa hợp đồng ID {id}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteContractAsync(id);
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task MarkDoneAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một hợp đồng để đánh dấu đã xong.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string currentStatus = row.Table.Columns.Contains("Status") ? row["Status"]?.ToString() : null;
            if (string.Equals(currentStatus, "Terminated", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currentStatus, "Expired", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Hợp đồng này đã ở trạng thái kết thúc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Đánh dấu hợp đồng này là 'Đã xong' (Status = Terminated)?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int id = row.Table.Columns.Contains("ContractId") ? Convert.ToInt32(row["ContractId"]) : 0;
            if (id <= 0)
            {
                MessageBox.Show("Không xác định được ContractId.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var raw = await _bll.GetContractsAsync();
                var found = raw.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["ContractId"]) == id);
                if (found == null)
                {
                    MessageBox.Show("Không tìm thấy hợp đồng để cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string contractNumber = found["ContractNumber"]?.ToString();
                int tenantId = Convert.ToInt32(found["TenantId"]);
                int roomId = Convert.ToInt32(found["RoomId"]);
                DateTime? signDate = DateTime.TryParse(found["SignDate"]?.ToString(), out var sd) ? (DateTime?)sd.Date : null;
                DateTime startDate = Convert.ToDateTime(found["StartDate"]).Date;
                DateTime endDate = Convert.ToDateTime(found["EndDate"]).Date;
                decimal? rentalPrice = decimal.TryParse(found["RentalPrice"]?.ToString(), out var rp) ? (decimal?)rp : null;
                decimal? depositRequired = decimal.TryParse(found["DepositRequired"]?.ToString(), out var dr) ? (decimal?)dr : null;
                string terms = found["Terms"]?.ToString();
                string pdf = found["ContractPdfPath"]?.ToString();

                await _bll.UpdateContractAsync(id, contractNumber, tenantId, roomId, signDate, startDate, endDate, rentalPrice, depositRequired, terms, pdf, "Terminated");
                await LoadDataAsync();
                AdminEvents.NotifyDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task EnsureAllowedBranchScopeAsync()
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

        private void ViewPdf()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để xem PDF.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string path = row.Table.Columns.Contains("ContractPdfPath") ? row["ContractPdfPath"]?.ToString() : null;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Hợp đồng chưa có đường dẫn PDF.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (!System.IO.File.Exists(path))
                {
                    MessageBox.Show("Không tìm thấy file: " + path, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
