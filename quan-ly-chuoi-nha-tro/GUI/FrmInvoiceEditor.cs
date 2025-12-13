using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmInvoiceEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existing;

        private TextBox txtInvoiceNumber;
        private ComboBox cboTenant;
        private ComboBox cboRoom;
        private DateTimePicker dtInvoiceDate;
        private DateTimePicker dtFromDate;
        private DateTimePicker dtToDate;
        private DateTimePicker dtDueDate;
        private NumericUpDown numRental;
        private NumericUpDown numUtility;
        private NumericUpDown numOther;
        private Label lblTotal;
        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;

        public FrmInvoiceEditor(AdminDataBLL bll, DataRow existing = null)
        {
            _bll = bll;
            _existing = existing;
            InitializeComponent();
            Load += async (s, e) => await LoadLookupAsync();
        }

        private void InitializeComponent()
        {
            Text = _existing == null ? "Thêm hóa đơn" : "Cập nhật hóa đơn";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 420);

            int labelWidth = 160;
            int inputWidth = 380;
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

            txtInvoiceNumber = new TextBox();
            lblHint = new Label
            {
                Text = "Để trống để hệ thống tự sinh số hóa đơn.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(left + labelWidth, top + 24)
            };

            cboTenant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            dtInvoiceDate = new DateTimePicker { Format = DateTimePickerFormat.Short };
            dtFromDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtToDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtDueDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            numRental = MakeMoney();
            numUtility = MakeMoney();
            numOther = MakeMoney();

            numRental.ValueChanged += (s, e) => UpdateTotal();
            numUtility.ValueChanged += (s, e) => UpdateTotal();
            numOther.ValueChanged += (s, e) => UpdateTotal();

            lblTotal = new Label { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            Controls.Add(MakeLabel("Số hóa đơn", top));
            Controls.Add(MakeInput(txtInvoiceNumber, top));
            Controls.Add(lblHint);
            top += line + 10;

            Controls.Add(MakeLabel("Khách thuê (*)", top));
            Controls.Add(MakeInput(cboTenant, top));
            top += line;

            Controls.Add(MakeLabel("Phòng (*)", top));
            Controls.Add(MakeInput(cboRoom, top));
            top += line;

            Controls.Add(MakeLabel("Ngày hóa đơn (*)", top));
            Controls.Add(MakeInput(dtInvoiceDate, top));
            top += line;

            Controls.Add(MakeLabel("Từ ngày", top));
            Controls.Add(MakeInput(dtFromDate, top));
            top += line;

            Controls.Add(MakeLabel("Đến ngày", top));
            Controls.Add(MakeInput(dtToDate, top));
            top += line;

            Controls.Add(MakeLabel("Hạn thanh toán", top));
            Controls.Add(MakeInput(dtDueDate, top));
            top += line;

            Controls.Add(MakeLabel("Tiền phòng", top));
            Controls.Add(MakeInput(numRental, top));
            top += line;

            Controls.Add(MakeLabel("Điện/Nước/DV", top));
            Controls.Add(MakeInput(numUtility, top));
            top += line;

            Controls.Add(MakeLabel("Phí khác", top));
            Controls.Add(MakeInput(numOther, top));
            top += line;

            Controls.Add(MakeLabel("Tổng cộng", top));
            lblTotal.Location = new Point(left + labelWidth, top + 6);
            Controls.Add(lblTotal);
            top += line;

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

        private static NumericUpDown MakeMoney()
        {
            return new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };
        }

        private async Task LoadLookupAsync()
        {
            try
            {
                var t1 = _bll.GetTenantsAsync();
                var t2 = _bll.GetRoomsAsync();
                await Task.WhenAll(t1, t2);

                _tenantTable = t1.Result;
                _roomTable = t2.Result;

                cboTenant.DataSource = _tenantTable;
                cboTenant.DisplayMember = _tenantTable.Columns.Contains("FullName") ? "FullName" : _tenantTable.Columns[0].ColumnName;
                cboTenant.ValueMember = _tenantTable.Columns.Contains("TenantId") ? "TenantId" : _tenantTable.Columns[0].ColumnName;

                if (!_roomTable.Columns.Contains("RoomDisplay"))
                    _roomTable.Columns.Add("RoomDisplay", typeof(string));

                foreach (DataRow r in _roomTable.Rows)
                {
                    string number = _roomTable.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : null;
                    string id = _roomTable.Columns.Contains("RoomId") ? r["RoomId"]?.ToString() : null;
                    r["RoomDisplay"] = !string.IsNullOrWhiteSpace(number) ? number : ("Phòng " + id);
                }

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = _roomTable.Columns.Contains("RoomId") ? "RoomId" : _roomTable.Columns[0].ColumnName;

                LoadExisting();
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tham chiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExisting()
        {
            if (_existing == null) return;

            if (_existing.Table.Columns.Contains("InvoiceNumber"))
                txtInvoiceNumber.Text = _existing["InvoiceNumber"]?.ToString();

            if (_existing.Table.Columns.Contains("TenantId") && cboTenant.DataSource != null)
                cboTenant.SelectedValue = _existing["TenantId"];

            if (_existing.Table.Columns.Contains("RoomId") && cboRoom.DataSource != null)
                cboRoom.SelectedValue = _existing["RoomId"];

            if (TryReadDate(_existing, "InvoiceDate", out var invDate))
                dtInvoiceDate.Value = invDate;

            if (TryReadDate(_existing, "FromDate", out var fromDate))
            {
                dtFromDate.Value = fromDate;
                dtFromDate.Checked = true;
            }
            else dtFromDate.Checked = false;

            if (TryReadDate(_existing, "ToDate", out var toDate))
            {
                dtToDate.Value = toDate;
                dtToDate.Checked = true;
            }
            else dtToDate.Checked = false;

            if (TryReadDate(_existing, "DueDate", out var dueDate))
            {
                dtDueDate.Value = dueDate;
                dtDueDate.Checked = true;
            }
            else dtDueDate.Checked = false;

            numRental.Value = ClampMoney(ReadDecimal(_existing, "RentalCost"));
            numUtility.Value = ClampMoney(ReadDecimal(_existing, "UtilityCost"));
            numOther.Value = ClampMoney(ReadDecimal(_existing, "OtherCost"));
        }

        private static bool TryReadDate(DataRow row, string col, out DateTime date)
        {
            date = default;
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return false;
            var v = row[col];
            if (v == null || v == DBNull.Value) return false;
            if (v is DateTime dt)
            {
                date = dt.Date;
                return true;
            }
            return DateTime.TryParse(v.ToString(), out date);
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

        private decimal ClampMoney(decimal value)
        {
            if (value < (decimal)numRental.Minimum) return numRental.Minimum;
            if (value > (decimal)numRental.Maximum) return numRental.Maximum;
            return value;
        }

        private void UpdateTotal()
        {
            decimal total = numRental.Value + numUtility.Value + numOther.Value;
            lblTotal.Text = $"{total:N0} VNĐ";
        }

        private async Task SaveAsync()
        {
            if (cboTenant.SelectedValue == null || cboRoom.SelectedValue == null)
            {
                MessageBox.Show("Chọn khách thuê và phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int tenantId = Convert.ToInt32(cboTenant.SelectedValue);
            int roomId = Convert.ToInt32(cboRoom.SelectedValue);

            string invoiceNumber = string.IsNullOrWhiteSpace(txtInvoiceNumber.Text) ? null : txtInvoiceNumber.Text.Trim();
            DateTime invoiceDate = dtInvoiceDate.Value.Date;
            DateTime? fromDate = dtFromDate.Checked ? (DateTime?)dtFromDate.Value.Date : null;
            DateTime? toDate = dtToDate.Checked ? (DateTime?)dtToDate.Value.Date : null;
            DateTime? dueDate = dtDueDate.Checked ? (DateTime?)dtDueDate.Value.Date : null;

            decimal rental = numRental.Value;
            decimal utility = numUtility.Value;
            decimal other = numOther.Value;

            try
            {
                if (_existing == null)
                {
                    await _bll.AddInvoiceAsync(invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rental, utility, other, dueDate);
                }
                else
                {
                    int invoiceId = Convert.ToInt32(_existing["InvoiceId"]);
                    await _bll.UpdateInvoiceAsync(invoiceId, invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rental, utility, other, dueDate);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

