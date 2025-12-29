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
            this.btnNavBranch = new System.Windows.Forms.Button();
            this.btnNavRoom = new System.Windows.Forms.Button();
            this.btnNavStaff = new System.Windows.Forms.Button();
            this.btnNavTenant = new System.Windows.Forms.Button();
            this.btnNavContract = new System.Windows.Forms.Button();
            this.btnNavDeposit = new System.Windows.Forms.Button();
            this.btnNavUtility = new System.Windows.Forms.Button();
            this.btnNavInvoice = new System.Windows.Forms.Button();
            this.btnNavReport = new System.Windows.Forms.Button();
            this.btnNavMaintenance = new System.Windows.Forms.Button();
            this.btnNavAsset = new System.Windows.Forms.Button();
            this.btnNavNotification = new System.Windows.Forms.Button();
            this.btnNavSettings = new System.Windows.Forms.Button();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblBrand = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlModuleHost = new System.Windows.Forms.Panel();
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
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.pnlSidebar.Controls.Add(this.flowSidebar);
            this.pnlSidebar.Controls.Add(this.lblUser);
            this.pnlSidebar.Controls.Add(this.lblBrand);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(240, 730);
            this.pnlSidebar.TabIndex = 0;

            // flowSidebar
            this.flowSidebar.AutoScroll = true;
            this.flowSidebar.BackColor = System.Drawing.Color.Transparent;
            this.flowSidebar.Controls.Add(this.btnNavOverview);
            this.flowSidebar.Controls.Add(this.btnNavBranch);
            this.flowSidebar.Controls.Add(this.btnNavRoom);
            this.flowSidebar.Controls.Add(this.btnNavStaff);
            this.flowSidebar.Controls.Add(this.btnNavTenant);
            this.flowSidebar.Controls.Add(this.btnNavContract);
            this.flowSidebar.Controls.Add(this.btnNavDeposit);
            this.flowSidebar.Controls.Add(this.btnNavUtility);
            this.flowSidebar.Controls.Add(this.btnNavInvoice);
            this.flowSidebar.Controls.Add(this.btnNavReport);
            this.flowSidebar.Controls.Add(this.btnNavMaintenance);
            this.flowSidebar.Controls.Add(this.btnNavAsset);
            this.flowSidebar.Controls.Add(this.btnNavNotification);
            this.flowSidebar.Controls.Add(this.btnNavSettings);
            this.flowSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowSidebar.Name = "flowSidebar";
            this.flowSidebar.Padding = new System.Windows.Forms.Padding(10, 15, 10, 10);
            this.flowSidebar.TabIndex = 2;
            this.flowSidebar.WrapContents = false;

            // btnNavOverview
            this.btnNavOverview.BackColor = System.Drawing.Color.Transparent;
            this.btnNavOverview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavOverview.FlatAppearance.BorderSize = 0;
            this.btnNavOverview.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavOverview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavOverview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavOverview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavOverview.Location = new System.Drawing.Point(0, 15);
            this.btnNavOverview.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavOverview.Name = "btnNavOverview";
            this.btnNavOverview.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavOverview.Size = new System.Drawing.Size(220, 45);
            this.btnNavOverview.TabIndex = 6;
            this.btnNavOverview.Text = "🏠  Tổng quan";
            this.btnNavOverview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavOverview.UseVisualStyleBackColor = false;
            this.btnNavOverview.Click += new System.EventHandler(this.btnOverview_Click);

            // btnNavBranch
            this.btnNavBranch.BackColor = System.Drawing.Color.Transparent;
            this.btnNavBranch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavBranch.FlatAppearance.BorderSize = 0;
            this.btnNavBranch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavBranch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavBranch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavBranch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavBranch.Location = new System.Drawing.Point(0, 65);
            this.btnNavBranch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavBranch.Name = "btnNavBranch";
            this.btnNavBranch.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavBranch.Size = new System.Drawing.Size(220, 45);
            this.btnNavBranch.TabIndex = 0;
            this.btnNavBranch.Text = "🏢  Chi nhánh";
            this.btnNavBranch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavBranch.UseVisualStyleBackColor = false;
            this.btnNavBranch.Click += new System.EventHandler(this.btnBranch_Click);

            // btnNavRoom
            this.btnNavRoom.BackColor = System.Drawing.Color.Transparent;
            this.btnNavRoom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavRoom.FlatAppearance.BorderSize = 0;
            this.btnNavRoom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavRoom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavRoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavRoom.Location = new System.Drawing.Point(0, 115);
            this.btnNavRoom.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavRoom.Name = "btnNavRoom";
            this.btnNavRoom.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavRoom.Size = new System.Drawing.Size(220, 45);
            this.btnNavRoom.TabIndex = 1;
            this.btnNavRoom.Text = "🏠️  Phòng";
            this.btnNavRoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavRoom.UseVisualStyleBackColor = false;
            this.btnNavRoom.Click += new System.EventHandler(this.btnRoom_Click);

            // btnNavStaff
            this.btnNavStaff.BackColor = System.Drawing.Color.Transparent;
            this.btnNavStaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavStaff.FlatAppearance.BorderSize = 0;
            this.btnNavStaff.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavStaff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavStaff.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavStaff.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavStaff.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavStaff.Name = "btnNavStaff";
            this.btnNavStaff.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavStaff.Size = new System.Drawing.Size(220, 45);
            this.btnNavStaff.TabIndex = 12;
            this.btnNavStaff.Text = "👥  Nhân viên";
            this.btnNavStaff.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavStaff.UseVisualStyleBackColor = false;
            this.btnNavStaff.Click += new System.EventHandler(this.btnStaff_Click);

            // btnNavTenant
            this.btnNavTenant.BackColor = System.Drawing.Color.Transparent;
            this.btnNavTenant.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavTenant.FlatAppearance.BorderSize = 0;
            this.btnNavTenant.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavTenant.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavTenant.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavTenant.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavTenant.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavTenant.Name = "btnNavTenant";
            this.btnNavTenant.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavTenant.Size = new System.Drawing.Size(220, 45);
            this.btnNavTenant.TabIndex = 2;
            this.btnNavTenant.Text = "👥  Khách thuê";
            this.btnNavTenant.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavTenant.UseVisualStyleBackColor = false;
            this.btnNavTenant.Click += new System.EventHandler(this.btnTenant_Click);

            // btnNavContract
            this.btnNavContract.BackColor = System.Drawing.Color.Transparent;
            this.btnNavContract.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavContract.FlatAppearance.BorderSize = 0;
            this.btnNavContract.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavContract.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavContract.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavContract.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavContract.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavContract.Name = "btnNavContract";
            this.btnNavContract.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavContract.Size = new System.Drawing.Size(220, 45);
            this.btnNavContract.TabIndex = 3;
            this.btnNavContract.Text = "📄  Hợp đồng";
            this.btnNavContract.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavContract.UseVisualStyleBackColor = false;
            this.btnNavContract.Click += new System.EventHandler(this.btnContract_Click);

            // btnNavDeposit
            this.btnNavDeposit.BackColor = System.Drawing.Color.Transparent;
            this.btnNavDeposit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavDeposit.FlatAppearance.BorderSize = 0;
            this.btnNavDeposit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavDeposit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavDeposit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavDeposit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavDeposit.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavDeposit.Name = "btnNavDeposit";
            this.btnNavDeposit.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavDeposit.Size = new System.Drawing.Size(220, 45);
            this.btnNavDeposit.TabIndex = 4;
            this.btnNavDeposit.Text = "💰  Đặt cọc";
            this.btnNavDeposit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavDeposit.UseVisualStyleBackColor = false;
            this.btnNavDeposit.Click += new System.EventHandler(this.btnDeposit_Click);

            // btnNavUtility
            this.btnNavUtility.BackColor = System.Drawing.Color.Transparent;
            this.btnNavUtility.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavUtility.FlatAppearance.BorderSize = 0;
            this.btnNavUtility.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavUtility.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavUtility.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavUtility.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavUtility.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavUtility.Name = "btnNavUtility";
            this.btnNavUtility.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavUtility.Size = new System.Drawing.Size(220, 45);
            this.btnNavUtility.TabIndex = 5;
            this.btnNavUtility.Text = "⚡  Điện/Nước/DV";
            this.btnNavUtility.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavUtility.UseVisualStyleBackColor = false;
            this.btnNavUtility.Click += new System.EventHandler(this.btnUtility_Click);

            // btnNavInvoice - MERGED with Payment: "💳 Hóa Đơn & Thanh Toán"
            this.btnNavInvoice.BackColor = System.Drawing.Color.Transparent;
            this.btnNavInvoice.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavInvoice.FlatAppearance.BorderSize = 0;
            this.btnNavInvoice.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavInvoice.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavInvoice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavInvoice.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavInvoice.Name = "btnNavInvoice";
            this.btnNavInvoice.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavInvoice.Size = new System.Drawing.Size(220, 45);
            this.btnNavInvoice.TabIndex = 6;
            this.btnNavInvoice.Text = "💳  Hóa Đơn & TT";
            this.btnNavInvoice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavInvoice.UseVisualStyleBackColor = false;
            this.btnNavInvoice.Click += new System.EventHandler(this.btnInvoice_Click);

            // btnNavReport
            this.btnNavReport.BackColor = System.Drawing.Color.Transparent;
            this.btnNavReport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavReport.FlatAppearance.BorderSize = 0;
            this.btnNavReport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavReport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavReport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavReport.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavReport.Name = "btnNavReport";
            this.btnNavReport.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavReport.Size = new System.Drawing.Size(220, 45);
            this.btnNavReport.TabIndex = 13;
            this.btnNavReport.Text = "📈  Báo cáo";
            this.btnNavReport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavReport.UseVisualStyleBackColor = false;
            this.btnNavReport.Click += new System.EventHandler(this.btnReport_Click);

            // btnNavMaintenance
            this.btnNavMaintenance.BackColor = System.Drawing.Color.Transparent;
            this.btnNavMaintenance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavMaintenance.FlatAppearance.BorderSize = 0;
            this.btnNavMaintenance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavMaintenance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavMaintenance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavMaintenance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavMaintenance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavMaintenance.Name = "btnNavMaintenance";
            this.btnNavMaintenance.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavMaintenance.Size = new System.Drawing.Size(220, 45);
            this.btnNavMaintenance.TabIndex = 8;
            this.btnNavMaintenance.Text = "🔧  Bảo trì";
            this.btnNavMaintenance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavMaintenance.UseVisualStyleBackColor = false;
            this.btnNavMaintenance.Click += new System.EventHandler(this.btnMaintenance_Click);

            // btnNavAsset
            this.btnNavAsset.BackColor = System.Drawing.Color.Transparent;
            this.btnNavAsset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavAsset.FlatAppearance.BorderSize = 0;
            this.btnNavAsset.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavAsset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavAsset.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavAsset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavAsset.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavAsset.Name = "btnNavAsset";
            this.btnNavAsset.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavAsset.Size = new System.Drawing.Size(220, 45);
            this.btnNavAsset.TabIndex = 9;
            this.btnNavAsset.Text = "📦  Tài sản";
            this.btnNavAsset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavAsset.UseVisualStyleBackColor = false;
            this.btnNavAsset.Click += new System.EventHandler(this.btnAsset_Click);

            // btnNavNotification
            this.btnNavNotification.BackColor = System.Drawing.Color.Transparent;
            this.btnNavNotification.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavNotification.FlatAppearance.BorderSize = 0;
            this.btnNavNotification.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavNotification.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavNotification.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavNotification.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavNotification.Name = "btnNavNotification";
            this.btnNavNotification.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavNotification.Size = new System.Drawing.Size(220, 45);
            this.btnNavNotification.TabIndex = 10;
            this.btnNavNotification.Text = "🔔  Thông báo";
            this.btnNavNotification.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavNotification.UseVisualStyleBackColor = false;
            this.btnNavNotification.Click += new System.EventHandler(this.btnNotification_Click);

            // btnNavSettings
            this.btnNavSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnNavSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavSettings.FlatAppearance.BorderSize = 0;
            this.btnNavSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.btnNavSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnNavSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnNavSettings.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.btnNavSettings.Name = "btnNavSettings";
            this.btnNavSettings.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnNavSettings.Size = new System.Drawing.Size(220, 45);
            this.btnNavSettings.TabIndex = 5;
            this.btnNavSettings.Text = "⚙️  Cấu hình";
            this.btnNavSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavSettings.UseVisualStyleBackColor = false;
            this.btnNavSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // lblUser
            this.lblUser.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUser.ForeColor = System.Drawing.Color.White;
            this.lblUser.Height = 44;
            this.lblUser.Name = "lblUser";
            this.lblUser.Padding = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "Admin: admin";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblBrand
            this.lblBrand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(49)))), ((int)(((byte)(63)))));
            this.lblBrand.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.White;
            this.lblBrand.Height = 70;
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Text = "QUẢN LÝ NHÀ TRỌ";
            this.lblBrand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

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
            this.pnlContent.Controls.Add(this.pnlModuleHost);
            this.pnlContent.Controls.Add(this.lblPlaceholder);
            this.pnlContent.Controls.Add(this.tableLayoutPanel1);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 88);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20);
            this.pnlContent.Size = new System.Drawing.Size(970, 642);
            this.pnlContent.TabIndex = 2;

            // pnlModuleHost
            this.pnlModuleHost.BackColor = System.Drawing.Color.White;
            this.pnlModuleHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlModuleHost.Location = new System.Drawing.Point(20, 20);
            this.pnlModuleHost.Name = "pnlModuleHost";
            this.pnlModuleHost.Size = new System.Drawing.Size(930, 602);
            this.pnlModuleHost.TabIndex = 0;
            this.pnlModuleHost.Visible = false;

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
            this.lblPlaceholder.Text = "Chọn chức năng ở thanh bên hoặc nhấn \"Tổng quan\" để xem thống kê nhanh.";
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
            this.lblWelcome.Text = "Admin Dashboard";

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
        private System.Windows.Forms.Button btnNavOverview;
        private System.Windows.Forms.Button btnNavBranch;
        private System.Windows.Forms.Button btnNavRoom;
        private System.Windows.Forms.Button btnNavStaff;
        private System.Windows.Forms.Button btnNavTenant;
        private System.Windows.Forms.Button btnNavContract;
        private System.Windows.Forms.Button btnNavDeposit;
        private System.Windows.Forms.Button btnNavUtility;
        private System.Windows.Forms.Button btnNavInvoice;
        private System.Windows.Forms.Button btnNavReport;
        private System.Windows.Forms.Button btnNavMaintenance;
        private System.Windows.Forms.Button btnNavAsset;
        private System.Windows.Forms.Button btnNavNotification;
        private System.Windows.Forms.Button btnNavSettings;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlModuleHost;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblPlaceholder;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Button btnLogout;
    }
}

