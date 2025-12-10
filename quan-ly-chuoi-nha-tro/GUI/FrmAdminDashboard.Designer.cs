namespace quan_ly_chuoi_nha_tro.GUI
{
    partial class FrmAdminDashboard
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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.flowSidebar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNavOverview = new System.Windows.Forms.Button();
            this.btnNavSettings = new System.Windows.Forms.Button();
            this.btnNavReport = new System.Windows.Forms.Button();
            this.btnNavInvoice = new System.Windows.Forms.Button();
            this.btnNavTenant = new System.Windows.Forms.Button();
            this.btnNavRoom = new System.Windows.Forms.Button();
            this.btnNavBranch = new System.Windows.Forms.Button();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblBrand = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblPlaceholder = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            this.flowSidebar.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();

            // pnlSidebar
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.pnlSidebar.Controls.Add(this.flowSidebar);
            this.pnlSidebar.Controls.Add(this.lblUser);
            this.pnlSidebar.Controls.Add(this.lblBrand);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Padding = new System.Windows.Forms.Padding(16, 20, 16, 20);
            this.pnlSidebar.Size = new System.Drawing.Size(230, 730);
            this.pnlSidebar.TabIndex = 0;

            // flowSidebar
            this.flowSidebar.AutoScroll = true;
            this.flowSidebar.Controls.Add(this.btnNavOverview);
            this.flowSidebar.Controls.Add(this.btnNavBranch);
            this.flowSidebar.Controls.Add(this.btnNavRoom);
            this.flowSidebar.Controls.Add(this.btnNavTenant);
            this.flowSidebar.Controls.Add(this.btnNavInvoice);
            this.flowSidebar.Controls.Add(this.btnNavReport);
            this.flowSidebar.Controls.Add(this.btnNavSettings);
            this.flowSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowSidebar.Location = new System.Drawing.Point(16, 104);
            this.flowSidebar.Name = "flowSidebar";
            this.flowSidebar.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.flowSidebar.Size = new System.Drawing.Size(198, 606);
            this.flowSidebar.TabIndex = 2;
            this.flowSidebar.WrapContents = false;

            // btnNavOverview
            this.btnNavOverview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavOverview.FlatAppearance.BorderSize = 0;
            this.btnNavOverview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavOverview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavOverview.ForeColor = System.Drawing.Color.White;
            this.btnNavOverview.Location = new System.Drawing.Point(3, 12);
            this.btnNavOverview.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavOverview.Name = "btnNavOverview";
            this.btnNavOverview.Size = new System.Drawing.Size(180, 42);
            this.btnNavOverview.TabIndex = 6;
            this.btnNavOverview.Text = "📊 Tổng quan";
            this.btnNavOverview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavOverview.UseVisualStyleBackColor = false;
            this.btnNavOverview.Click += new System.EventHandler(this.btnOverview_Click);

            // btnNavSettings
            this.btnNavSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavSettings.FlatAppearance.BorderSize = 0;
            this.btnNavSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavSettings.ForeColor = System.Drawing.Color.White;
            this.btnNavSettings.Location = new System.Drawing.Point(3, 223);
            this.btnNavSettings.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavSettings.Name = "btnNavSettings";
            this.btnNavSettings.Size = new System.Drawing.Size(180, 42);
            this.btnNavSettings.TabIndex = 5;
            this.btnNavSettings.Text = "⚙️ Cấu hình";
            this.btnNavSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavSettings.UseVisualStyleBackColor = false;
            this.btnNavSettings.Click += new System.EventHandler(this.btnSettings_Click);

            // btnNavReport
            this.btnNavReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavReport.FlatAppearance.BorderSize = 0;
            this.btnNavReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavReport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavReport.ForeColor = System.Drawing.Color.White;
            this.btnNavReport.Location = new System.Drawing.Point(3, 181);
            this.btnNavReport.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavReport.Name = "btnNavReport";
            this.btnNavReport.Size = new System.Drawing.Size(180, 42);
            this.btnNavReport.TabIndex = 4;
            this.btnNavReport.Text = "📊 Báo cáo";
            this.btnNavReport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavReport.UseVisualStyleBackColor = false;
            this.btnNavReport.Click += new System.EventHandler(this.btnReport_Click);

            // btnNavInvoice
            this.btnNavInvoice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavInvoice.FlatAppearance.BorderSize = 0;
            this.btnNavInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavInvoice.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavInvoice.ForeColor = System.Drawing.Color.White;
            this.btnNavInvoice.Location = new System.Drawing.Point(3, 139);
            this.btnNavInvoice.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavInvoice.Name = "btnNavInvoice";
            this.btnNavInvoice.Size = new System.Drawing.Size(180, 42);
            this.btnNavInvoice.TabIndex = 3;
            this.btnNavInvoice.Text = "💳 Hóa đơn";
            this.btnNavInvoice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavInvoice.UseVisualStyleBackColor = false;
            this.btnNavInvoice.Click += new System.EventHandler(this.btnInvoice_Click);

            // btnNavTenant
            this.btnNavTenant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavTenant.FlatAppearance.BorderSize = 0;
            this.btnNavTenant.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavTenant.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavTenant.ForeColor = System.Drawing.Color.White;
            this.btnNavTenant.Location = new System.Drawing.Point(3, 97);
            this.btnNavTenant.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavTenant.Name = "btnNavTenant";
            this.btnNavTenant.Size = new System.Drawing.Size(180, 42);
            this.btnNavTenant.TabIndex = 2;
            this.btnNavTenant.Text = "👥 Khách thuê";
            this.btnNavTenant.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavTenant.UseVisualStyleBackColor = false;
            this.btnNavTenant.Click += new System.EventHandler(this.btnTenant_Click);

            // btnNavRoom
            this.btnNavRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavRoom.FlatAppearance.BorderSize = 0;
            this.btnNavRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavRoom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavRoom.ForeColor = System.Drawing.Color.White;
            this.btnNavRoom.Location = new System.Drawing.Point(3, 97);
            this.btnNavRoom.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavRoom.Name = "btnNavRoom";
            this.btnNavRoom.Size = new System.Drawing.Size(180, 42);
            this.btnNavRoom.TabIndex = 1;
            this.btnNavRoom.Text = "🏠 Phòng";
            this.btnNavRoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavRoom.UseVisualStyleBackColor = false;
            this.btnNavRoom.Click += new System.EventHandler(this.btnRoom_Click);

            // btnNavBranch
            this.btnNavBranch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnNavBranch.FlatAppearance.BorderSize = 0;
            this.btnNavBranch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavBranch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNavBranch.ForeColor = System.Drawing.Color.White;
            this.btnNavBranch.Location = new System.Drawing.Point(3, 54);
            this.btnNavBranch.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.btnNavBranch.Name = "btnNavBranch";
            this.btnNavBranch.Size = new System.Drawing.Size(180, 42);
            this.btnNavBranch.TabIndex = 0;
            this.btnNavBranch.Text = "📍 Chi nhánh";
            this.btnNavBranch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavBranch.UseVisualStyleBackColor = false;
            this.btnNavBranch.Click += new System.EventHandler(this.btnBranch_Click);

            // lblUser
            this.lblUser.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUser.ForeColor = System.Drawing.Color.White;
            this.lblUser.Location = new System.Drawing.Point(16, 60);
            this.lblUser.Name = "lblUser";
            this.lblUser.Padding = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.lblUser.Size = new System.Drawing.Size(198, 44);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "Admin Panel";

            // lblBrand
            this.lblBrand.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.White;
            this.lblBrand.Location = new System.Drawing.Point(16, 20);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(198, 40);
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Text = "Quản Lý Nhà Trọ";

            // pnlMain
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.pnlMain.Controls.Add(this.pnlContent);
            this.pnlMain.Controls.Add(this.pnlHeader);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(230, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(970, 730);
            this.pnlMain.TabIndex = 1;

            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.pnlContent.Controls.Add(this.lblPlaceholder);
            this.pnlContent.Controls.Add(this.tableLayoutPanel1);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 88);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20);
            this.pnlContent.Size = new System.Drawing.Size(970, 642);
            this.pnlContent.TabIndex = 2;

            // lblPlaceholder
            this.lblPlaceholder.BackColor = System.Drawing.Color.White;
            this.lblPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPlaceholder.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblPlaceholder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(94)))), ((int)(((byte)(184)))));
            this.lblPlaceholder.Location = new System.Drawing.Point(20, 20);
            this.lblPlaceholder.Name = "lblPlaceholder";
            this.lblPlaceholder.Padding = new System.Windows.Forms.Padding(10);
            this.lblPlaceholder.Size = new System.Drawing.Size(930, 602);
            this.lblPlaceholder.TabIndex = 1;
            this.lblPlaceholder.Text = "Chọn chức năng ở thanh bên hoặc nhấn \"Tổng quan\" để xem thẻ nhanh.";
            this.lblPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // tableLayoutPanel1
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 20);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(930, 602);
            this.tableLayoutPanel1.TabIndex = 0;

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.btnLogout);
            this.pnlHeader.Controls.Add(this.lblWelcome);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
            this.pnlHeader.Size = new System.Drawing.Size(970, 88);
            this.pnlHeader.TabIndex = 1;

            // lblWelcome
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(94)))), ((int)(((byte)(184)))));
            this.lblWelcome.Location = new System.Drawing.Point(20, 24);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(220, 37);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "👋 Admin Dashboard";

            // btnLogout
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(858, 24);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(92, 36);
            this.btnLogout.TabIndex = 2;
            this.btnLogout.Text = "Đăng xuất";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // FrmAdminDashboard
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1200, 730);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "FrmAdminDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Admin Dashboard";
            this.Load += new System.EventHandler(this.FrmAdminDashboard_Load);
            this.pnlSidebar.ResumeLayout(false);
            this.flowSidebar.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.FlowLayoutPanel flowSidebar;
        private System.Windows.Forms.Button btnNavSettings;
        private System.Windows.Forms.Button btnNavReport;
        private System.Windows.Forms.Button btnNavInvoice;
        private System.Windows.Forms.Button btnNavTenant;
        private System.Windows.Forms.Button btnNavRoom;
        private System.Windows.Forms.Button btnNavBranch;
        private System.Windows.Forms.Button btnNavOverview;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblPlaceholder;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Button btnLogout;
    }
}
