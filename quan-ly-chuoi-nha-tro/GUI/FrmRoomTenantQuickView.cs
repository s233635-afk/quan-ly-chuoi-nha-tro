using System;
using System.Data;
using System.Drawing;
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

        private int _roomId;
        private int _tenantId;

        private DataRow _roomRow;
        private DataRow _tenantRow;
        private DataRow _contractRow;

        private Label _lblTitle;
        private Label _lblSub;
        private Label _lblRoomInfo;
        private Label _lblTenantInfo;

        private Button _btnEdit;
        private Button _btnClose;
        private ContextMenuStrip _editMenu;

        public FrmRoomTenantQuickView(AdminDataBLL bll, Func<int, Task> refreshRoomAsync)
        {
            _bll = bll;
            _refreshRoomAsync = refreshRoomAsync;
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

            _editMenu = new ContextMenuStrip();
            _editMenu.Items.Add("Sửa phòng", null, async (s, e) => await EditRoomAsync());
            _editMenu.Items.Add("Sửa khách thuê", null, async (s, e) => await EditTenantAsync());
            _btnEdit.Click += (s, e) => _editMenu.Show(_btnEdit, new Point(0, _btnEdit.Height));

            header.Controls.Add(_lblTitle);
            header.Controls.Add(_lblSub);
            header.Controls.Add(_btnEdit);
            header.Controls.Add(_btnClose);
            header.Resize += (s, e) =>
            {
                _btnClose.Location = new Point(header.ClientSize.Width - _btnClose.Width - 14, 16);
                _btnEdit.Location = new Point(_btnClose.Left - _btnEdit.Width - 10, 16);
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
            var tenantBox = MakeInfoBox("Người đang sử dụng", out _lblTenantInfo);

            body.Controls.Add(roomBox, 0, 0);
            body.Controls.Add(tenantBox, 0, 1);

            Controls.Add(body);
            Controls.Add(header);
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

        public void UpdateData(int roomId, DataRow roomRow, DataRow tenantRow, DataRow contractRow)
        {
            _roomId = roomId;
            _roomRow = roomRow;
            _tenantRow = tenantRow;
            _contractRow = contractRow;

            _tenantId = 0;
            if (_tenantRow != null && _tenantRow.Table.Columns.Contains("TenantId"))
            {
                try { _tenantId = Convert.ToInt32(_tenantRow["TenantId"]); } catch { _tenantId = 0; }
            }

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

            _lblRoomInfo.Text =
                $"Khu/Dãy: {NullDash(section)}   |   Loại: {NullDash(type)}\n" +
                $"Giá: {NullDash(price)}   |   Tầng: {NullDash(floor)}   |   Diện tích: {NullDash(area)}\n" +
                $"Kích hoạt: {NullDash(isActive)}";

            if (_contractRow == null || _tenantRow == null)
            {
                _lblTenantInfo.Text = "Phòng hiện chưa có người sử dụng (không có hợp đồng Active/Extended).";
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
                    $"Họ tên: {NullDash(tenantName)}   |   SĐT: {NullDash(tenantPhone)}   |   CCCD: {NullDash(tenantIdCard)}\n" +
                    $"Hợp đồng: {NullDash(contractNo)}   |   {NullDash(st)}   |   {NullDash(start)} → {NullDash(end)}";
            }

            bool canEditTenant = _tenantRow != null;
            if (_editMenu != null && _editMenu.Items.Count >= 2)
                _editMenu.Items[1].Enabled = canEditTenant;
        }

        private async Task EditRoomAsync()
        {
            if (_roomRow == null)
            {
                MessageBox.Show("Không tìm thấy dữ liệu phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmRoomEditor(_bll, _roomRow))
            {
                if (frm.ShowDialog(this) == DialogResult.OK && _refreshRoomAsync != null)
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
