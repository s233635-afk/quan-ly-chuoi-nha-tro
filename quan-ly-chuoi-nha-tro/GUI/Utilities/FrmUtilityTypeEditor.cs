using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmUtilityTypeEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox txtName;
        private TextBox txtCode;
        private TextBox txtUnit;
        private CheckBox chkRecurring;
        private NumericUpDown numDefaultPrice;
        private CheckBox chkActive;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        public FrmUtilityTypeEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += (s, e) => LoadExisting();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm loại dịch vụ" : "Cập nhật loại dịch vụ";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 420);
            BackColor = Color.White;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 190;
            int inputWidth = 440;
            int top = 10;
            int left = 6;
            int line = 34;

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

            txtName = new TextBox();
            txtCode = new TextBox();
            txtUnit = new TextBox();
            chkRecurring = new CheckBox { Text = "Dịch vụ định kỳ", AutoSize = true, Checked = true };
            numDefaultPrice = new NumericUpDown { Minimum = 0, Maximum = 100000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            chkActive = new CheckBox { Text = "Kích hoạt", AutoSize = true, Checked = true };
            txtDescription = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Tên dịch vụ (*)", top));
            pnlBody.Controls.Add(MakeInput(txtName, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Mã dịch vụ", top));
            pnlBody.Controls.Add(MakeInput(txtCode, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Đơn vị", top));
            pnlBody.Controls.Add(MakeInput(txtUnit, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Đơn giá mặc định", top));
            pnlBody.Controls.Add(MakeInput(numDefaultPrice, top));
            top += line;

            var pnlChecks = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 26 };
            chkRecurring.Parent = pnlChecks;
            chkRecurring.Location = new Point(0, 3);
            chkActive.Parent = pnlChecks;
            chkActive.Location = new Point(160, 3);
            pnlBody.Controls.Add(MakeLabel("Thiết lập", top));
            pnlBody.Controls.Add(pnlChecks);
            top += line;

            pnlBody.Controls.Add(MakeLabel("Mô tả", top));
            pnlBody.Controls.Add(MakeInput(txtDescription, top));

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);
            btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private void LoadExisting()
        {
            if (_existingRow == null) return;
            txtName.Text = _existingRow.Table.Columns.Contains("UtilityName") ? _existingRow["UtilityName"]?.ToString() : string.Empty;
            txtCode.Text = _existingRow.Table.Columns.Contains("UtilityCode") ? _existingRow["UtilityCode"]?.ToString() : string.Empty;
            txtUnit.Text = _existingRow.Table.Columns.Contains("Unit") ? _existingRow["Unit"]?.ToString() : string.Empty;

            chkRecurring.Checked = ReadBool(_existingRow, "IsRecurring") ?? true;
            chkActive.Checked = ReadBool(_existingRow, "IsActive") ?? true;

            decimal price = ReadDecimal(_existingRow, "DefaultPrice");
            if (price >= numDefaultPrice.Minimum && price <= numDefaultPrice.Maximum)
                numDefaultPrice.Value = price;

            txtDescription.Text = _existingRow.Table.Columns.Contains("Description") ? _existingRow["Description"]?.ToString() : string.Empty;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên dịch vụ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal? defaultPrice = numDefaultPrice.Value > 0 ? (decimal?)numDefaultPrice.Value : null;

                if (_existingRow == null)
                {
                    await _bll.AddUtilityTypeAsync(
                        name,
                        txtCode.Text.Trim(),
                        txtUnit.Text.Trim(),
                        chkRecurring.Checked,
                        defaultPrice,
                        txtDescription.Text.Trim(),
                        chkActive.Checked);
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["UtilityTypeId"]);
                    await _bll.UpdateUtilityTypeAsync(
                        id,
                        name,
                        txtCode.Text.Trim(),
                        txtUnit.Text.Trim(),
                        chkRecurring.Checked,
                        defaultPrice,
                        txtDescription.Text.Trim(),
                        chkActive.Checked);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu loại dịch vụ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool? ReadBool(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == null || v == DBNull.Value) return null;
            if (bool.TryParse(v.ToString(), out var b)) return b;
            try { return Convert.ToBoolean(v); } catch { return null; }
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return 0m;
        }
    }
}

