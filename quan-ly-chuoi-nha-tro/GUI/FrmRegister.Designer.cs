namespace quan_ly_chuoi_nha_tro.GUI
{
    partial class FrmRegister
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
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.lblLogin = new System.Windows.Forms.Label();
            this.pnlContainer.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.SuspendLayout();

            // pnlContainer
            this.pnlContainer.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlContainer.Controls.Add(this.pnlForm);
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(400, 550);
            this.pnlContainer.TabIndex = 0;

            // pnlForm
            this.pnlForm.BackColor = System.Drawing.Color.White;
            this.pnlForm.Controls.Add(this.lblLogin);
            this.pnlForm.Controls.Add(this.btnRegister);
            this.pnlForm.Controls.Add(this.txtName);
            this.pnlForm.Controls.Add(this.lblName);
            this.pnlForm.Controls.Add(this.txtPass);
            this.pnlForm.Controls.Add(this.lblPass);
            this.pnlForm.Controls.Add(this.txtUser);
            this.pnlForm.Controls.Add(this.lblUser);
            this.pnlForm.Controls.Add(this.lblTitle);
            this.pnlForm.Location = new System.Drawing.Point(30, 30);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(340, 490);
            this.pnlForm.TabIndex = 1;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📝 Đăng Ký";

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
            this.txtPass.TextChanged += new System.EventHandler(this.txtPass_TextChanged);

            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblName.Location = new System.Drawing.Point(20, 190);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(80, 19);
            this.lblName.TabIndex = 5;
            this.lblName.Text = "Tên Đầy Đủ";

            // txtName
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtName.Location = new System.Drawing.Point(20, 212);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(300, 26);
            this.txtName.TabIndex = 6;
            this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // btnRegister
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(20, 260);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(300, 40);
            this.btnRegister.TabIndex = 7;
            this.btnRegister.Text = "✅ Đăng Ký";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);

            // lblLogin
            this.lblLogin.AutoSize = true;
            this.lblLogin.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLogin.ForeColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblLogin.Location = new System.Drawing.Point(20, 320);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(180, 19);
            this.lblLogin.TabIndex = 8;
            this.lblLogin.Text = "🔐 Đã có tài khoản? Đăng nhập";
            this.lblLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLogin.Click += new System.EventHandler(this.lblLogin_Click);

            // FrmRegister
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 550);
            this.Controls.Add(this.pnlContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRegister";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng Ký - Quản Lý Chuỗi Nhà Trọ";
            this.Load += new System.EventHandler(this.FrmRegister_Load);
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
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label lblLogin;
    }
}
