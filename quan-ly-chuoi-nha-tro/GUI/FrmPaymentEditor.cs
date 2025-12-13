using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmPaymentEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _invoiceRow;

        private Label lblInvoice;
        private Label lblAmounts;
        private DateTimePicker dtPaymentDate;
        private NumericUpDown numAmount;
        private ComboBox cboMethod;
        private TextBox txtReference;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public FrmPaymentEditor(AdminDataBLL bll, DataRow invoiceRow)
        {
            _bll = bll;
            _invoiceRow = invoiceRow;
            InitializeComponent();
            Load += (s, e) => LoadInvoiceInfo();
        }

        private void InitializeComponent()
        {
            Text = "Thu tiền / Ghi nhận thanh toán";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 360);

            int labelWidth = 160;
            int inputWidth = 340;
            int top = 18;
            int left = 20;
            int line = 32;

            Label MakeLabel(string text, int y) => new Label
            {
                Text = text,
                Location = new Point(left, y),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Control MakeInput(Control ctl, int y)
            {
                ctl.Location = new Point(left + labelWidth, y);
                ctl.Width = inputWidth;
                return ctl;
            }

            lblInvoice = new Label { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(left, top) };
            lblAmounts = new Label { AutoSize = true, ForeColor = Color.DimGray, Location = new Point(left, top + 26) };

            top += 60;

            dtPaymentDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            numAmount = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            cboMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboMethod.Items.AddRange(new object[] { "Cash", "Transfer", "QR", "Check" });
            if (cboMethod.Items.Count > 0) cboMethod.SelectedIndex = 0;

            txtReference = new TextBox();
            txtNotes = new TextBox { Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical };

            Controls.Add(lblInvoice);
            Controls.Add(lblAmounts);

            Controls.Add(MakeLabel("Ngày thanh toán (*)", top));
            Controls.Add(MakeInput(dtPaymentDate, top));
            top += line;

            Controls.Add(MakeLabel("Số tiền (*)", top));
            Controls.Add(MakeInput(numAmount, top));
            top += line;

            Controls.Add(MakeLabel("Hình thức", top));
            Controls.Add(MakeInput(cboMethod, top));
            top += line;

            Controls.Add(MakeLabel("Mã giao dịch", top));
            Controls.Add(MakeInput(txtReference, top));
            top += line;

            Controls.Add(MakeLabel("Ghi chú", top));
            Controls.Add(MakeInput(txtNotes, top));

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 32,
                Location = new Point(ClientSize.Width - 220, ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 32,
                Location = new Point(ClientSize.Width - 110, ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void LoadInvoiceInfo()
        {
            if (_invoiceRow == null) return;

            int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
            string invoiceNo = _invoiceRow.Table.Columns.Contains("InvoiceNumber") ? _invoiceRow["InvoiceNumber"]?.ToString() : invoiceId.ToString();
            string tenant = _invoiceRow.Table.Columns.Contains("TenantName") ? _invoiceRow["TenantName"]?.ToString() : null;
            string room = _invoiceRow.Table.Columns.Contains("RoomNumber") ? _invoiceRow["RoomNumber"]?.ToString() : null;

            decimal total = ReadDecimal(_invoiceRow, "TotalAmount");
            decimal paid = ReadDecimal(_invoiceRow, "PaidAmount");
            decimal remaining = ReadDecimal(_invoiceRow, "RemainingAmount");

            lblInvoice.Text = $"Hóa đơn: {invoiceNo} | {tenant} | Phòng {room}";
            lblAmounts.Text = $"Tổng: {total:N0} | Đã thu: {paid:N0} | Còn nợ: {remaining:N0}";

            if (remaining > 0)
            {
                numAmount.Maximum = Math.Max(1, remaining);
                numAmount.Value = Math.Min(numAmount.Maximum, remaining);
            }
        }

        private static decimal ReadDecimal(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            if (v is decimal d) return d;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return 0m;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (_invoiceRow == null)
            {
                MessageBox.Show("Không xác định được hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int invoiceId = Convert.ToInt32(_invoiceRow["InvoiceId"]);
            decimal amount = numAmount.Value;
            if (amount <= 0)
            {
                MessageBox.Show("Số tiền thanh toán phải > 0.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _bll.AddPaymentAsync(
                    invoiceId,
                    dtPaymentDate.Value.Date,
                    amount,
                    cboMethod.Text,
                    txtReference.Text.Trim(),
                    txtNotes.Text.Trim());

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghi nhận thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

