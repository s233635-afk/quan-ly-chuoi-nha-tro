using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    partial class FrmBranch
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dataGridViewBranches;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnSearch;
        private TextBox txtSearch;
        private Label lblTotalBranches;
        private Panel pnlHeader;
        private Label lblSubtitle;
        private Label lblTitle;
        private TableLayoutPanel tableLayoutPanelMain;
        private Panel pnlStats;
        private FlowLayoutPanel flowLayoutPanelStats;
        private Panel pnlStatTotal;
        private Label lblStatTotalLabel;
        private Label lblStatTotalValue;
        private Panel pnlStatActive;
        private Label lblStatActiveLabel;
        private Label lblStatActiveValue;
        private Panel pnlStatInactive;
        private Label lblStatInactiveLabel;
        private Label lblStatInactiveValue;
        private Panel pnlActions;
        private Label lblSearch;

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
            this.dataGridViewBranches = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblTotalBranches = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlStats = new System.Windows.Forms.Panel();
            this.flowLayoutPanelStats = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStatTotal = new System.Windows.Forms.Panel();
            this.lblStatTotalValue = new System.Windows.Forms.Label();
            this.lblStatTotalLabel = new System.Windows.Forms.Label();
            this.pnlStatActive = new System.Windows.Forms.Panel();
            this.lblStatActiveValue = new System.Windows.Forms.Label();
            this.lblStatActiveLabel = new System.Windows.Forms.Label();
            this.pnlStatInactive = new System.Windows.Forms.Panel();
            this.lblStatInactiveValue = new System.Windows.Forms.Label();
            this.lblStatInactiveLabel = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.lblSearch = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBranches)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.pnlStats.SuspendLayout();
            this.flowLayoutPanelStats.SuspendLayout();
            this.pnlStatTotal.SuspendLayout();
            this.pnlStatActive.SuspendLayout();
            this.pnlStatInactive.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewBranches
            // 
            this.dataGridViewBranches.AllowUserToAddRows = false;
            this.dataGridViewBranches.AllowUserToDeleteRows = false;
            this.dataGridViewBranches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewBranches.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewBranches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewBranches.Location = new System.Drawing.Point(20, 230);
            this.dataGridViewBranches.MultiSelect = false;
            this.dataGridViewBranches.Name = "dataGridViewBranches";
            this.dataGridViewBranches.ReadOnly = true;
            this.dataGridViewBranches.RowHeadersVisible = false;
            this.dataGridViewBranches.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewBranches.Size = new System.Drawing.Size(960, 410);
            this.dataGridViewBranches.TabIndex = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(15, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(90, 30);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Thêm";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Location = new System.Drawing.Point(111, 12);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(90, 30);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Sửa";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(207, 12);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 30);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Xóa";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Location = new System.Drawing.Point(303, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 30);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Tải lại";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(880, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 30);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "Tìm";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(630, 14);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(240, 25);
            this.txtSearch.TabIndex = 5;
            // 
            // lblTotalBranches
            // 
            this.lblTotalBranches.AutoSize = true;
            this.lblTotalBranches.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalBranches.Location = new System.Drawing.Point(410, 19);
            this.lblTotalBranches.Name = "lblTotalBranches";
            this.lblTotalBranches.Size = new System.Drawing.Size(80, 15);
            this.lblTotalBranches.TabIndex = 7;
            this.lblTotalBranches.Text = "Tổng: 0 chi nhánh";
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(20, 20);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(960, 70);
            this.pnlHeader.TabIndex = 8;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(105)))), ((int)(((byte)(130)))));
            this.lblSubtitle.Location = new System.Drawing.Point(20, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(268, 19);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Giám sát hoạt động và tình trạng chi nhánh";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(60)))), ((int)(((byte)(130)))));
            this.lblTitle.Location = new System.Drawing.Point(18, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(210, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Quản trị Chi Nhánh";
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.pnlHeader, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.pnlStats, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.pnlActions, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.dataGridViewBranches, 0, 3);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.Padding = new System.Windows.Forms.Padding(20);
            this.tableLayoutPanelMain.RowCount = 4;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1000, 660);
            this.tableLayoutPanelMain.TabIndex = 10;
            // 
            // pnlStats
            // 
            this.pnlStats.Controls.Add(this.flowLayoutPanelStats);
            this.pnlStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStats.Location = new System.Drawing.Point(20, 110);
            this.pnlStats.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Size = new System.Drawing.Size(960, 90);
            this.pnlStats.TabIndex = 11;
            // 
            // flowLayoutPanelStats
            // 
            this.flowLayoutPanelStats.Controls.Add(this.pnlStatTotal);
            this.flowLayoutPanelStats.Controls.Add(this.pnlStatActive);
            this.flowLayoutPanelStats.Controls.Add(this.pnlStatInactive);
            this.flowLayoutPanelStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelStats.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelStats.Name = "flowLayoutPanelStats";
            this.flowLayoutPanelStats.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.flowLayoutPanelStats.Size = new System.Drawing.Size(960, 90);
            this.flowLayoutPanelStats.TabIndex = 0;
            // 
            // pnlStatTotal
            // 
            this.pnlStatTotal.BackColor = System.Drawing.Color.White;
            this.pnlStatTotal.Controls.Add(this.lblStatTotalValue);
            this.pnlStatTotal.Controls.Add(this.lblStatTotalLabel);
            this.pnlStatTotal.Location = new System.Drawing.Point(10, 10);
            this.pnlStatTotal.Margin = new System.Windows.Forms.Padding(10);
            this.pnlStatTotal.Name = "pnlStatTotal";
            this.pnlStatTotal.Padding = new System.Windows.Forms.Padding(15);
            this.pnlStatTotal.Size = new System.Drawing.Size(200, 70);
            this.pnlStatTotal.TabIndex = 0;
            // 
            // lblStatTotalValue
            // 
            this.lblStatTotalValue.AutoSize = true;
            this.lblStatTotalValue.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatTotalValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.lblStatTotalValue.Location = new System.Drawing.Point(12, 30);
            this.lblStatTotalValue.Name = "lblStatTotalValue";
            this.lblStatTotalValue.Size = new System.Drawing.Size(25, 30);
            this.lblStatTotalValue.TabIndex = 1;
            this.lblStatTotalValue.Text = "0";
            // 
            // lblStatTotalLabel
            // 
            this.lblStatTotalLabel.AutoSize = true;
            this.lblStatTotalLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatTotalLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(105)))), ((int)(((byte)(130)))));
            this.lblStatTotalLabel.Location = new System.Drawing.Point(12, 8);
            this.lblStatTotalLabel.Name = "lblStatTotalLabel";
            this.lblStatTotalLabel.Size = new System.Drawing.Size(142, 19);
            this.lblStatTotalLabel.TabIndex = 0;
            this.lblStatTotalLabel.Text = "Tổng số chi nhánh";
            // 
            // pnlStatActive
            // 
            this.pnlStatActive.BackColor = System.Drawing.Color.White;
            this.pnlStatActive.Controls.Add(this.lblStatActiveValue);
            this.pnlStatActive.Controls.Add(this.lblStatActiveLabel);
            this.pnlStatActive.Location = new System.Drawing.Point(230, 10);
            this.pnlStatActive.Margin = new System.Windows.Forms.Padding(10);
            this.pnlStatActive.Name = "pnlStatActive";
            this.pnlStatActive.Padding = new System.Windows.Forms.Padding(15);
            this.pnlStatActive.Size = new System.Drawing.Size(200, 70);
            this.pnlStatActive.TabIndex = 1;
            // 
            // lblStatActiveValue
            // 
            this.lblStatActiveValue.AutoSize = true;
            this.lblStatActiveValue.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatActiveValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.lblStatActiveValue.Location = new System.Drawing.Point(12, 30);
            this.lblStatActiveValue.Name = "lblStatActiveValue";
            this.lblStatActiveValue.Size = new System.Drawing.Size(25, 30);
            this.lblStatActiveValue.TabIndex = 1;
            this.lblStatActiveValue.Text = "0";
            // 
            // lblStatActiveLabel
            // 
            this.lblStatActiveLabel.AutoSize = true;
            this.lblStatActiveLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatActiveLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(105)))), ((int)(((byte)(130)))));
            this.lblStatActiveLabel.Location = new System.Drawing.Point(12, 8);
            this.lblStatActiveLabel.Name = "lblStatActiveLabel";
            this.lblStatActiveLabel.Size = new System.Drawing.Size(141, 19);
            this.lblStatActiveLabel.TabIndex = 0;
            this.lblStatActiveLabel.Text = "Chi nhánh hoạt động";
            // 
            // pnlStatInactive
            // 
            this.pnlStatInactive.BackColor = System.Drawing.Color.White;
            this.pnlStatInactive.Controls.Add(this.lblStatInactiveValue);
            this.pnlStatInactive.Controls.Add(this.lblStatInactiveLabel);
            this.pnlStatInactive.Location = new System.Drawing.Point(450, 10);
            this.pnlStatInactive.Margin = new System.Windows.Forms.Padding(10);
            this.pnlStatInactive.Name = "pnlStatInactive";
            this.pnlStatInactive.Padding = new System.Windows.Forms.Padding(15);
            this.pnlStatInactive.Size = new System.Drawing.Size(200, 70);
            this.pnlStatInactive.TabIndex = 2;
            // 
            // lblStatInactiveValue
            // 
            this.lblStatInactiveValue.AutoSize = true;
            this.lblStatInactiveValue.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatInactiveValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.lblStatInactiveValue.Location = new System.Drawing.Point(12, 30);
            this.lblStatInactiveValue.Name = "lblStatInactiveValue";
            this.lblStatInactiveValue.Size = new System.Drawing.Size(25, 30);
            this.lblStatInactiveValue.TabIndex = 1;
            this.lblStatInactiveValue.Text = "0";
            // 
            // lblStatInactiveLabel
            // 
            this.lblStatInactiveLabel.AutoSize = true;
            this.lblStatInactiveLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatInactiveLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(105)))), ((int)(((byte)(130)))));
            this.lblStatInactiveLabel.Location = new System.Drawing.Point(12, 8);
            this.lblStatInactiveLabel.Name = "lblStatInactiveLabel";
            this.lblStatInactiveLabel.Size = new System.Drawing.Size(150, 19);
            this.lblStatInactiveLabel.TabIndex = 0;
            this.lblStatInactiveLabel.Text = "Chi nhánh tạm dừng";
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.lblSearch);
            this.pnlActions.Controls.Add(this.btnAdd);
            this.pnlActions.Controls.Add(this.btnEdit);
            this.pnlActions.Controls.Add(this.btnDelete);
            this.pnlActions.Controls.Add(this.btnRefresh);
            this.pnlActions.Controls.Add(this.lblTotalBranches);
            this.pnlActions.Controls.Add(this.txtSearch);
            this.pnlActions.Controls.Add(this.btnSearch);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.Location = new System.Drawing.Point(20, 200);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(960, 70);
            this.pnlActions.TabIndex = 12;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(105)))), ((int)(((byte)(130)))));
            this.lblSearch.Location = new System.Drawing.Point(565, 19);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(59, 15);
            this.lblSearch.TabIndex = 8;
            this.lblSearch.Text = "Tìm kiếm:";
            // 
            // FrmBranch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 660);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "FrmBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản Lý Chi Nhánh";
            this.Load += new System.EventHandler(this.FrmBranch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBranches)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.pnlStats.ResumeLayout(false);
            this.flowLayoutPanelStats.ResumeLayout(false);
            this.pnlStatTotal.ResumeLayout(false);
            this.pnlStatTotal.PerformLayout();
            this.pnlStatActive.ResumeLayout(false);
            this.pnlStatActive.PerformLayout();
            this.pnlStatInactive.ResumeLayout(false);
            this.pnlStatInactive.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.pnlActions.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
