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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblPass = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.pnlContainer.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.SuspendLayout();

            // pnlContainer
            this.pnlContainer.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlContainer.Controls.Add(this.pnlForm);
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(400, 500);
            this.pnlContainer.TabIndex = 0;

            // pnlForm
            this.pnlForm.BackColor = System.Drawing.Color.White;
            this.pnlForm.Controls.Add(this.btnLogin);
            this.pnlForm.Controls.Add(this.txtPass);
            this.pnlForm.Controls.Add(this.lblPass);
            this.pnlForm.Controls.Add(this.txtUser);
            this.pnlForm.Controls.Add(this.lblUser);
            this.pnlForm.Controls.Add(this.lblTitle);
            this.pnlForm.Location = new System.Drawing.Point(30, 80);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(340, 340);
            this.pnlForm.TabIndex = 1;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🔐 Đăng Nhập";

            // lblUser
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblUser.Location = new System.Drawing.Point(20, 70);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(80, 19);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "Tài Khoản";

            // txtUser
            this.txtUser.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUser.Location = new System.Drawing.Point(20, 92);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(300, 26);
            this.txtUser.TabIndex = 2;
            this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // lblPass
            this.lblPass.AutoSize = true;
            this.lblPass.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPass.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblPass.Location = new System.Drawing.Point(20, 130);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(75, 19);
            this.lblPass.TabIndex = 3;
            this.lblPass.Text = "Mật Khẩu";

            // txtPass
            this.txtPass.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPass.Location = new System.Drawing.Point(20, 152);
            this.txtPass.Name = "txtPass";
            this.txtPass.Size = new System.Drawing.Size(300, 26);
            this.txtPass.TabIndex = 4;
            this.txtPass.UseSystemPasswordChar = true;
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPass_KeyDown);

            // btnLogin
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(20, 220);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(300, 45);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "🔓 Đăng Nhập";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            // FrmLogin
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 500);
            this.Controls.Add(this.pnlContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng Nhập - Quản Lý Chuỗi Nhà Trọ";
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.pnlContainer.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.pnlForm.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Button btnLogin;
    }
}