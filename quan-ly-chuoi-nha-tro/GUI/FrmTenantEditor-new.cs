using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Form Thêm/Sửa khách thuê với các field: Mã HĐ, Họ tên, CMND/CCCD, Ảnh, Tạm trú, Phòng, Giá thuê, Tiền cọc, v.v.
    /// </summary>
    public class FrmTenantEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;
        private DataTable _rooms;

        // Contract fields
        private TextBox txtContractId;
        private DateTimePicker dtContractDate;
        
        // Tenant fields
        private TextBox txtFullName;
        private TextBox txtIdentity;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private DateTimePicker dtBirth;
        private TextBox txtAddress;
        
        // ID Photo fields
        private TextBox txtFrontIdPhoto;
        private TextBox txtBackIdPhoto;
        private Button btnBrowseFront;
        private Button btnBrowseBack;
        
        // Temporary registration
        private TextBox txtTempReg;
        private DateTimePicker dtTempRegFrom;
        private DateTimePicker dtTempRegTo;
        
        // Room & Rental
        private ComboBox cboRoom;
        private TextBox txtRentalPrice;  // Auto-filled, read-only
        private TextBox txtDeposit;      // Auto-calculated (rental price * 1)
        private DateTimePicker dtStartDate;
        private DateTimePicker dtEndDate;
        
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;

        public int? SavedTenantId { get; private set; }

        public FrmTenantEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) =>
            {
                await LoadRoomsAsync();
                LoadExisting();
            };
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm khách thuê" : "Cập nhật khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(750, 750);
            BackColor = Color.White;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

            int labelWidth = 200;
            int inputWidth = 450;
            int top = 10;
            int left = 6;
            int line = 34;

            Label MakeLabel(string text, int y) => new Label
            {
                Text = text + ":",
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth, y);
                ctl.Width = inputWidth;
                return ctl;
            }

            // ===== HỢP ĐỒNG SECTION =====
            var lblContractSection = new Label
            {
                Text = "--- THÔNG TIN HỢP ĐỒNG ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblContractSection);
            top += line + 4;

            // Mã Hợp Đồng
            pnlBody.Controls.Add(MakeLabel("Mã Hợp Đồng (*)", top));
            txtContractId = new TextBox { ReadOnly = _existingRow != null };
            pnlBody.Controls.Add(MakeInput(txtContractId, top));
            top += line;

            // Ngày kí Hợp Đồng
            pnlBody.Controls.Add(MakeLabel("Ngày kí (*)", top));
            dtContractDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Value = DateTime.Now, Checked = true };
            pnlBody.Controls.Add(MakeInput(dtContractDate, top));
            top += line;

            // ===== KHÁCH THUÊ SECTION =====
            var lblTenantSection = new Label
            {
                Text = "--- THÔNG TIN KHÁCH THUÊ ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblTenantSection);
            top += line + 4;

            // Họ và tên
            pnlBody.Controls.Add(MakeLabel("Họ và tên (*)", top));
            txtFullName = new TextBox();
            pnlBody.Controls.Add(MakeInput(txtFullName, top));
            top += line;

            // CMND/CCCD
            pnlBody.Controls.Add(MakeLabel("CMND/CCCD", top));
            txtIdentity = new TextBox();
            pnlBody.Controls.Add(MakeInput(txtIdentity, top));
            top += line;

            // Số điện thoại
            pnlBody.Controls.Add(MakeLabel("Số điện thoại", top));
            txtPhone = new TextBox();
            pnlBody.Controls.Add(MakeInput(txtPhone, top));
            top += line;

            // Email
            pnlBody.Controls.Add(MakeLabel("Email", top));
            txtEmail = new TextBox();
            pnlBody.Controls.Add(MakeInput(txtEmail, top));
            top += line;

            // Ngày sinh
            pnlBody.Controls.Add(MakeLabel("Ngày sinh", top));
            dtBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            pnlBody.Controls.Add(MakeInput(dtBirth, top));
            top += line;

            // Địa chỉ thường trú
            pnlBody.Controls.Add(MakeLabel("Địa chỉ thường trú", top));
            txtAddress = new TextBox { Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical };
            pnlBody.Controls.Add(MakeInput(txtAddress, top));
            top += 80;

            // Ảnh CCCD - Mặt trước
            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt trước)", top));
            var pnlFront = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 26, AutoSize = false };
            txtFrontIdPhoto = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            btnBrowseFront = new Button { Text = "Chọn...", Width = 80, Dock = DockStyle.Right, Margin = new Padding(4, 0, 0, 0) };
            btnBrowseFront.Click += (s, e) => BrowseFileToTextBox(txtFrontIdPhoto, "Chọn ảnh CCCD mặt trước");
            pnlFront.Controls.Add(txtFrontIdPhoto);
            pnlFront.Controls.Add(btnBrowseFront);
            pnlBody.Controls.Add(pnlFront);
            top += line;

            // Ảnh CCCD - Mặt sau
            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt sau)", top));
            var pnlBack = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 26, AutoSize = false };
            txtBackIdPhoto = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            btnBrowseBack = new Button { Text = "Chọn...", Width = 80, Dock = DockStyle.Right, Margin = new Padding(4, 0, 0, 0) };
            btnBrowseBack.Click += (s, e) => BrowseFileToTextBox(txtBackIdPhoto, "Chọn ảnh CCCD mặt sau");
            pnlBack.Controls.Add(txtBackIdPhoto);
            pnlBack.Controls.Add(btnBrowseBack);
            pnlBody.Controls.Add(pnlBack);
            top += line;

            // ===== TẠM TRÚ SECTION =====
            var lblTempRegSection = new Label
            {
                Text = "--- THÔNG TIN TẠM TRÚ ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblTempRegSection);
            top += line + 4;

            // Tạm trú tại
            pnlBody.Controls.Add(MakeLabel("Tạm trú tại", top));
            txtTempReg = new TextBox();
            pnlBody.Controls.Add(MakeInput(txtTempReg, top));
            top += line;

            // Ngày đăng ký tạm trú
            pnlBody.Controls.Add(MakeLabel("Ngày đăng ký tạm trú", top));
            dtTempRegFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            pnlBody.Controls.Add(MakeInput(dtTempRegFrom, top));
            top += line;

            // Hết hạn tạm trú
            pnlBody.Controls.Add(MakeLabel("Hết hạn tạm trú", top));
            dtTempRegTo = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            pnlBody.Controls.Add(MakeInput(dtTempRegTo, top));
            top += line;

            // ===== PHÒNG & GIÁ THUÊ SECTION =====
            var lblRentalSection = new Label
            {
                Text = "--- THÔNG TIN PHÒNG & GIÁ THUÊ ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblRentalSection);
            top += line + 4;

            // Phòng
            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom.SelectedValueChanged += (s, e) => UpdateRentalPrice();
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            // Giá thuê tháng (auto-filled, read-only)
            pnlBody.Controls.Add(MakeLabel("Giá thuê tháng (đ)", top));
            txtRentalPrice = new TextBox { ReadOnly = true, BackColor = Color.WhiteSmoke, TextAlign = HorizontalAlignment.Right };
            pnlBody.Controls.Add(MakeInput(txtRentalPrice, top));
            top += line;

            // Tiền cọc (auto-calculated)
            pnlBody.Controls.Add(MakeLabel("Tiền cọc (đ)", top));
            txtDeposit = new TextBox { ReadOnly = true, BackColor = Color.WhiteSmoke, TextAlign = HorizontalAlignment.Right };
            pnlBody.Controls.Add(MakeInput(txtDeposit, top));
            top += line;

            // Ngày bắt đầu
            pnlBody.Controls.Add(MakeLabel("Ngày bắt đầu (*)", top));
            dtStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Value = DateTime.Now, Checked = true };
            pnlBody.Controls.Add(MakeInput(dtStartDate, top));
            top += line;

            // Ngày kết thúc
            pnlBody.Controls.Add(MakeLabel("Ngày kết thúc", top));
            dtEndDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = false };
            pnlBody.Controls.Add(MakeInput(dtEndDate, top));
            top += line;

            // Trạng thái
            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true, Location = new Point(left + labelWidth, top + 6) };
            pnlBody.Controls.Add(chkActive);

            // Buttons
            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0)
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            pnlBottom.Controls.Add(btnPanel);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            try
            {
                _rooms = await _bll.GetRoomsAsync();
                if (_rooms != null)
                {
                    cboRoom.DataSource = _rooms;
                    cboRoom.DisplayMember = "RoomNumber";
                    cboRoom.ValueMember = "RoomId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách phòng: {ex.Message}", "Lỗi");
            }
        }

        private void UpdateRentalPrice()
        {
            if (cboRoom.SelectedItem is DataRowView row)
            {
                decimal price = 0;
                if (row["Price"] != DBNull.Value && decimal.TryParse(row["Price"].ToString(), out var p))
                {
                    price = p;
                }
                txtRentalPrice.Text = price.ToString("N0");
                txtDeposit.Text = price.ToString("N0");  // Deposit = Rental price * 1
            }
            else
            {
                txtRentalPrice.Text = "";
                txtDeposit.Text = "";
            }
        }

        private void LoadExisting()
        {
            if (_existingRow == null) 
            {
                // Generate contract ID for new tenant
                txtContractId.Text = GenerateContractId();
                return;
            }

            // Load from existing row
            txtFullName.Text = _existingRow["FullName"]?.ToString() ?? "";
            txtIdentity.Text = _existingRow["IdentityCard"]?.ToString() ?? "";
            txtPhone.Text = _existingRow["PhoneNumber"]?.ToString() ?? "";
            txtEmail.Text = _existingRow["Email"]?.ToString() ?? "";
            txtAddress.Text = _existingRow["Address"]?.ToString() ?? "";
            txtFrontIdPhoto.Text = _existingRow["FrontIdPhoto"]?.ToString() ?? "";
            txtBackIdPhoto.Text = _existingRow["BackIdPhoto"]?.ToString() ?? "";
            txtTempReg.Text = _existingRow["TemporaryRegistration"]?.ToString() ?? "";
            
            SetDatePicker(dtBirth, _existingRow["BirthDate"]?.ToString());
            SetDatePicker(dtTempRegFrom, _existingRow["TemporaryRegistrationDate"]?.ToString());
            SetDatePicker(dtTempRegTo, _existingRow["TemporaryRegistrationExpiry"]?.ToString());
            
            chkActive.Checked = _existingRow.Table.Columns.Contains("IsActive") && bool.TryParse(_existingRow["IsActive"]?.ToString(), out var active) && active;
        }

        private void SetDatePicker(DateTimePicker picker, string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue)) return;
            if (DateTime.TryParse(rawValue, out var dt))
            {
                picker.Value = dt;
                picker.Checked = true;
            }
        }

        private string GenerateContractId()
        {
            // Format: HD-ddMMyyyy-XXXX where XXXX is random 4 digits
            DateTime now = DateTime.Now;
            int randomPart = new Random().Next(1000, 10000);
            return $"HD-{now:ddMMyyyy}-{randomPart}";
        }

        private void BrowseFileToTextBox(TextBox txt, string title)
        {
            using (var ofd = new OpenFileDialog { Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txt.Text = ofd.FileName;
                }
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Họ và tên không được trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_existingRow == null)
                {
                    // Add new
                    int newId = await _bll.AddTenantAsync(
                        txtFullName.Text.Trim(),
                        txtIdentity.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(),
                        dtBirth.Checked ? dtBirth.Value : (DateTime?)null,
                        txtAddress.Text.Trim(),
                        txtTempReg.Text.Trim(),
                        dtTempRegFrom.Checked ? dtTempRegFrom.Value : (DateTime?)null,
                        dtTempRegTo.Checked ? dtTempRegTo.Value : (DateTime?)null,
                        chkActive.Checked,
                        txtFrontIdPhoto.Text.Trim(),
                        txtBackIdPhoto.Text.Trim()
                    );
                    SavedTenantId = newId;
                }
                else
                {
                    // Update existing
                    int tenantId = int.TryParse(_existingRow["TenantId"]?.ToString(), out var id) ? id : 0;
                    await _bll.UpdateTenantAsync(
                        tenantId,
                        txtFullName.Text.Trim(),
                        txtIdentity.Text.Trim(),
                        txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(),
                        dtBirth.Checked ? dtBirth.Value : (DateTime?)null,
                        txtAddress.Text.Trim(),
                        txtTempReg.Text.Trim(),
                        dtTempRegFrom.Checked ? dtTempRegFrom.Value : (DateTime?)null,
                        dtTempRegTo.Checked ? dtTempRegTo.Value : (DateTime?)null,
                        chkActive.Checked,
                        txtFrontIdPhoto.Text.Trim(),
                        txtBackIdPhoto.Text.Trim()
                    );
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu khách thuê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
