using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmDepositEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existing;

        private ComboBox cboTenant;
        private ComboBox cboRoom;
        private NumericUpDown numAmount;
        private DateTimePicker dtDeposit;
        private ComboBox cboType;
        private ComboBox cboStatus;
        private NumericUpDown numReturned;
        private DateTimePicker dtReturned;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;

        public FrmDepositEditor(AdminDataBLL bll, DataRow existing = null)
        {
            _bll = bll;
            _existing = existing;
            InitializeComponent();
            this.Load += async (s, e) => await LoadLookupAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _existing == null ? "Thêm đặt phòng & cọc" : "Cập nhật đặt phòng & cọc";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(560, 460);

            int labelWidth = 150;
            int inputWidth = 320;
            int top = 20;
            int left = 20;
            int line = 32;

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

            cboTenant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            numAmount = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            dtDeposit = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            numReturned = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            dtReturned = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            txtNotes = new TextBox { Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };

            cboType.Items.AddRange(new object[] { "Booking", "Official" });
            cboStatus.Items.AddRange(new object[] { "Pending", "Confirmed", "Returned", "Cancelled" });

            this.Controls.Add(MakeLabel("Khách thuê (*)", top));
            this.Controls.Add(MakeInput(cboTenant, top));
            top += line;

            this.Controls.Add(MakeLabel("Phòng (*)", top));
            this.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            this.Controls.Add(MakeLabel("Số tiền cọc", top));
            this.Controls.Add(MakeInput(numAmount, top));
            top += line;

            this.Controls.Add(MakeLabel("Ngày cọc", top));
            this.Controls.Add(MakeInput(dtDeposit, top));
            top += line;

            this.Controls.Add(MakeLabel("Loại cọc", top));
            this.Controls.Add(MakeInput(cboType, top));
            top += line;

            this.Controls.Add(MakeLabel("Trạng thái", top));
            this.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            this.Controls.Add(MakeLabel("Số tiền hoàn", top));
            this.Controls.Add(MakeInput(numReturned, top));
            top += line;

            this.Controls.Add(MakeLabel("Ngày hoàn", top));
            this.Controls.Add(MakeInput(dtReturned, top));
            top += line;

            this.Controls.Add(MakeLabel("Ghi chú", top));
            this.Controls.Add(MakeInput(txtNotes, top));
            top += 90;

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

        private async System.Threading.Tasks.Task LoadLookupAsync()
        {
            try
            {
                _tenantTable = await _bll.GetTenantsAsync();
                _roomTable = await _bll.GetRoomsAsync();

                cboTenant.DataSource = _tenantTable;
                cboTenant.DisplayMember = "FullName";
                cboTenant.ValueMember = "TenantId";

                if (_roomTable.Columns.Contains("RoomNumber"))
                {
                    _roomTable.Columns["RoomNumber"].ColumnName = "RoomDisplay";
                }
                else if (!_roomTable.Columns.Contains("RoomDisplay"))
                {
                    _roomTable.Columns.Add("RoomDisplay", typeof(string));
                }
                foreach (DataRow r in _roomTable.Rows)
                {
                    if (_roomTable.Columns.Contains("RoomNumber"))
                        r["RoomDisplay"] = r["RoomNumber"].ToString();
                    else if (_roomTable.Columns.Contains("RoomId"))
                        r["RoomDisplay"] = "Phòng " + r["RoomId"];
                }

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = "RoomId";

                LoadExisting();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tham chiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExisting()
        {
            if (_existing == null) return;
            if (_existing.Table.Columns.Contains("TenantId"))
                cboTenant.SelectedValue = _existing["TenantId"];
            if (_existing.Table.Columns.Contains("RoomId"))
                cboRoom.SelectedValue = _existing["RoomId"];

            if (decimal.TryParse(_existing["DepositAmount"]?.ToString(), out var amt))
                numAmount.Value = Math.Min(numAmount.Maximum, amt);

            if (DateTime.TryParse(_existing["DepositDate"]?.ToString(), out var d1))
            {
                dtDeposit.Value = d1;
                dtDeposit.Checked = true;
            }
            else dtDeposit.Checked = false;

            cboType.SelectedItem = _existing["DepositType"]?.ToString();
            cboStatus.SelectedItem = _existing["Status"]?.ToString();

            if (decimal.TryParse(_existing["ReturnedAmount"]?.ToString(), out var ret))
                numReturned.Value = Math.Min(numReturned.Maximum, ret);

            if (DateTime.TryParse(_existing["ReturnedDate"]?.ToString(), out var d2))
            {
                dtReturned.Value = d2;
                dtReturned.Checked = true;
            }
            else dtReturned.Checked = false;

            txtNotes.Text = _existing["Notes"]?.ToString();
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (cboTenant.SelectedValue == null || cboRoom.SelectedValue == null)
            {
                MessageBox.Show("Chọn khách thuê và phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int tenantId = Convert.ToInt32(cboTenant.SelectedValue);
            int roomId = Convert.ToInt32(cboRoom.SelectedValue);
            decimal amount = numAmount.Value;
            decimal? returned = numReturned.Value > 0 ? (decimal?)numReturned.Value : null;
            DateTime? depositDate = dtDeposit.Checked ? (DateTime?)dtDeposit.Value.Date : null;
            DateTime? returnedDate = dtReturned.Checked ? (DateTime?)dtReturned.Value.Date : null;
            string type = cboType.Text;
            string status = string.IsNullOrWhiteSpace(cboStatus.Text) ? "Pending" : cboStatus.Text;
            string notes = txtNotes.Text.Trim();

            try
            {
                if (_existing == null)
                {
                    await _bll.AddDepositAsync(tenantId, roomId, amount, depositDate, type, status, returned, returnedDate, notes);
                }
                else
                {
                    int id = Convert.ToInt32(_existing["DepositId"]);
                    await _bll.UpdateDepositAsync(id, tenantId, roomId, amount, depositDate, type, status, returned, returnedDate, notes);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đặt phòng/cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
