using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Dialog for selecting year, month and due day for generating monthly invoices
    /// </summary>
    public class MonthlyInvoiceDialog : Form
    {
        private ComboBox cboYear;
        private ComboBox cboMonth;
        private NumericUpDown numDueDay;
        private Button btnOK;
        private Button btnCancel;

        public int SelectedYear { get; private set; }
        public int SelectedMonth { get; private set; }
        public int DueDay { get; private set; }

        public MonthlyInvoiceDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Tạo Hóa Đơn Tháng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(350, 200);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            // Year label and combo
            var lblYear = new Label { Text = "Năm:", Location = new Point(20, 20), AutoSize = true };
            cboYear = new ComboBox
            {
                Location = new Point(100, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 2; y <= currentYear + 2; y++)
            {
                cboYear.Items.Add(y);
            }
            cboYear.SelectedIndex = cboYear.Items.IndexOf(currentYear);

            // Month label and combo
            var lblMonth = new Label { Text = "Tháng:", Location = new Point(20, 60), AutoSize = true };
            cboMonth = new ComboBox
            {
                Location = new Point(100, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            string[] months = new string[12];
            for (int m = 1; m <= 12; m++)
            {
                months[m - 1] = $"{m:00} - {new DateTime(currentYear, m, 1).ToString("MMMM")}";
            }
            cboMonth.Items.AddRange(months);
            cboMonth.SelectedIndex = DateTime.Now.Month - 1;

            // Due Day label and numeric
            var lblDueDay = new Label { Text = "Hạn thanh toán:", Location = new Point(20, 100), AutoSize = true };
            numDueDay = new NumericUpDown
            {
                Location = new Point(100, 100),
                Width = 200,
                Minimum = 1,
                Maximum = 31,
                Value = 10
            };

            // OK Button
            btnOK = new Button
            {
                Text = "Tạo",
                Location = new Point(100, 150),
                Width = 90,
                Height = 36,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.None
            };
            btnOK.Click += (s, e) =>
            {
                SelectedYear = (int)cboYear.SelectedItem;
                SelectedMonth = cboMonth.SelectedIndex + 1;
                DueDay = (int)numDueDay.Value;
                DialogResult = DialogResult.OK;
                Close();
            };

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(200, 150),
                Width = 90,
                Height = 36,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lblYear);
            Controls.Add(cboYear);
            Controls.Add(lblMonth);
            Controls.Add(cboMonth);
            Controls.Add(lblDueDay);
            Controls.Add(numDueDay);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
        }
    }
}
