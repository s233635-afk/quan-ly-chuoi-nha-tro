using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Popup nhỏ: xem nhanh thông tin phòng + người thuê (theo hợp đồng Active/Extended).
    /// Có nút chỉnh sửa phòng / khách thuê.
    /// </summary>
    public class FrmRoomTenantQuickView : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly Func<int, Task> _refreshRoomAsync;
        private readonly bool _isStaffMode;

        private int _roomId;
        private DataRow _roomRow;
        private DataRow _tenantRow;
        private DataRow _contractRow;
        private string _tenantSummary;

        private Label _lblTitle;
        private Label _lblSub;
        private Label _lblRoomInfo;
        private Label _lblTenantInfo;

        private Button _btnEdit;
        private Button _btnClose;
        private ContextMenuStrip _editMenu;

        public FrmRoomTenantQuickView(AdminDataBLL bll, Func<int, Task> refreshRoomAsync, bool staffMode = false)
        {
            _bll = bll;
            _refreshRoomAsync = refreshRoomAsync;
            _isStaffMode = staffMode;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Chi tiết phòng";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(660, 420);
            BackColor = Color.White;

            var header = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = UiKit.Primary });

            _lblTitle = new Label
            {
                AutoSize = true,
                Text = "Phòng",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Location = new Point(14, 10)
            };

            _lblSub = new Label
            {
                AutoSize = true,
                Text = "—",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 90, 90),
                Location = new Point(16, 38)
            };

            _btnClose = new Button
            {
                Text = "Đóng",
                Width = 90,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (s, e) => Close();

            _btnEdit = new Button
            {
                Text = "Chỉnh sửa",
                Width = 110,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnEdit.FlatAppearance.BorderSize = 0;

            if (_isStaffMode)
            {
                _btnEdit.Text = "Chỉnh sửa";
                _btnEdit.Width = 120;
                _btnEdit.Height = 36;
                _btnEdit.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                _btnEdit.BackColor = Color.FromArgb(0, 122, 204);
                _btnEdit.ForeColor = Color.White;

                _btnClose.Visible = false;
            }


            if (_isStaffMode)
            {
                _btnEdit.Click += async (s, e) => await EditRoomAsync();
            }
            else
            {
                _editMenu = new ContextMenuStrip();
                _editMenu.Items.Add("Sửa phòng", null, async (s, e) => await EditRoomAsync());
                _editMenu.Items.Add("Sửa khách thuê", null, async (s, e) => await EditTenantAsync());
                _btnEdit.Click += (s, e) => _editMenu.Show(_btnEdit, new Point(0, _btnEdit.Height));
            }

            header.Controls.Add(_lblTitle);
            header.Controls.Add(_lblSub);
            header.Controls.Add(_btnEdit);
            header.Controls.Add(_btnClose);
            header.Resize += (s, e) =>
            {
                if (_btnClose.Visible)
                {
                    _btnClose.Location = new Point(header.ClientSize.Width - _btnClose.Width - 14, 16);
                    _btnEdit.Location = new Point(_btnClose.Left - _btnEdit.Width - 10, 16);
                }
                else
                {
                    _btnEdit.Location = new Point(header.ClientSize.Width - _btnEdit.Width - 14, 16);
                }
            };

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 14)
            };
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

            var roomBox = MakeInfoBox("Thông tin phòng", out _lblRoomInfo);
            var tenantTitle = _isStaffMode ? "Người đã thuê" : "Ng??i ?ang s? d?ng";
            var tenantBox = MakeInfoBox(tenantTitle, out _lblTenantInfo);

            if (_isStaffMode)
            {
                _lblRoomInfo.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
                _lblRoomInfo.ForeColor = Color.FromArgb(35, 35, 35);
                _lblRoomInfo.AutoEllipsis = false;

                _lblTenantInfo.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
                _lblTenantInfo.ForeColor = Color.FromArgb(35, 35, 35);
                _lblTenantInfo.AutoEllipsis = false;
            }

            body.Controls.Add(roomBox, 0, 0);
            body.Controls.Add(tenantBox, 0, 1);

            Controls.Add(header);
            Controls.Add(body);
        }

        private static Panel MakeInfoBox(string title, out Label content)
        {
            var box = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 249, 255),
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12)
            };
            box.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 230, 240)))
                {
                    var rect = new Rectangle(0, 0, box.Width - 1, box.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = title ?? string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 79, 159),
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 6)
            };

            content = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoEllipsis = true
            };

            box.Controls.Add(content);
            box.Controls.Add(lblTitle);
            return box;
        }

        public void UpdateData(int roomId, DataRow roomRow, DataRow tenantRow, DataRow contractRow, string tenantSummary = null)
        {
            _roomId = roomId;
            _roomRow = roomRow;
            _tenantRow = tenantRow;
            _contractRow = contractRow;
            _tenantSummary = tenantSummary;

            string roomNumber = ReadString(_roomRow, "RoomNumber");
            string section = ReadString(_roomRow, "SectionName");
            string type = ReadString(_roomRow, "RoomTypeName");
            string status = ReadString(_roomRow, "StatusName");
            string price = FormatMoney(ReadString(_roomRow, "RoomPrice"));
            string floor = ReadString(_roomRow, "Floor");
            string area = ReadString(_roomRow, "Area");
            string isActive = ReadBool(_roomRow, "IsActive") ? "Có" : "Không";

            _lblTitle.Text = $"Phòng {NullDash(roomNumber)}";
            _lblSub.Text = $"RoomId: {roomId}   |   Trạng thái: {NullDash(status)}";

            if (_isStaffMode)
            {
                _lblSub.Text = $"Mã phòng: {NullDash(roomNumber)}";
                var areaText = string.IsNullOrWhiteSpace(area) ? "-" : area + " m2";
                _lblRoomInfo.Text =
                    $"Mã phòng: {NullDash(roomNumber)}\n" +
                    $"Giá/Tháng: {NullDash(price)}\n" +
                    $"Trạng thái: {NullDash(status)}\n" +
                    $"Diện tích: {areaText}";
            }
            else
            {
                    _lblRoomInfo.Text =
                    $"Khu/DA?y: {NullDash(section)}   |   Lo???i: {NullDash(type)}\n" +
                    $"GiA?: {NullDash(price)}   |   T???ng: {NullDash(floor)}   |   Di???n tA-ch: {NullDash(area)}\n" +
                    $"KA-ch ho???t: {NullDash(isActive)}";
            }


            if (_isStaffMode)
            {
                if (!string.IsNullOrWhiteSpace(_tenantSummary))
                {
                    _lblTenantInfo.Text = _tenantSummary;
                }
                else
                {
                    _lblTenantInfo.Text = "Phòng hiện tại còn trống";
                }
            }
            else if (_contractRow == null || _tenantRow == null)
            {
                _lblTenantInfo.Text = "Phong hien chua co nguoi su dung (khong co hop dong Active/Extended).";
            }
            else
            {
                string tenantName = ReadString(_tenantRow, "FullName");
                string tenantPhone = ReadString(_tenantRow, "PhoneNumber");
                string tenantIdCard = ReadString(_tenantRow, "IdentityCard");

                string contractNo = ReadString(_contractRow, "ContractNumber");
                string st = ReadString(_contractRow, "Status");
                string start = FormatDate(ReadString(_contractRow, "StartDate"));
                string end = FormatDate(ReadString(_contractRow, "EndDate"));

                _lblTenantInfo.Text =
                    $"Ho ten: {NullDash(tenantName)}   |   SDT: {NullDash(tenantPhone)}   |   CCCD: {NullDash(tenantIdCard)}\n" +
                    $"Hop dong: {NullDash(contractNo)}   |   {NullDash(st)}   |   {NullDash(start)} -> {NullDash(end)}";
            }

            bool canEditTenant = _tenantRow != null;
            if (_editMenu != null && _editMenu.Items.Count >= 2)
                _editMenu.Items[1].Enabled = canEditTenant;
        }

        private async Task EditRoomAsync()
        {
            if (_roomRow == null)
            {
                var message = _isStaffMode
                    ? "Khong tim thay du lieu phong."
                    : "KhA'ng tAªm th §y d ¯_ li ¯Øu phAýng.";
                var title = _isStaffMode ? "Thong bao" : "ThA'ng bA­o";
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_isStaffMode)
            {
                using (var frm = new FrmRoomEditor(_bll, _roomRow))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK && _refreshRoomAsync != null)
                        await _refreshRoomAsync(_roomId);
                }
                return;
            }

            var roomTypes = await _bll.GetRoomTypesAsync();
            var statuses = await _bll.GetRoomStatusesAsync();

            using (var dlg = new FrmRoomManager.RoomEditDialog(_roomRow, roomTypes, statuses, true))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                int roomId = TryGetInt(_roomRow, "RoomId");
                string roomNumber = ReadString(_roomRow, "RoomNumber") ?? string.Empty;
                int branchId = TryGetInt(_roomRow, "BranchId");
                int? sectionId = TryGetNullableInt(_roomRow, "SectionId");
                int? roomTypeId = dlg.SelectedRoomTypeId ?? TryGetNullableInt(_roomRow, "RoomTypeId");
                decimal? price = TryGetDecimal(_roomRow, "RoomPrice");
                int? statusId = dlg.SelectedStatusId ?? TryGetNullableInt(_roomRow, "CurrentStatusId");
                int? floor = TryGetNullableInt(_roomRow, "Floor");
                decimal? area = dlg.Area ?? TryGetDecimal(_roomRow, "Area");
                bool? isActive = dlg.IsActive ?? TryGetBool(_roomRow, "IsActive");
                int occupants = dlg.OccupantCount ?? TryGetInt(_roomRow, "Occupants");

                if (occupants > 5)
                {
                    MessageBox.Show("Moi phong toi da 5 nguoi.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (occupants >= 1)
                {
                    var occupiedId = GetOccupiedStatusId(statuses);
                    if (occupiedId.HasValue)
                        statusId = occupiedId.Value;
                }
                else if (IsOccupiedStatusId(statusId, statuses) && occupants < 1)
                {
                    var emptyId = GetEmptyStatusId(statuses);
                    if (emptyId.HasValue)
                    {
                        statusId = emptyId.Value;
                        MessageBox.Show("Phong chua co nguoi, tu dong chuyen trang thai ve Trong.", "Thong bao",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Trang thai Dang o yeu cau it nhat 1 nguoi.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                await _bll.UpdateRoomAsync(roomId, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, occupants);

                UpdateRoomRowValues(_roomRow, roomTypeId, price, statusId, floor, area, isActive, occupants, roomTypes, statuses);
                UpdateData(_roomId, _roomRow, _tenantRow, _contractRow);

                if (_refreshRoomAsync != null)
                    await _refreshRoomAsync(_roomId);
            }
        }

        private async Task EditTenantAsync()
        {
            if (_tenantRow == null)
            {
                MessageBox.Show("Phòng hiện chưa có người thuê để chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmTenantEditor(_bll, _tenantRow))
            {
                if (frm.ShowDialog(this) == DialogResult.OK && _refreshRoomAsync != null)
                    await _refreshRoomAsync(_roomId);
            }
        }

        private static string ReadString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return null;
            var v = r[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private static bool ReadBool(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return false;
            try { return Convert.ToBoolean(r[col]); } catch { return false; }
        }


        private static int TryGetInt(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var val) ? val : 0;
        }

        private static int? TryGetNullableInt(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return int.TryParse(row[column]?.ToString(), out var val) ? (int?)val : null;
        }

        private static decimal? TryGetDecimal(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return decimal.TryParse(row[column]?.ToString(), out var val) ? (decimal?)val : null;
        }

        private static bool? TryGetBool(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column)) return null;
            return bool.TryParse(row[column]?.ToString(), out var val) ? (bool?)val : null;
        }

        private static int? GetEmptyStatusId(DataTable statuses)
        {
            if (statuses == null || !statuses.Columns.Contains("StatusId")) return null;
            var row = statuses.AsEnumerable()
                .FirstOrDefault(r => NormalizeStatusKey(r["StatusName"]?.ToString()).Contains("trong"));
            if (row == null) return null;
            return int.TryParse(row["StatusId"]?.ToString(), out var id) ? (int?)id : null;
        }

        private static int? GetOccupiedStatusId(DataTable statuses)
        {
            if (statuses == null || !statuses.Columns.Contains("StatusId")) return null;
            DataRow row = statuses.AsEnumerable()
                .FirstOrDefault(r => NormalizeStatusKey(r["StatusName"]?.ToString()).Contains("dang o"));
            if (row == null)
            {
                row = statuses.AsEnumerable().FirstOrDefault(r =>
                {
                    var key = NormalizeStatusKey(r["StatusName"]?.ToString());
                    return key.Contains("dang thue") || key.Contains("da thue") || key.Contains("dang");
                });
            }
            if (row == null) return null;
            return int.TryParse(row["StatusId"]?.ToString(), out var id) ? (int?)id : null;
        }

        private static bool IsOccupiedStatusId(int? statusId, DataTable statuses)
        {
            if (!statusId.HasValue || statuses == null || !statuses.Columns.Contains("StatusId")) return false;
            var row = statuses.AsEnumerable()
                .FirstOrDefault(r => int.TryParse(r["StatusId"]?.ToString(), out var id) && id == statusId.Value);
            var name = row?["StatusName"]?.ToString() ?? string.Empty;
            return IsOccupiedStatusName(name);
        }

        private static bool IsOccupiedStatusName(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return false;
            var key = NormalizeStatusKey(statusName);
            return key.Contains("dang o") || key.Contains("dang thue") || key.Contains("da thue");
        }

        private static string NormalizeStatusKey(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName)) return string.Empty;
            var fixedName = TextFixer.FixUtf8Mojibake(statusName) ?? statusName;
            return RemoveDiacritics(fixedName).ToLowerInvariant();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static void UpdateRoomRowValues(
            DataRow row,
            int? roomTypeId,
            decimal? price,
            int? statusId,
            int? floor,
            decimal? area,
            bool? isActive,
            int occupants,
            DataTable roomTypes,
            DataTable statuses)
        {
            if (row == null || row.Table == null) return;
            void Set(string col, object val)
            {
                if (row.Table.Columns.Contains(col))
                    row[col] = val ?? DBNull.Value;
            }

            Set("RoomTypeId", roomTypeId);
            Set("RoomPrice", price);
            Set("CurrentStatusId", statusId);
            Set("Floor", floor);
            Set("Area", area);
            Set("IsActive", isActive);
            Set("Occupants", occupants);

            if (row.Table.Columns.Contains("RoomTypeName") && roomTypeId.HasValue && roomTypes != null)
            {
                var typeRow = roomTypes.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["RoomTypeId"]?.ToString(), out var id) && id == roomTypeId.Value);
                row["RoomTypeName"] = typeRow?["RoomTypeName"]?.ToString() ?? row["RoomTypeName"];
            }

            if (row.Table.Columns.Contains("TypeName") && roomTypeId.HasValue && roomTypes != null)
            {
                var typeRow = roomTypes.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["RoomTypeId"]?.ToString(), out var id) && id == roomTypeId.Value);
                row["TypeName"] = typeRow?["RoomTypeName"]?.ToString() ?? row["TypeName"];
            }

            if (row.Table.Columns.Contains("StatusName") && statusId.HasValue && statuses != null)
            {
                var statusRow = statuses.AsEnumerable()
                    .FirstOrDefault(r => int.TryParse(r["StatusId"]?.ToString(), out var id) && id == statusId.Value);
                row["StatusName"] = statusRow?["StatusName"]?.ToString() ?? row["StatusName"];
            }
        }

        private static string NullDash(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s;

        private static string FormatMoney(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, out var money))
                return money.ToString("N0");
            return raw;
        }

        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return raw;
        }
    }
}
