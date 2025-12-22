using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
        private NumericUpDown numTaxRate;
        private NumericUpDown numTaxAmount;
        private Label lblTotal;
        private Label lblHint;
        private Button btnSave;
        private Button btnPayNow;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;
        private bool _syncingTax;

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
            ClientSize = new Size(620, 500);

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
            numTaxRate = MakePercent();
            numTaxAmount = MakeMoney();

            numRental.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numUtility.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numOther.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numTaxRate.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numTaxAmount.ValueChanged += (s, e) => SyncTaxRateFromAmount();

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

            Controls.Add(MakeLabel("Thuế (%)", top));
            Controls.Add(MakeInput(numTaxRate, top));
            top += line;

            Controls.Add(MakeLabel("Tiền thuế", top));
            Controls.Add(MakeInput(numTaxAmount, top));
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
                Location = new Point(ClientSize.Width - 350, ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync(false);

            btnPayNow = new Button
            {
                Text = "Thanh toán ngay",
                Width = 120,
                Height = 32,
                Location = new Point(ClientSize.Width - 240, ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = _existing == null
            };
            btnPayNow.Click += async (s, e) => await SaveAsync(true);

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
            Controls.Add(btnPayNow);
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

        private static NumericUpDown MakePercent()
        {
            return new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 2,
                Increment = 0.1m,
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

                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
                _roomTable = AdminBranchScope.FilterByBranchIds(_roomTable, allowedIds);
                TextFixer.FixDataTable(_roomTable, "RoomNumber", "BranchName", "StatusName");

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
                if (_existing == null)
                    await LoadDefaultTaxRateAsync();
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

            _syncingTax = true;
            decimal baseAmount = numRental.Value + numUtility.Value + numOther.Value;
            decimal taxRate = ReadDecimal(_existing, "TaxRate");
            decimal taxAmount = ReadDecimal(_existing, "TaxAmount");
            if (taxRate <= 0m && taxAmount > 0m && baseAmount > 0m)
                taxRate = Math.Round(taxAmount / baseAmount * 100m, 2, MidpointRounding.AwayFromZero);

            numTaxRate.Value = ClampPercent(taxRate);
            numTaxAmount.Value = ClampMoney(taxAmount);
            _syncingTax = false;

            if (taxAmount <= 0m && taxRate > 0m)
                SyncTaxAmountFromRate();
        }

        private async Task LoadDefaultTaxRateAsync()
        {
            try
            {
                var settings = await _bll.GetSystemSettingsAsync();
                if (settings == null || !settings.Columns.Contains("SettingKey")) return;

                var row = settings.AsEnumerable()
                    .FirstOrDefault(r => string.Equals(r["SettingKey"]?.ToString(), "DefaultTaxRatePercent", StringComparison.OrdinalIgnoreCase));
                if (row == null) return;

                var value = row.Table.Columns.Contains("SettingValue") ? row["SettingValue"]?.ToString() : null;
                if (!TryParseDecimal(value, out var rate)) return;

                _syncingTax = true;
                numTaxRate.Value = ClampPercent(rate);
                _syncingTax = false;
                SyncTaxAmountFromRate();
            }
            catch
            {
                // Ignore default tax rate if settings are missing.
            }
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
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

        private decimal ClampPercent(decimal value)
        {
            if (value < (decimal)numTaxRate.Minimum) return numTaxRate.Minimum;
            if (value > (decimal)numTaxRate.Maximum) return numTaxRate.Maximum;
            return value;
        }

        private decimal GetBaseAmount()
        {
            return numRental.Value + numUtility.Value + numOther.Value;
        }

        private void SyncTaxAmountFromRate()
        {
            if (_syncingTax) return;
            _syncingTax = true;
            decimal taxAmount = CalculateTaxAmount();
            numTaxAmount.Value = ClampMoney(taxAmount);
            _syncingTax = false;
            UpdateTotal();
        }

        private void SyncTaxRateFromAmount()
        {
            if (_syncingTax) return;
            _syncingTax = true;
            decimal baseAmount = GetBaseAmount();
            decimal rate = baseAmount <= 0m
                ? 0m
                : Math.Round(numTaxAmount.Value / baseAmount * 100m, 2, MidpointRounding.AwayFromZero);
            numTaxRate.Value = ClampPercent(rate);
            _syncingTax = false;
            UpdateTotal();
        }

        private decimal CalculateTaxAmount()
        {
            decimal baseAmount = GetBaseAmount();
            decimal rate = numTaxRate.Value;
            return Math.Round(baseAmount * rate / 100m, 2, MidpointRounding.AwayFromZero);
        }

        private void UpdateTotal()
        {
            decimal total = GetBaseAmount() + numTaxAmount.Value;
            lblTotal.Text = $"{total:N0} VNĐ";
        }

        private async Task SaveAsync(bool payNow)
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
            decimal taxRate = numTaxRate.Value;

            try
            {
                int invoiceId = 0;
                if (_existing == null)
                {
                    invoiceId = await _bll.AddInvoiceAsync(invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rental, utility, other, dueDate, taxRate);
                }
                else
                {
                    invoiceId = Convert.ToInt32(_existing["InvoiceId"]);
                    await _bll.UpdateInvoiceAsync(invoiceId, invoiceNumber, tenantId, roomId, invoiceDate, fromDate, toDate, rental, utility, other, dueDate, taxRate);
                }

                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyRoomsChanged();

                if (payNow && invoiceId > 0)
                {
                    await OpenPaymentFormAsync(invoiceId);
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OpenPaymentFormAsync(int invoiceId)
        {
            try
            {
                var table = await _bll.GetInvoicesViewAsync();
                var row = table?.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["InvoiceId"]) == invoiceId);
                if (row == null)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn vừa tạo để thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
