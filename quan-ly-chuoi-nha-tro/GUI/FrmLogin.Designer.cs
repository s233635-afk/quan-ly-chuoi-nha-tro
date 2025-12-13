namespace quan_ly_chuoi_nha_tro.GUI
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.tblCard = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lnkRegister = new System.Windows.Forms.LinkLabel();
            this.lnkForgot = new System.Windows.Forms.LinkLabel();
            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.pnlPassBox = new System.Windows.Forms.Panel();
            this.lblPassIcon = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.pnlUserBox = new System.Windows.Forms.Panel();
            this.lblUserIcon = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblWelcome2 = new System.Windows.Forms.Label();
            this.lblWelcome1 = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlContainer.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.tblCard.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlPassBox.SuspendLayout();
            this.pnlUserBox.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContainer
            // 
            this.pnlContainer.Controls.Add(this.pnlForm);
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(980, 560);
            this.pnlContainer.TabIndex = 0;
            this.pnlContainer.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlContainer_Paint);
            // 
            // pnlForm
            // 
            this.pnlForm.BackColor = System.Drawing.Color.White;
            this.pnlForm.Controls.Add(this.tblCard);
            this.pnlForm.Location = new System.Drawing.Point(60, 60);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(860, 440);
            this.pnlForm.TabIndex = 0;
            this.pnlForm.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlForm_Paint);
            // 
            // tblCard
            // 
            this.tblCard.ColumnCount = 2;
            this.tblCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tblCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tblCard.Controls.Add(this.pnlLeft, 0, 0);
            this.tblCard.Controls.Add(this.pnlRight, 1, 0);
            this.tblCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCard.Location = new System.Drawing.Point(0, 0);
            this.tblCard.Margin = new System.Windows.Forms.Padding(0);
            this.tblCard.Name = "tblCard";
            this.tblCard.RowCount = 1;
            this.tblCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCard.Size = new System.Drawing.Size(860, 440);
            this.tblCard.TabIndex = 0;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.lnkRegister);
            this.pnlLeft.Controls.Add(this.lnkForgot);
            this.pnlLeft.Controls.Add(this.chkRemember);
            this.pnlLeft.Controls.Add(this.btnLogin);
            this.pnlLeft.Controls.Add(this.pnlPassBox);
            this.pnlLeft.Controls.Add(this.pnlUserBox);
            this.pnlLeft.Controls.Add(this.lblSubtitle);
            this.pnlLeft.Controls.Add(this.lblWelcome2);
            this.pnlLeft.Controls.Add(this.lblWelcome1);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(44, 44, 30, 44);
            this.pnlLeft.Size = new System.Drawing.Size(473, 440);
            this.pnlLeft.TabIndex = 0;
            // 
            // lnkRegister
            // 
            this.lnkRegister.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.lnkRegister.AutoSize = true;
            this.lnkRegister.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.lnkRegister.Location = new System.Drawing.Point(44, 376);
            this.lnkRegister.Name = "lnkRegister";
            this.lnkRegister.Size = new System.Drawing.Size(98, 17);
            this.lnkRegister.TabIndex = 8;
            this.lnkRegister.TabStop = true;
            this.lnkRegister.Text = "Tạo tài khoản";
            this.lnkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRegister_LinkClicked);
            // 
            // lnkForgot
            // 
            this.lnkForgot.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.lnkForgot.AutoSize = true;
            this.lnkForgot.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lnkForgot.Location = new System.Drawing.Point(316, 252);
            this.lnkForgot.Name = "lnkForgot";
            this.lnkForgot.Size = new System.Drawing.Size(102, 17);
            this.lnkForgot.TabIndex = 7;
            this.lnkForgot.TabStop = true;
            this.lnkForgot.Text = "Quên mật khẩu?";
            this.lnkForgot.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkForgot_LinkClicked);
            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.chkRemember.Location = new System.Drawing.Point(47, 251);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new System.Drawing.Size(88, 21);
            this.chkRemember.TabIndex = 6;
            this.chkRemember.Text = "Ghi nhớ";
            this.chkRemember.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(47, 290);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(371, 48);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "Đăng nhập";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // pnlPassBox
            // 
            this.pnlPassBox.BackColor = System.Drawing.Color.White;
            this.pnlPassBox.Controls.Add(this.lblPassIcon);
            this.pnlPassBox.Controls.Add(this.txtPass);
            this.pnlPassBox.Location = new System.Drawing.Point(47, 196);
            this.pnlPassBox.Name = "pnlPassBox";
            this.pnlPassBox.Size = new System.Drawing.Size(371, 44);
            this.pnlPassBox.TabIndex = 4;
            this.pnlPassBox.Paint += new System.Windows.Forms.PaintEventHandler(this.inputBox_Paint);
            // 
            // lblPassIcon
            // 
            this.lblPassIcon.AutoSize = true;
            this.lblPassIcon.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F);
            this.lblPassIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.lblPassIcon.Location = new System.Drawing.Point(14, 12);
            this.lblPassIcon.Name = "lblPassIcon";
            this.lblPassIcon.Size = new System.Drawing.Size(20, 16);
            this.lblPassIcon.TabIndex = 1;
            this.lblPassIcon.Text = "";
            // 
            // txtPass
            // 
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPass.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPass.Location = new System.Drawing.Point(44, 12);
            this.txtPass.Name = "txtPass";
            this.txtPass.Size = new System.Drawing.Size(312, 20);
            this.txtPass.TabIndex = 0;
            this.txtPass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPass_KeyDown);
            // 
            // pnlUserBox
            // 
            this.pnlUserBox.BackColor = System.Drawing.Color.White;
            this.pnlUserBox.Controls.Add(this.lblUserIcon);
            this.pnlUserBox.Controls.Add(this.txtUser);
            this.pnlUserBox.Location = new System.Drawing.Point(47, 142);
            this.pnlUserBox.Name = "pnlUserBox";
            this.pnlUserBox.Size = new System.Drawing.Size(371, 44);
            this.pnlUserBox.TabIndex = 3;
            this.pnlUserBox.Paint += new System.Windows.Forms.PaintEventHandler(this.inputBox_Paint);
            // 
            // lblUserIcon
            // 
            this.lblUserIcon.AutoSize = true;
            this.lblUserIcon.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F);
            this.lblUserIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.lblUserIcon.Location = new System.Drawing.Point(14, 12);
            this.lblUserIcon.Name = "lblUserIcon";
            this.lblUserIcon.Size = new System.Drawing.Size(20, 16);
            this.lblUserIcon.TabIndex = 1;
            this.lblUserIcon.Text = "";
            // 
            // txtUser
            // 
            this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUser.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUser.Location = new System.Drawing.Point(44, 12);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(312, 20);
            this.txtUser.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblSubtitle.Location = new System.Drawing.Point(44, 96);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(194, 17);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Đăng nhập để tiếp tục sử dụng";
            // 
            // lblWelcome2
            // 
            this.lblWelcome2.AutoSize = true;
            this.lblWelcome2.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblWelcome2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(181)))));
            this.lblWelcome2.Location = new System.Drawing.Point(41, 54);
            this.lblWelcome2.Name = "lblWelcome2";
            this.lblWelcome2.Size = new System.Drawing.Size(321, 37);
            this.lblWelcome2.TabIndex = 1;
            this.lblWelcome2.Text = "Quản Lý Chuỗi Nhà Trọ";
            // 
            // lblWelcome1
            // 
            this.lblWelcome1.AutoSize = true;
            this.lblWelcome1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblWelcome1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblWelcome1.Location = new System.Drawing.Point(41, 18);
            this.lblWelcome1.Name = "lblWelcome1";
            this.lblWelcome1.Size = new System.Drawing.Size(159, 32);
            this.lblWelcome1.TabIndex = 0;
            this.lblWelcome1.Text = "Welcome to";
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.lblLogo);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(473, 0);
            this.pnlRight.Margin = new System.Windows.Forms.Padding(0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(387, 440);
            this.pnlRight.TabIndex = 1;
            this.pnlRight.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlRight_Paint);
            // 
            // lblLogo
            // 
            this.lblLogo.AutoSize = true;
            this.lblLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.White;
            this.lblLogo.Location = new System.Drawing.Point(250, 380);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(112, 30);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "YOUR LOGO";
            // 
            // FrmLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 560);
            this.Controls.Add(this.pnlContainer);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập - Quản Lý Chuỗi Nhà Trọ";
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.pnlContainer.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.tblCard.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.pnlPassBox.ResumeLayout(false);
            this.pnlPassBox.PerformLayout();
            this.pnlUserBox.ResumeLayout(false);
            this.pnlUserBox.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.TableLayoutPanel tblCard;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Label lblWelcome1;
        private System.Windows.Forms.Label lblWelcome2;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlUserBox;
        private System.Windows.Forms.Label lblUserIcon;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Panel pnlPassBox;
        private System.Windows.Forms.Label lblPassIcon;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.CheckBox chkRemember;
        private System.Windows.Forms.LinkLabel lnkForgot;
        private System.Windows.Forms.LinkLabel lnkRegister;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblLogo;
    }
}
