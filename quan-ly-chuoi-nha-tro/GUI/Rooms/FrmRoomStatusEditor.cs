using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomStatusEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox _txtName;
        private TextBox _txtDescription;
        private Button _btnSave;
        private Button _btnCancel;

        public int? SavedStatusId { get; private set; }

        public FrmRoomStatusEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm Trạng thái phòng" : "Cập nhật Trạng thái phòng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 280);
            BackColor = Color.White;

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White };
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12), BackColor = Color.White };

            int labelWidth = 160;
            int inputWidth = 410;
            int y = 6;
            int line = 34;

            Label MakeLabel(string text, int top) => new Label { Text = text, Width = labelWidth, Location = new Point(6, top), TextAlign = ContentAlignment.MiddleLeft };
            Control Place(Control control, int top)
            {
                control.Location = new Point(6 + labelWidth, top);
                control.Width = inputWidth;
                return control;
            }

            _txtName = new TextBox();
            _txtDescription = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Tên trạng thái (*)", y));
            pnlBody.Controls.Add(Place(_txtName, y));
            y += line;

            pnlBody.Controls.Add(MakeLabel("Mô tả", y));
            pnlBody.Controls.Add(Place(_txtDescription, y));

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
            if (_existingRow.Table.Columns.Contains("StatusName"))
                _txtName.Text = _existingRow["StatusName"]?.ToString();
            if (_existingRow.Table.Columns.Contains("Description"))
                _txtDescription.Text = _existingRow["Description"]?.ToString();
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string name = (_txtName.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập Tên trạng thái.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string desc = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text.Trim();

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddRoomStatusAsync(name, desc);
                    SavedStatusId = newId;
                }
                else
                {
                    int id = _existingRow.Table.Columns.Contains("StatusId") ? Convert.ToInt32(_existingRow["StatusId"]) : 0;
                    await _bll.UpdateRoomStatusAsync(id, name, desc);
                    SavedStatusId = id;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

