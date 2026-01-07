using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Form hiển thị chi tiết khách thuê (read-only)
    /// </summary>
    public class FrmTenantDetail : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _tenantRow;

        public FrmTenantDetail(AdminDataBLL bll, DataRow tenantRow)
        {
            _bll = bll;
            _tenantRow = tenantRow;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Chi tiết khách thuê";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(750, 700);
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

            TextBox MakeReadOnlyField(string value, int y)
            {
                var txt = new TextBox
                {
                    Text = value ?? "",
                    Location = new Point(left + labelWidth, y),
                    Width = inputWidth,
                    ReadOnly = true,
                    BackColor = Color.WhiteSmoke,
                    TabStop = false
                };
                return txt;
            }

            // Mã khách
            var lblTenantId = new Label
            {
                Text = "--- THÔNG TIN KHÁCH THUÊ ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblTenantId);
            top += line + 4;

            // Mã khách
            pnlBody.Controls.Add(MakeLabel("Mã khách", top));
            var txtTenantId = MakeReadOnlyField(_tenantRow["TenantId"]?.ToString(), top);
            pnlBody.Controls.Add(txtTenantId);
            top += line;

            // Họ và tên
            pnlBody.Controls.Add(MakeLabel("Họ và tên", top));
            var txtFullName = MakeReadOnlyField(_tenantRow["FullName"]?.ToString(), top);
            pnlBody.Controls.Add(txtFullName);
            top += line;

            // CMND/CCCD
            pnlBody.Controls.Add(MakeLabel("CMND/CCCD", top));
            var txtIdentity = MakeReadOnlyField(_tenantRow["IdentityCard"]?.ToString(), top);
            pnlBody.Controls.Add(txtIdentity);
            top += line;

            // Số điện thoại
            pnlBody.Controls.Add(MakeLabel("Số điện thoại", top));
            var txtPhone = MakeReadOnlyField(_tenantRow["PhoneNumber"]?.ToString(), top);
            pnlBody.Controls.Add(txtPhone);
            top += line;

            // Email
            pnlBody.Controls.Add(MakeLabel("Email", top));
            var txtEmail = MakeReadOnlyField(_tenantRow["Email"]?.ToString(), top);
            pnlBody.Controls.Add(txtEmail);
            top += line;

            // Ngày sinh
            pnlBody.Controls.Add(MakeLabel("Ngày sinh", top));
            var dtBirth = DateTime.TryParse(_tenantRow["BirthDate"]?.ToString(), out var d) ? d.ToString("dd/MM/yyyy") : "";
            var txtBirth = MakeReadOnlyField(dtBirth, top);
            pnlBody.Controls.Add(txtBirth);
            top += line;

            // Địa chỉ thường trú
            pnlBody.Controls.Add(MakeLabel("Địa chỉ thường trú", top));
            var txtAddress = new TextBox
            {
                Text = _tenantRow["Address"]?.ToString() ?? "",
                Location = new Point(left + labelWidth, top),
                Width = inputWidth,
                Height = 70,
                ReadOnly = true,
                Multiline = true,
                BackColor = Color.WhiteSmoke,
                TabStop = false,
                ScrollBars = ScrollBars.Vertical
            };
            pnlBody.Controls.Add(txtAddress);
            top += 80;

            // Ảnh CCCD
            var lblIdPhotos = new Label
            {
                Text = "--- ẢNH CCCD ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblIdPhotos);
            top += line + 4;

            // Ảnh mặt trước
            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt trước)", top));
            var txtFrontPhoto = MakeReadOnlyField(_tenantRow["FrontIdPhoto"]?.ToString(), top);
            pnlBody.Controls.Add(txtFrontPhoto);
            top += line;

            // Ảnh mặt sau
            pnlBody.Controls.Add(MakeLabel("Ảnh CCCD (mặt sau)", top));
            var txtBackPhoto = MakeReadOnlyField(_tenantRow["BackIdPhoto"]?.ToString(), top);
            pnlBody.Controls.Add(txtBackPhoto);
            top += line;

            // Tạm trú
            var lblTempReg = new Label
            {
                Text = "--- THÔNG TIN TẠM TRÚ ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblTempReg);
            top += line + 4;

            // Tạm trú tại
            pnlBody.Controls.Add(MakeLabel("Tạm trú tại", top));
            var txtTempReg = MakeReadOnlyField(_tenantRow["TemporaryRegistration"]?.ToString(), top);
            pnlBody.Controls.Add(txtTempReg);
            top += line;

            // Ngày đăng ký tạm trú
            pnlBody.Controls.Add(MakeLabel("Ngày đăng ký tạm trú", top));
            var tempRegDate = DateTime.TryParse(_tenantRow["TemporaryRegistrationDate"]?.ToString(), out var td) ? td.ToString("dd/MM/yyyy") : "";
            var txtTempRegDate = MakeReadOnlyField(tempRegDate, top);
            pnlBody.Controls.Add(txtTempRegDate);
            top += line;

            // Hết hạn tạm trú
            pnlBody.Controls.Add(MakeLabel("Hết hạn tạm trú", top));
            var tempRegExpiry = DateTime.TryParse(_tenantRow["TemporaryRegistrationExpiry"]?.ToString(), out var te) ? te.ToString("dd/MM/yyyy") : "";
            var txtTempRegExpiry = MakeReadOnlyField(tempRegExpiry, top);
            pnlBody.Controls.Add(txtTempRegExpiry);
            top += line;

            // Trạng thái
            var lblStatus = new Label
            {
                Text = "--- TRẠNG THÁI ---",
                Location = new Point(left, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            pnlBody.Controls.Add(lblStatus);
            top += line + 4;

            // Trạng thái hoạt động
            var isActive = _tenantRow.Table.Columns.Contains("IsActive") && bool.TryParse(_tenantRow["IsActive"]?.ToString(), out var a) && a;
            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            var txtStatus = MakeReadOnlyField(isActive ? "Đang hoạt động" : "Không hoạt động", top);
            pnlBody.Controls.Add(txtStatus);
            top += line;

            // Close button
            var btnClose = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 36,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0)
            };
            btnPanel.Controls.Add(btnClose);
            pnlBottom.Controls.Add(btnPanel);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }
    }
}
