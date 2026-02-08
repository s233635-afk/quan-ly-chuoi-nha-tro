using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    partial class FrmBranchDetail
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
            // Form
            Text = "Chi Nhánh";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 420);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

            // Form panel
            pnlForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 10)
            };

            // Form layout - 2 columns for compact design
            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                AutoSize = false
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            int rowHeight = 50;
            for (int i = 0; i < 6; i++)
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Description row

            // Row 0: Code + Name
            formLayout.Controls.Add(CreateFieldPanel("Mã chi nhánh *", out txtCode, 20), 0, 0);
            formLayout.Controls.Add(CreateFieldPanel("Tên chi nhánh *", out txtName, 255), 1, 0);

            // Row 1: Address (span 2)
            var addressPanel = CreateFieldPanel("Địa chỉ", out txtAddress, 500);
            formLayout.Controls.Add(addressPanel, 0, 1);
            formLayout.SetColumnSpan(addressPanel, 2);

            // Row 2: Phone + Hotline
            formLayout.Controls.Add(CreateFieldPanel("Điện thoại", out txtPhone, 20), 0, 2);
            formLayout.Controls.Add(CreateFieldPanel("Hotline", out txtHotline, 20), 1, 2);

            // Row 3: Hours (span 2)
            var hoursPanel = CreateFieldPanel("Giờ hoạt động", out txtHours, 100);
            formLayout.Controls.Add(hoursPanel, 0, 3);
            formLayout.SetColumnSpan(hoursPanel, 2);

            // Row 4: Description (span 2, multiline)
            var descPanel = CreateFieldPanel("Mô tả", out txtDescription, 500, true);
            formLayout.Controls.Add(descPanel, 0, 4);
            formLayout.SetColumnSpan(descPanel, 2);
            formLayout.SetRowSpan(descPanel, 2);

            // Row 6: Active checkbox
            chkActive = new CheckBox
            {
                Text = "Hoạt động",
                Checked = true,
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(3, 5, 0, 0)
            };
            formLayout.Controls.Add(chkActive, 0, 6);

            pnlForm.Controls.Add(formLayout);

            // Button panel
            pnlButtons = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            btnDelete = CreateButton("🗑 Xóa", Color.FromArgb(220, 53, 69));
            btnDelete.Dock = DockStyle.Left;
            btnDelete.Click += btnDelete_Click;

            var rightButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            btnSave = CreateButton("💾 Lưu", Color.FromArgb(40, 167, 69));
            btnSave.Click += btnSave_Click;

            btnCancel = CreateButton("Đóng", Color.FromArgb(108, 117, 125));
            btnCancel.Click += btnCancel_Click;

            rightButtons.Controls.Add(btnSave);
            rightButtons.Controls.Add(btnCancel);

            pnlButtons.Controls.Add(btnDelete);
            pnlButtons.Controls.Add(rightButtons);

            mainLayout.Controls.Add(pnlForm, 0, 0);
            mainLayout.Controls.Add(pnlButtons, 0, 1);

            Controls.Add(mainLayout);
            Load += FrmBranchDetail_Load;
        }

        private Panel CreateFieldPanel(string labelText, out TextBox textBox, int maxLength, bool multiline = false)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 8, 0)
            };

            var lbl = new Label
            {
                Text = labelText,
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Top
            };

            textBox = new TextBox
            {
                Dock = DockStyle.Top,
                MaxLength = maxLength,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = multiline,
                Height = multiline ? 60 : 25
            };

            panel.Controls.Add(textBox);
            panel.Controls.Add(lbl);

            return panel;
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 90,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private TextBox txtCode;
        private TextBox txtName;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private TextBox txtHotline;
        private TextBox txtHours;
        private TextBox txtDescription;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;
        private Button btnDelete;
        private Panel pnlForm;
        private Panel pnlButtons;
    }
}
