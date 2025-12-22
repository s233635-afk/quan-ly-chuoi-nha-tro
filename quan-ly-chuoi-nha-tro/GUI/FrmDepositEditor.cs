using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmDepositEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existing;

        private ComboBox cboTenant;
        private ComboBox cboRoom;
        private NumericUpDown numAmount;
        private DateTimePicker dtDeposit;
        private ComboBox cboType;
        private ComboBox cboStatus;
        private ComboBox cboPaymentMethod;
        private NumericUpDown numReturned;
        private DateTimePicker dtReturned;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;

        public FrmDepositEditor(AdminDataBLL bll, DataRow existing = null)
        {
            _bll = bll;
            _existing = existing;
            InitializeComponent();
            this.Load += async (s, e) => await LoadLookupAsync();
        }

        private void InitializeComponent()
        {
            // Tiêu đề sẽ được cập nhật khi load dữ liệu (LoadExisting)
            this.Text = _existing == null ? "Thêm đặt phòng & cọc" : "Cập nhật đặt phòng & cọc";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(560, 500);

            int labelWidth = 150;
            int inputWidth = 320;
            int top = 20;
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

            cboTenant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            numAmount = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            dtDeposit = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            numReturned = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            dtReturned = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            txtNotes = new TextBox { Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };

            cboType.Items.AddRange(new object[] { "Đặt chỗ", "Chính thức" });
            cboStatus.Items.AddRange(new object[] { "Chờ xử lý", "Đã xác nhận", "Hoàn cọc", "Hủy" });
            cboPaymentMethod.Items.AddRange(new object[] { "Tiền mặt", "Thẻ" });
            cboPaymentMethod.SelectedIndex = 0;

            this.Controls.Add(MakeLabel("Khách thuê (*)", top));
            this.Controls.Add(MakeInput(cboTenant, top));
            top += line;

            this.Controls.Add(MakeLabel("Phòng (*)", top));
            this.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            this.Controls.Add(MakeLabel("Số tiền cọc", top));
            this.Controls.Add(MakeInput(numAmount, top));
            top += line;

            this.Controls.Add(MakeLabel("Ngày cọc", top));
            this.Controls.Add(MakeInput(dtDeposit, top));
            top += line;

            this.Controls.Add(MakeLabel("Loại cọc", top));
            this.Controls.Add(MakeInput(cboType, top));
            top += line;

            this.Controls.Add(MakeLabel("Trạng thái", top));
            this.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            this.Controls.Add(MakeLabel("Hình thức thanh toán", top));
            this.Controls.Add(MakeInput(cboPaymentMethod, top));
            top += line;

            this.Controls.Add(MakeLabel("Số tiền hoàn", top));
            this.Controls.Add(MakeInput(numReturned, top));
            top += line;

            this.Controls.Add(MakeLabel("Ngày hoàn", top));
            this.Controls.Add(MakeInput(dtReturned, top));
            top += line;

            this.Controls.Add(MakeLabel("Ghi chú", top));
            this.Controls.Add(MakeInput(txtNotes, top));
            top += 90;

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 32,
                Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 50),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            if (_existing == null)
            {
                cboStatus.SelectedIndex = 1;
                dtDeposit.Checked = true;
            }
        }

        private async System.Threading.Tasks.Task LoadLookupAsync()
        {
            try
            {
                _tenantTable = await _bll.GetTenantsAsync();
                _roomTable = await _bll.GetRoomsAsync();

                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
                _roomTable = AdminBranchScope.FilterByBranchIds(_roomTable, allowedIds);
                TextFixer.FixDataTable(_roomTable, "RoomNumber", "BranchName", "StatusName");

                cboTenant.DataSource = _tenantTable;
                cboTenant.DisplayMember = "FullName";
                cboTenant.ValueMember = "TenantId";

                if (!_roomTable.Columns.Contains("RoomDisplay"))
                    _roomTable.Columns.Add("RoomDisplay", typeof(string));
                foreach (DataRow r in _roomTable.Rows)
                {
                    if (_roomTable.Columns.Contains("RoomNumber") && r["RoomNumber"] != DBNull.Value)
                        r["RoomDisplay"] = r["RoomNumber"]?.ToString();
                    else if (_roomTable.Columns.Contains("RoomId") && r["RoomId"] != DBNull.Value)
                        r["RoomDisplay"] = "Phòng " + r["RoomId"];
                }

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = "RoomId";

                LoadExisting();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tham chiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExisting()
        {
            if (_existing == null) return;
            
            // Cập nhật tiêu đề với tên khách thuê
            if (_existing.Table.Columns.Contains("TenantId") && _tenantTable != null)
            {
                var tenantRow = _tenantTable.AsEnumerable()
                    .FirstOrDefault(r => r["TenantId"].ToString() == _existing["TenantId"].ToString());
                if (tenantRow != null && tenantRow.Table.Columns.Contains("FullName"))
                {
                    this.Text = $"Cập nhật cọc - {tenantRow["FullName"]}";
                }
            }
            
            if (_existing.Table.Columns.Contains("TenantId"))
                cboTenant.SelectedValue = _existing["TenantId"];
            if (_existing.Table.Columns.Contains("RoomId"))
                cboRoom.SelectedValue = _existing["RoomId"];

            if (decimal.TryParse(_existing["DepositAmount"]?.ToString(), out var amt))
                numAmount.Value = Math.Min(numAmount.Maximum, amt);

            if (DateTime.TryParse(_existing["DepositDate"]?.ToString(), out var d1))
            {
                dtDeposit.Value = d1;
                dtDeposit.Checked = true;
            }
            else dtDeposit.Checked = false;

            string depositType = _existing["DepositType"]?.ToString();
            if (depositType == "Booking") depositType = "Đặt chỗ";
            else if (depositType == "Official") depositType = "Chính thức";
            cboType.SelectedItem = depositType;

            string status = _existing["Status"]?.ToString();
            if (status == "Pending") status = "Chờ xử lý";
            else if (status == "Confirmed") status = "Đã xác nhận";
            else if (status == "Returned") status = "Hoàn cọc";
            else if (status == "Cancelled") status = "Hủy";
            cboStatus.SelectedItem = status;

            if (decimal.TryParse(_existing["ReturnedAmount"]?.ToString(), out var ret))
                numReturned.Value = Math.Min(numReturned.Maximum, ret);

            if (DateTime.TryParse(_existing["ReturnedDate"]?.ToString(), out var d2))
            {
                dtReturned.Value = d2;
                dtReturned.Checked = true;
            }
            else dtReturned.Checked = false;

            txtNotes.Text = _existing["Notes"]?.ToString();
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (cboTenant.SelectedValue == null || cboRoom.SelectedValue == null)
            {
                MessageBox.Show("Chọn khách thuê và phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra Ngày hoàn không được để trống
            if (!dtReturned.Checked && cboStatus.Text == "Hoàn cọc")
            {
                MessageBox.Show("Ngày hoàn không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtReturned.Focus();
                return;
            }

            // Kiểm tra Trạng thái không được để trống và không được là Chờ xử lý khi tạo mới
            if (string.IsNullOrWhiteSpace(cboStatus.Text))
            {
                MessageBox.Show("Trạng thái không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboStatus.Focus();
                return;
            }

            if ((cboStatus.Text == "Đã xác nhận" || cboStatus.Text == "Hoàn cọc") && cboPaymentMethod.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn hình thức thanh toán.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPaymentMethod.Focus();
                return;
            }

            int tenantId = Convert.ToInt32(cboTenant.SelectedValue);
            int roomId = Convert.ToInt32(cboRoom.SelectedValue);
            decimal amount = numAmount.Value;
            decimal? returned = numReturned.Value > 0 ? (decimal?)numReturned.Value : null;
            DateTime? depositDate = dtDeposit.Checked ? (DateTime?)dtDeposit.Value.Date : null;
            DateTime? returnedDate = dtReturned.Checked ? (DateTime?)dtReturned.Value.Date : null;
            string type = cboType.Text;
            string status = cboStatus.Text;
            string paymentMethod = cboPaymentMethod.SelectedItem?.ToString() == "Thẻ" ? "Card" : "Cash";
            string notes = txtNotes.Text.Trim();

            try
            {
                bool isNewDeposit = (_existing == null);
                int depositId;

                if (isNewDeposit)
                {
                    depositId = await _bll.AddDepositAsync(tenantId, roomId, amount, depositDate, type, status, returned, returnedDate, notes);
                    
                    // Lưu lịch sử khách hàng: nhân viên thêm khách vào phòng
                    DateTime checkInDate = depositDate ?? DateTime.Now.Date;
                    await _bll.AddTenantHistoryAsync(tenantId, roomId, checkInDate, null, "Active", 
                        $"[Đặt cọc] {type} - Tiền cọc: {amount:N0} VND - Ghi chú: {notes}");
                }
                else
                {
                    depositId = Convert.ToInt32(_existing["DepositId"]);
                    await _bll.UpdateDepositAsync(depositId, tenantId, roomId, amount, depositDate, type, status, returned, returnedDate, notes);
                    
                    // Nếu trạng thái thay đổi sang "Returned" hoặc "Cancelled", cập nhật lịch sử
                    string oldStatus = _existing["Status"]?.ToString() ?? "";
                    if (oldStatus != status && (status == "Returned" || status == "Cancelled"))
                    {
                        // Tìm lịch sử gần nhất của khách này trong phòng này
                        DataTable historyTable = await _bll.GetTenantHistoryAsync();
                        var recentHistory = historyTable?.AsEnumerable()
                            .Where(r => r["TenantId"]?.ToString() == tenantId.ToString() && 
                                       r["RoomId"]?.ToString() == roomId.ToString())
                            .OrderByDescending(r => r["CheckInDate"])
                            .FirstOrDefault();
                        
                        if (recentHistory != null && int.TryParse(recentHistory["HistoryId"]?.ToString(), out int historyId))
                        {
                            DateTime checkOutDate = returnedDate ?? DateTime.Now.Date;
                            await _bll.UpdateTenantHistoryAsync(historyId, roomId, 
                                DateTime.Parse(recentHistory["CheckInDate"].ToString()), 
                                checkOutDate, 
                                status == "Returned" ? "Completed" : "Cancelled", 
                                $"[Cập nhật cọc] {status} - Tiền hoàn: {returned:N0} VND");
                        }
                    }
                }

                if ((status == "Đã xác nhận" || status == "Hoàn cọc") && !HasLinkedPayment(notes))
                {
                    bool isRefund = status == "Hoàn cọc";
                    decimal linkAmount = isRefund ? (returned ?? amount) : amount;
                    DateTime linkDate = isRefund
                        ? (returnedDate ?? DateTime.Today)
                        : (depositDate ?? DateTime.Today);
                    string actionType = isRefund ? "Refund" : "Deposit";

                    var result = await _bll.CreateDepositPaymentAsync(
                        tenantId,
                        roomId,
                        linkAmount,
                        linkDate,
                        actionType,
                        paymentMethod,
                        $"{(isRefund ? "Hoàn cọc" : "Xác nhận cọc")} - DepositId: {depositId}");

                    notes = AppendPaymentNote(notes, result.Item1, result.Item2, actionType);
                    await _bll.UpdateDepositAsync(depositId, tenantId, roomId, amount, depositDate, type, status, returned, returnedDate, notes);

                    string actionTitle = isRefund ? "Hoàn cọc" : "Xác nhận cọc";
                    string message = $"{actionTitle}: {cboTenant.Text} | Phòng: {cboRoom.Text} | Số tiền: {linkAmount:N0} | Hình thức: {(paymentMethod == \"Card\" ? "Thẻ" : "Tiền mặt")}";
                    await _bll.AddNotificationAsync(null, actionTitle, message, "Unread");
                }

                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyInvoicesChanged();
                DataSyncManager.NotifyPaymentsChanged();
                DataSyncManager.NotifyRoomsChanged();
                DataSyncManager.NotifyTenantsChanged();
                DataSyncManager.NotifyContractsChanged();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đặt phòng/cọc: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool HasLinkedPayment(string notes)
        {
            return !string.IsNullOrWhiteSpace(notes) && notes.Contains("[INV:");
        }

        private static string AppendPaymentNote(string notes, int invoiceId, int paymentId, string actionType)
        {
            if (invoiceId <= 0 || paymentId <= 0)
                return notes;

            string tag = $"[INV:{invoiceId}|PAY:{paymentId}|{actionType}]";
            if (string.IsNullOrWhiteSpace(notes)) return tag;
            if (notes.Contains(tag)) return notes;
            return notes.TrimEnd() + " " + tag;
        }
    }
}
