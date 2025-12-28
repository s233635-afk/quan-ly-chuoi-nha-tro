 using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomTypeEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox _txtName;
        private NumericUpDown _numDefaultPrice;
        private NumericUpDown _numMaxCapacity;
        private TextBox _txtAmenities;
        private TextBox _txtDescription;
        private CheckBox _chkActive;
        private Button _btnSave;
        private Button _btnCancel;

        public int? SavedRoomTypeId { get; private set; }

        public FrmRoomTypeEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm Loại phòng" : "Cập nhật Loại phòng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 480);
            BackColor = Color.White;

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White, AutoScroll = true };
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12), BackColor = Color.White };

            int labelWidth = 170;
            int inputWidth = 470;
            int top = 6;
            int line = 34;

            Label MakeLabel(string text, int y) => new Label { Text = text, Width = labelWidth, Location = new Point(6, y), TextAlign = ContentAlignment.MiddleLeft };
            Control Place(Control control, int y)
            {
                control.Location = new Point(6 + labelWidth, y);
                control.Width = inputWidth;
                return control;
            }

            _txtName = new TextBox();
            _numDefaultPrice = new NumericUpDown { Minimum = 0, Maximum = 100000000000m, DecimalPlaces = 0, Increment = 100000m, ThousandsSeparator = true };
            _numMaxCapacity = new NumericUpDown { Minimum = 0, Maximum = 1000, DecimalPlaces = 0, Increment = 1, ThousandsSeparator = true };
            _txtAmenities = new TextBox { Multiline = true, Height = 72, ScrollBars = ScrollBars.Vertical };
            _txtDescription = new TextBox { Multiline = true, Height = 72, ScrollBars = ScrollBars.Vertical };
            _chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };

            pnlBody.Controls.Add(MakeLabel("Tên loại phòng (*)", top));
            pnlBody.Controls.Add(Place(_txtName, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Giá mặc định", top));
            pnlBody.Controls.Add(Place(_numDefaultPrice, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Sức chứa tối đa", top));
            pnlBody.Controls.Add(Place(_numMaxCapacity, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tiện ích (Amenities)", top));
            pnlBody.Controls.Add(Place(_txtAmenities, top));
            top += _txtAmenities.Height + 8;

            pnlBody.Controls.Add(MakeLabel("Mô tả", top));
            pnlBody.Controls.Add(Place(_txtDescription, top));
            top += _txtDescription.Height + 8;

            _chkActive.Location = new Point(6 + labelWidth, top);
            pnlBody.Controls.Add(_chkActive);

            _btnSave = UiKit.MakeButton("Lưu", UiKit.Primary, async (s, e) => await SaveAsync(), 110);
            _btnCancel = new ModernButton { Text = "Hủy", Width = 110, Height = 32, FlatStyle = FlatStyle.Flat, BaseColor = Color.White, BackColor = Color.Transparent, ForeColor = Color.Black };
            _btnCancel.FlatAppearance.BorderSize = 1;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            pnlBottom.Controls.Add(_btnSave);
            pnlBottom.Controls.Add(_btnCancel);
            pnlBottom.Resize += (s, e) =>
            {
                _btnCancel.Location = new Point(pnlBottom.ClientSize.Width - _btnCancel.Width - 6, 12);
                _btnSave.Location = new Point(_btnCancel.Left - _btnSave.Width - 10, 12);
            };

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;

            if (_existingRow.Table.Columns.Contains("RoomTypeName"))
                _txtName.Text = RoomTypeCatalog.Canonicalize(_existingRow["RoomTypeName"]?.ToString());

            // Chỉ cho chỉnh các thuộc tính (giá/tiện ích...) để giữ đúng 3 loại phòng chuẩn.
            _txtName.ReadOnly = true;

            if (_existingRow.Table.Columns.Contains("DefaultPrice") && decimal.TryParse(_existingRow["DefaultPrice"]?.ToString(), out var p) && p >= 0)
                _numDefaultPrice.Value = Math.Min(_numDefaultPrice.Maximum, p);

            if (_existingRow.Table.Columns.Contains("MaxCapacity") && int.TryParse(_existingRow["MaxCapacity"]?.ToString(), out var cap) && cap >= 0)
                _numMaxCapacity.Value = Math.Min(_numMaxCapacity.Maximum, cap);

            if (_existingRow.Table.Columns.Contains("Amenities"))
                _txtAmenities.Text = _existingRow["Amenities"]?.ToString();

            if (_existingRow.Table.Columns.Contains("Description"))
                _txtDescription.Text = _existingRow["Description"]?.ToString();

            if (_existingRow.Table.Columns.Contains("IsActive"))
            {
                try { _chkActive.Checked = Convert.ToBoolean(_existingRow["IsActive"]); } catch { }
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string name = (_txtName.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập Tên loại phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal? defaultPrice = _numDefaultPrice.Value > 0 ? (decimal?)_numDefaultPrice.Value : null;
            int? maxCapacity = _numMaxCapacity.Value > 0 ? (int?)Convert.ToInt32(_numMaxCapacity.Value) : null;
            string amenities = string.IsNullOrWhiteSpace(_txtAmenities.Text) ? null : _txtAmenities.Text.Trim();
            string description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text.Trim();

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddRoomTypeAsync(name, defaultPrice, amenities, maxCapacity, description, _chkActive.Checked);
                    SavedRoomTypeId = newId;
                }
                else
                {
                    int id = _existingRow.Table.Columns.Contains("RoomTypeId") ? Convert.ToInt32(_existingRow["RoomTypeId"]) : 0;
                    await _bll.UpdateRoomTypeAsync(id, name, defaultPrice, amenities, maxCapacity, description, _chkActive.Checked);
                    SavedRoomTypeId = id;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
