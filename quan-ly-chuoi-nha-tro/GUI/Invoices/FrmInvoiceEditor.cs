using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

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
        private NumericUpDown numAsset;
        private NumericUpDown numTaxRate;
        private NumericUpDown numTaxAmount;
        private Label lblTotal;
        private Label lblHint;
        private Button btnSave;
        private Button btnPayNow;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;
        private DataTable _assetTable;
        private DataTable _utilityTable;
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
            ClientSize = new Size(680, 750); // Slightly wider to avoid clipping
            BackColor = ModernTheme.Colors.Background;
            Font = ModernTheme.Fonts.NormalFont;

            // Main scrollable container
            var mainScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(25, 10, 25, 10)
            };

            // Internal panel to hold sections (forces width and allows Dock.Top)
            var pnlContent = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainScroll.Controls.Add(pnlContent);

            int labelWidth = 150;
            int inputWidth = 400;
            int rowHeight = 35;

            // Helper to create a row
            Panel MakeRow(string labelText, Control inputControl)
            {
                var row = new Panel { Dock = DockStyle.Top, Height = rowHeight, Margin = new Padding(0, 0, 0, 6) };
                var lbl = new Label
                {
                    Text = labelText,
                    Location = new Point(0, 6),
                    Width = labelWidth,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ModernTheme.Colors.TextSecondary,
                    Font = ModernTheme.Fonts.Regular(ModernTheme.Fonts.Normal)
                };
                inputControl.Location = new Point(labelWidth, 2);
                inputControl.Width = inputWidth;

                if (inputControl is TextBox tb) ModernTheme.StyleTextBox(tb);
                if (inputControl is ComboBox cb) ModernTheme.StyleComboBox(cb);
                
                row.Controls.Add(lbl);
                row.Controls.Add(inputControl);
                return row;
            }

            // Helper to create a section
            GroupBox MakeSection(string title, Color accentColor, int height)
            {
                var grp = new GroupBox
                {
                    Text = title,
                    Dock = DockStyle.Top,
                    Height = height,
                    ForeColor = accentColor,
                    Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Normal),
                    Margin = new Padding(0, 0, 0, 20),
                    Padding = new Padding(15, 25, 15, 10),
                    BackColor = ModernTheme.Colors.Background
                };
                return grp;
            }

            // --- 1. GENERAL INFO ---
            var grpGeneral = MakeSection("📝 THÔNG TIN CHUNG", ModernTheme.Colors.Primary, 185);
            
            txtInvoiceNumber = new TextBox { ReadOnly = true, BackColor = ModernTheme.Colors.Surface, TabStop = false };
            lblHint = new Label
            {
                Text = "Số hóa đơn được tạo tự động.",
                AutoSize = true,
                Font = ModernTheme.Fonts.Italic(ModernTheme.Fonts.Small),
                ForeColor = ModernTheme.Colors.TextTertiary,
                Location = new Point(labelWidth + 5, 28) // Relative to its position in the flow
            };

            cboTenant = new ComboBox();
            cboRoom = new ComboBox();

            grpGeneral.Controls.Add(MakeRow("Phòng (*)", cboRoom));
            grpGeneral.Controls.Add(MakeRow("Khách thuê (*)", cboTenant));
            // For general info, we add hint below invoice number manually
            var rowInv = MakeRow("Số hóa đơn", txtInvoiceNumber);
            rowInv.Height = 55;
            rowInv.Controls.Add(lblHint);
            grpGeneral.Controls.Add(rowInv);

            // --- 2. DATES ---
            var grpDates = MakeSection("📅 THỜI GIAN", ModernTheme.Colors.Warning, 185);
            
            dtInvoiceDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 200 };
            dtFromDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 200 };
            dtToDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 200 };
            dtDueDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 200 };

            grpDates.Controls.Add(MakeRow("Hạn thanh toán", dtDueDate));
            grpDates.Controls.Add(MakeRow("Kỳ đến ngày", dtToDate));
            grpDates.Controls.Add(MakeRow("Kỳ từ ngày", dtFromDate));
            grpDates.Controls.Add(MakeRow("Ngày lập h.đơn", dtInvoiceDate));

            // --- 3. FINANCIALS ---
            var grpCosts = MakeSection("💰 CHI TIẾT TÀI CHÍNH", ModernTheme.Colors.Success, 260);

            numRental = MakeMoney();
            numUtility = MakeMoney();
            numOther = MakeMoney();
            numAsset = MakeMoney();
            numTaxRate = MakePercent();
            numTaxAmount = MakeMoney();

            numRental.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numUtility.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numOther.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numAsset.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numTaxRate.ValueChanged += (s, e) => SyncTaxAmountFromRate();
            numTaxAmount.ValueChanged += (s, e) => SyncTaxRateFromAmount();

            grpCosts.Controls.Add(MakeRow("Tiền thuế", numTaxAmount));
            grpCosts.Controls.Add(MakeRow("Thuế (%)", numTaxRate));
            grpCosts.Controls.Add(MakeRow("Phí tài sản", numAsset));
            grpCosts.Controls.Add(MakeRow("Phí bảo trì/khác", numOther));
            grpCosts.Controls.Add(MakeRow("Điện/Nước/DV", numUtility));
            grpCosts.Controls.Add(MakeRow("Tiền phòng", numRental));

            // Add sections in REVERSE order for Dock.Top to maintain logical sequence
            pnlContent.Controls.Add(grpCosts);
            pnlContent.Controls.Add(grpDates);
            pnlContent.Controls.Add(grpGeneral);

            // --- BOTTOM PANEL (Total & Actions) ---
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = ModernTheme.Colors.Surface };
            var pnlSep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ModernTheme.Colors.Divider };
            pnlBottom.Controls.Add(pnlSep);

            lblTotal = new Label 
            { 
                Text = "0 VNĐ",
                Font = ModernTheme.Fonts.Bold(20),
                ForeColor = ModernTheme.Colors.Primary,
                AutoSize = true,
                Location = new Point(20, 35)
            };
            var lblTotalCap = new Label { Text = "TỔNG CỘNG:", Location = new Point(20, 15), AutoSize = true, Font = ModernTheme.Fonts.Bold(ModernTheme.Fonts.Small), ForeColor = ModernTheme.Colors.TextSecondary };

            btnCancel = new Button { Text = "Hủy", Width = 90 };
            btnSave = new Button { Text = "Lưu", Width = 100 };
            btnPayNow = new Button { Text = "Thanh toán ngay", Width = 140, Visible = _existing == null };

            ModernTheme.StyleOutlineButton(btnCancel, ModernTheme.Colors.TextSecondary);
            ModernTheme.StylePrimaryButton(btnSave);
            ModernTheme.StyleSuccessButton(btnPayNow);

            btnCancel.Location = new Point(ClientSize.Width - 110, 27);
            btnSave.Location = new Point(ClientSize.Width - 220, 27);
            btnPayNow.Location = new Point(ClientSize.Width - 370, 27);

            pnlBottom.Controls.Add(lblTotalCap);
            pnlBottom.Controls.Add(lblTotal);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnPayNow);

            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnSave.Click += async (s, e) => await SaveAsync(false);
            btnPayNow.Click += async (s, e) => await SaveAsync(true);

            Controls.Add(mainScroll);
            Controls.Add(pnlBottom);
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
                var t3 = _bll.GetAssetsAsync();
                var t4 = _bll.GetUtilitiesAsync();
                await Task.WhenAll(t1, t2, t3, t4);

                _tenantTable = t1.Result;
                _roomTable = t2.Result;
                _assetTable = t3.Result ?? new DataTable();
                _utilityTable = t4.Result ?? new DataTable();

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
                cboRoom.SelectedValueChanged += (s, e) => RefreshRoomDerivedValues();

                LoadExisting();
                if (_existing == null)
                {
                    txtInvoiceNumber.Text = "HD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    await LoadDefaultTaxRateAsync();
                }
                RefreshRoomDerivedValues();
                UpdateTotal();
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "LoadLookup", "Lỗi tải dữ liệu tham chiếu");
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

            numAsset.Value = 0;
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

        private decimal GetBaseAmount() => numRental.Value + numUtility.Value + numOther.Value + numAsset.Value;

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
                ToastNotification.Warning("Chọn khách thuê và phòng");
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
            decimal other = numOther.Value + numAsset.Value;
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
                ErrorLogger.HandleException(ex, "SaveInvoice", "Lỗi lưu hóa đơn");
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
                    ToastNotification.Info("Không tìm thấy hóa đơn vừa tạo để thanh toán");
                    return;
                }

                using (var frm = new FrmPaymentWithTenantInfo(_bll, row))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.HandleException(ex, "OpenPayment", "Lỗi mở thanh toán");
            }
        }

        private void RefreshRoomDerivedValues()
        {
            int roomId = GetSelectedRoomId();

            if (_existing == null && roomId > 0)
            {
                numRental.Value = ClampMoney(CalculateRoomRental(roomId));
                numUtility.Value = ClampMoney(CalculateUtilityCharge(roomId));
                numAsset.Value = ClampMoney(CalculateAssetCharge(roomId));
            }

            UpdateTotal();
        }

        private int GetSelectedRoomId()
        {
            if (cboRoom?.SelectedValue == null) return 0;
            try { return Convert.ToInt32(cboRoom.SelectedValue); }
            catch { return 0; }
        }

        private decimal CalculateAssetCharge(int roomId)
        {
            if (_assetTable == null || !_assetTable.Columns.Contains("RoomId")) return 0m;

            var rows = _assetTable.AsEnumerable()
                .Where(r => TryGetInt(r, "RoomId") == roomId)
                .Where(r =>
                {
                    var active = TryGetBool(r, "IsActive");
                    return active == null || active.Value;
                });

            decimal total = 0m;
            foreach (var asset in rows)
            {
                int qty = Math.Max(1, TryGetInt(asset, "Quantity"));
                decimal price = TryGetDecimal(asset, "PurchasePrice") ?? 0m;
                total += qty * price;
            }

            return total;
        }

        private decimal CalculateRoomRental(int roomId)
        {
            var roomRow = GetRoomRow(roomId);
            decimal basePrice = TryGetDecimal(roomRow, "RoomPrice") ?? 0m;
            return basePrice;
        }

        private decimal CalculateUtilityCharge(int roomId)
        {
            if (_utilityTable == null || !_utilityTable.Columns.Contains("RoomId")) return 0m;

            var rows = _utilityTable.AsEnumerable()
                .Where(r => TryGetInt(r, "RoomId") == roomId)
                .Where(r => TryGetBool(r, "IsActive") != false); // ignore explicitly inactive rows if flag exists

            var latestPerType = rows
                .GroupBy(r => TryGetInt(r, "UtilityTypeId"))
                .Select(g => g
                    .OrderByDescending(r => TryGetDate(r, "ReadingDate") ?? DateTime.MinValue)
                    .FirstOrDefault())
                .Where(r => r != null);

            decimal total = 0m;
            foreach (var reading in latestPerType)
            {
                total += TryGetDecimal(reading, "TotalCost") ?? 0m;
            }

            return total;
        }

        private DataRow GetRoomRow(int roomId)
        {
            if (_roomTable == null || !_roomTable.Columns.Contains("RoomId")) return null;
            return _roomTable.AsEnumerable().FirstOrDefault(r => TryGetInt(r, "RoomId") == roomId);
        }

        private static int TryGetInt(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return 0;
            return int.TryParse(row[column]?.ToString(), out var value) ? value : 0;
        }

        private static decimal? TryGetDecimal(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            return decimal.TryParse(row[column]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : (decimal?)null;
        }

        private static bool? TryGetBool(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            if (bool.TryParse(row[column]?.ToString(), out var value)) return value;
            return null;
        }

        private static DateTime? TryGetDate(DataRow row, string column)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(column)) return null;
            var value = row[column];
            if (value == null || value == DBNull.Value) return null;
            if (value is DateTime dt) return dt;
            return DateTime.TryParse(value.ToString(), out var parsed) ? parsed : (DateTime?)null;
        }
    }
}
