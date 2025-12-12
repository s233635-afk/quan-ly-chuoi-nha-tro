using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox txtRoomNumber;
        private NumericUpDown numBranchId;
        private NumericUpDown numSectionId;
        private NumericUpDown numRoomTypeId;
        private NumericUpDown numStatusId;
        private NumericUpDown numFloor;
        private NumericUpDown numArea;
        private NumericUpDown numPrice;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;

        public int? SavedRoomId { get; private set; }

        public FrmRoomEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            LoadExisting();
        }

        private void InitializeComponent()
        {
            this.Text = _existingRow == null ? "Thêm phòng" : "Cập nhật phòng";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(560, 420);

            int labelWidth = 160;
            int inputWidth = 340;
            int top = 20;
            int left = 20;
            int lineHeight = 30;

            Label MakeLabel(string text, int y) => new Label
            {
                Text = text,
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth, y);
                ctl.Width = inputWidth;
                return ctl;
            }

            txtRoomNumber = new TextBox();
            numBranchId = new NumericUpDown { Minimum = 0, Maximum = 1000000, DecimalPlaces = 0 };
            numSectionId = new NumericUpDown { Minimum = 0, Maximum = 1000000, DecimalPlaces = 0 };
            numRoomTypeId = new NumericUpDown { Minimum = 0, Maximum = 1000000, DecimalPlaces = 0 };
            numStatusId = new NumericUpDown { Minimum = 0, Maximum = 1000000, DecimalPlaces = 0 };
            numFloor = new NumericUpDown { Minimum = 0, Maximum = 200, DecimalPlaces = 0 };
            numArea = new NumericUpDown { Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Increment = 0.5m };
            numPrice = new NumericUpDown { Minimum = 0, Maximum = 100000000000m, DecimalPlaces = 0, Increment = 100000m };
            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };

            this.Controls.Add(MakeLabel("Số phòng (*)", top));
            this.Controls.Add(MakeInput(txtRoomNumber, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Chi nhánh (BranchId) (*)", top));
            this.Controls.Add(MakeInput(numBranchId, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Khu/Dãy (SectionId)", top));
            this.Controls.Add(MakeInput(numSectionId, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Loại phòng (RoomTypeId)", top));
            this.Controls.Add(MakeInput(numRoomTypeId, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Trạng thái (CurrentStatusId)", top));
            this.Controls.Add(MakeInput(numStatusId, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Tầng (Floor)", top));
            this.Controls.Add(MakeInput(numFloor, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Diện tích (Area)", top));
            this.Controls.Add(MakeInput(numArea, top));
            top += lineHeight;

            this.Controls.Add(MakeLabel("Giá phòng (RoomPrice)", top));
            this.Controls.Add(MakeInput(numPrice, top));
            top += lineHeight;

            chkActive.Location = new Point(left + labelWidth, top);
            this.Controls.Add(chkActive);

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;

            txtRoomNumber.Text = ReadString(_existingRow, "RoomNumber", "RoomCode") ?? "";
            numBranchId.Value = ReadDecimal(_existingRow, 0, "BranchId");
            numSectionId.Value = ReadDecimal(_existingRow, 0, "SectionId");
            numRoomTypeId.Value = ReadDecimal(_existingRow, 0, "RoomTypeId", "RoomType");
            numStatusId.Value = ReadDecimal(_existingRow, 0, "CurrentStatusId", "Status");
            numFloor.Value = ReadDecimal(_existingRow, 0, "Floor");
            numArea.Value = ReadDecimal(_existingRow, 0, "Area");
            numPrice.Value = ReadDecimal(_existingRow, 0, "RoomPrice", "RentPrice");

            if (TryReadBool(_existingRow, out var act, "IsActive"))
                chkActive.Checked = act;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string roomNumber = (txtRoomNumber.Text ?? "").Trim();
            int branchId = (int)numBranchId.Value;

            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                MessageBox.Show("Vui lòng nhập Số phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (branchId <= 0)
            {
                MessageBox.Show("Vui lòng nhập BranchId > 0.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? sectionId = numSectionId.Value > 0 ? (int?)numSectionId.Value : null;
            int? roomTypeId = numRoomTypeId.Value > 0 ? (int?)numRoomTypeId.Value : null;
            int? statusId = numStatusId.Value > 0 ? (int?)numStatusId.Value : null;
            int? floor = numFloor.Value > 0 ? (int?)numFloor.Value : null;
            decimal? area = numArea.Value > 0 ? (decimal?)numArea.Value : null;
            decimal? price = numPrice.Value > 0 ? (decimal?)numPrice.Value : null;
            bool? isActive = chkActive.Checked;

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddRoomAsync(roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive);
                    SavedRoomId = newId;
                }
                else
                {
                    int id = ReadInt(_existingRow, 0, "RoomId", "RoomID", "Id");
                    await _bll.UpdateRoomAsync(id, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive);
                    SavedRoomId = id;
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ReadString(DataRow row, params string[] cols)
        {
            if (row == null) return null;
            foreach (var c in cols)
            {
                if (!string.IsNullOrWhiteSpace(c) && row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    return v.ToString();
                }
            }
            return null;
        }

        public static int ReadInt(DataRow row, int defaultValue, params string[] cols)
        {
            if (row == null) return defaultValue;
            foreach (var c in cols)
            {
                if (!string.IsNullOrWhiteSpace(c) && row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (int.TryParse(v.ToString(), out var i)) return i;
                    try { return Convert.ToInt32(v); } catch { }
                }
            }
            return defaultValue;
        }

        private static decimal ReadDecimal(DataRow row, decimal defaultValue, params string[] cols)
        {
            if (row == null) return defaultValue;
            foreach (var c in cols)
            {
                if (!string.IsNullOrWhiteSpace(c) && row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (decimal.TryParse(v.ToString(), out var d)) return d;
                    try { return Convert.ToDecimal(v); } catch { }
                }
            }
            return defaultValue;
        }

        private static bool TryReadBool(DataRow row, out bool value, params string[] cols)
        {
            value = false;
            if (row == null) return false;
            foreach (var c in cols)
            {
                if (!string.IsNullOrWhiteSpace(c) && row.Table.Columns.Contains(c))
                {
                    var v = row[c];
                    if (v == null || v == DBNull.Value) continue;
                    if (bool.TryParse(v.ToString(), out value)) return true;
                    try { value = Convert.ToBoolean(v); return true; } catch { }
                }
            }
            return false;
        }
    }
}

