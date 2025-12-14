using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmBranchSectionEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;
        private readonly int? _presetBranchId;

        private ComboBox _cboBranch;
        private TextBox _txtCode;
        private TextBox _txtName;
        private TextBox _txtDesc;
        private CheckBox _chkActive;
        private Button _btnSave;
        private Button _btnCancel;

        private DataTable _branches;

        public int? SavedSectionId { get; private set; }

        public FrmBranchSectionEditor(AdminDataBLL bll, DataRow existingRow = null, int? presetBranchId = null)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _existingRow = existingRow;
            _presetBranchId = presetBranchId;
            InitializeComponent();
            Load += async (s, e) => await LoadLookupsAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm Khu/Dãy" : "Cập nhật Khu/Dãy";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 360);
            BackColor = Color.White;

            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White };
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12), BackColor = Color.White };

            int labelWidth = 140;
            int inputWidth = 430;
            int y = 6;
            int line = 34;

            Label MakeLabel(string text, int top) => new Label
            {
                Text = text,
                Width = labelWidth,
                Location = new Point(6, top),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control Place(Control control, int top)
            {
                control.Location = new Point(6 + labelWidth, top);
                control.Width = inputWidth;
                return control;
            }

            _cboBranch = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _txtCode = new TextBox();
            _txtName = new TextBox();
            _txtDesc = new TextBox { Multiline = true, Height = 72, ScrollBars = ScrollBars.Vertical };
            _chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };

            pnlBody.Controls.Add(MakeLabel("Chi nhánh (*)", y));
            pnlBody.Controls.Add(Place(_cboBranch, y));
            y += line;

            pnlBody.Controls.Add(MakeLabel("Mã khu/dãy (*)", y));
            pnlBody.Controls.Add(Place(_txtCode, y));
            y += line;

            pnlBody.Controls.Add(MakeLabel("Tên khu/dãy (*)", y));
            pnlBody.Controls.Add(Place(_txtName, y));
            y += line;

            pnlBody.Controls.Add(MakeLabel("Mô tả", y));
            pnlBody.Controls.Add(Place(_txtDesc, y));
            y += _txtDesc.Height + 8;

            _chkActive.Location = new Point(6 + labelWidth, y);
            pnlBody.Controls.Add(_chkActive);

            _btnSave = UiKit.MakeButton("Lưu", UiKit.Primary, async (s, e) => await SaveAsync(), 110);
            _btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            _btnCancel.FlatAppearance.BorderSize = 1;
            _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            pnlBottom.Controls.Add(_btnSave);
            pnlBottom.Controls.Add(_btnCancel);
            pnlBottom.Resize += (s, e) =>
            {
                _btnCancel.Location = new Point(pnlBottom.ClientSize.Width - _btnCancel.Width - 6, 12);
                _btnSave.Location = new Point(_btnCancel.Left - _btnSave.Width - 10, 12);
            };
            pnlBottom.PerformLayout();

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadLookupsAsync()
        {
            try
            {
                _branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                if (_branches == null)
                    _branches = new DataTable();

                var dt = _branches.Copy();
                if (!dt.Columns.Contains("BranchDisplay"))
                    dt.Columns.Add("BranchDisplay", typeof(string));

                TextFixer.FixDataTable(dt, "BranchCode", "BranchName");
                foreach (DataRow r in dt.Rows)
                {
                    string code = dt.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                    string name = dt.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                    r["BranchDisplay"] = string.IsNullOrWhiteSpace(code) ? name : $"{code} - {name}";
                }

                _cboBranch.DataSource = dt;
                _cboBranch.DisplayMember = dt.Columns.Contains("BranchDisplay") ? "BranchDisplay" : dt.Columns[0].ColumnName;
                _cboBranch.ValueMember = dt.Columns.Contains("BranchId") ? "BranchId" : dt.Columns[0].ColumnName;

                LoadExisting();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách chi nhánh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExisting()
        {
            if (_existingRow != null)
            {
                if (_existingRow.Table.Columns.Contains("BranchId") && int.TryParse(_existingRow["BranchId"]?.ToString(), out var bid))
                    _cboBranch.SelectedValue = bid;

                if (_existingRow.Table.Columns.Contains("SectionCode"))
                    _txtCode.Text = _existingRow["SectionCode"]?.ToString();
                if (_existingRow.Table.Columns.Contains("SectionName"))
                    _txtName.Text = _existingRow["SectionName"]?.ToString();
                if (_existingRow.Table.Columns.Contains("Description"))
                    _txtDesc.Text = _existingRow["Description"]?.ToString();
                if (_existingRow.Table.Columns.Contains("IsActive"))
                {
                    try { _chkActive.Checked = Convert.ToBoolean(_existingRow["IsActive"]); } catch { }
                }

                return;
            }

            if (_presetBranchId.HasValue)
                _cboBranch.SelectedValue = _presetBranchId.Value;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (!(_cboBranch.SelectedValue is int branchId) || branchId <= 0)
            {
                MessageBox.Show("Vui lòng chọn Chi nhánh.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var code = (_txtCode.Text ?? string.Empty).Trim();
            var name = (_txtName.Text ?? string.Empty).Trim();
            var desc = (_txtDesc.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Vui lòng nhập Mã khu/dãy.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập Tên khu/dãy.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddBranchSectionAsync(branchId, code, name, string.IsNullOrWhiteSpace(desc) ? null : desc, _chkActive.Checked);
                    SavedSectionId = newId;
                }
                else
                {
                    int id = _existingRow.Table.Columns.Contains("SectionId") ? Convert.ToInt32(_existingRow["SectionId"]) : 0;
                    await _bll.UpdateBranchSectionAsync(id, branchId, code, name, string.IsNullOrWhiteSpace(desc) ? null : desc, _chkActive.Checked);
                    SavedSectionId = id;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu khu/dãy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
