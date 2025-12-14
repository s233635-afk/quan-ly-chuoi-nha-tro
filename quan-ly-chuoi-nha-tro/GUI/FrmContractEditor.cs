using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmContractEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existing;

        private TextBox txtContractNumber;
        private ComboBox cboTenant;
        private ComboBox cboRoom;
        private DateTimePicker dtSign;
        private DateTimePicker dtStart;
        private DateTimePicker dtEnd;
        private NumericUpDown numRental;
        private NumericUpDown numDeposit;
        private ComboBox cboStatus;
        private TextBox txtPdfPath;
        private Button btnBrowsePdf;
        private TextBox txtTerms;
        private Button btnSave;
        private Button btnCancel;

        private DataTable _tenantTable;
        private DataTable _roomTable;

        public string ContractNumber { get; private set; }
        public int TenantId { get; private set; }
        public int RoomId { get; private set; }
        public DateTime? SignDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal? RentalPrice { get; private set; }
        public decimal? DepositRequired { get; private set; }
        public string Terms { get; private set; }
        public string ContractPdfPath { get; private set; }
        public string Status { get; private set; }

        public FrmContractEditor(AdminDataBLL bll, DataRow existing = null)
        {
            _bll = bll;
            _existing = existing;

            InitializeComponent();
            this.Load += async (s, e) => await LoadLookupAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _existing == null ? "Thêm hợp đồng" : "Cập nhật hợp đồng";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(720, 520);
            this.BackColor = Color.White;

            int labelWidth = 160;
            int inputWidth = 500;
            int top = 18;
            int left = 18;
            int line = 34;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = Color.White
            };

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 10),
                AutoScroll = true,
                BackColor = Color.White
            };

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

            txtContractNumber = new TextBox();
            cboTenant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            dtSign = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            dtStart = new DateTimePicker { Format = DateTimePickerFormat.Short };
            dtEnd = new DateTimePicker { Format = DateTimePickerFormat.Short };
            numRental = new NumericUpDown { Minimum = 0, Maximum = 100000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            numDeposit = new NumericUpDown { Minimum = 0, Maximum = 100000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Active", "Extended", "Terminated", "Expired", "Pending" });

            txtPdfPath = new TextBox();
            btnBrowsePdf = new Button { Text = "Chọn...", Width = 80, Height = 26 };
            btnBrowsePdf.Click += (s, e) => BrowsePdf();

            txtTerms = new TextBox { Multiline = true, Height = 140, ScrollBars = ScrollBars.Vertical };

            pnlBody.Controls.Add(MakeLabel("Số hợp đồng (*)", top));
            pnlBody.Controls.Add(MakeInput(txtContractNumber, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Khách thuê (*)", top));
            pnlBody.Controls.Add(MakeInput(cboTenant, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày ký", top));
            pnlBody.Controls.Add(MakeInput(dtSign, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày bắt đầu (*)", top));
            pnlBody.Controls.Add(MakeInput(dtStart, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày kết thúc (*)", top));
            pnlBody.Controls.Add(MakeInput(dtEnd, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Giá thuê/tháng", top));
            pnlBody.Controls.Add(MakeInput(numRental, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tiền cọc", top));
            pnlBody.Controls.Add(MakeInput(numDeposit, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("File PDF", top));
            var pnlPdf = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 28 };
            txtPdfPath.Parent = pnlPdf;
            txtPdfPath.Location = new Point(0, 0);
            txtPdfPath.Width = inputWidth - btnBrowsePdf.Width - 8;
            btnBrowsePdf.Parent = pnlPdf;
            btnBrowsePdf.Location = new Point(inputWidth - btnBrowsePdf.Width, 0);
            pnlBody.Controls.Add(pnlPdf);
            top += line;

            pnlBody.Controls.Add(MakeLabel("Điều khoản", top));
            pnlBody.Controls.Add(MakeInput(txtTerms, top));

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSave.Click += async (s, e) => await SaveAsync();
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadLookupAsync()
        {
            try
            {
                if (_bll == null)
                {
                    MessageBox.Show("Thiếu kết nối dữ liệu (AdminDataBLL).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                _tenantTable = await _bll.GetTenantsAsync();
                _roomTable = await _bll.GetRoomsAsync();

                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
                _roomTable = AdminBranchScope.FilterByBranchIds(_roomTable, allowedIds);

                cboTenant.DataSource = _tenantTable;
                cboTenant.DisplayMember = _tenantTable.Columns.Contains("FullName") ? "FullName" : _tenantTable.Columns[0].ColumnName;
                cboTenant.ValueMember = _tenantTable.Columns.Contains("TenantId") ? "TenantId" : _tenantTable.Columns[0].ColumnName;

                if (!_roomTable.Columns.Contains("RoomDisplay"))
                {
                    _roomTable.Columns.Add("RoomDisplay", typeof(string));
                }
                foreach (DataRow r in _roomTable.Rows)
                {
                    string roomNum = _roomTable.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : null;
                    string roomId = _roomTable.Columns.Contains("RoomId") ? r["RoomId"]?.ToString() : null;
                    string price = _roomTable.Columns.Contains("RoomPrice") ? r["RoomPrice"]?.ToString() : null;

                    string display = !string.IsNullOrWhiteSpace(roomNum) ? roomNum : (!string.IsNullOrWhiteSpace(roomId) ? ("Phòng " + roomId) : "Phòng");
                    if (!string.IsNullOrWhiteSpace(price))
                        display += $" (Giá: {price})";
                    r["RoomDisplay"] = display;
                }

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = _roomTable.Columns.Contains("RoomId") ? "RoomId" : _roomTable.Columns[0].ColumnName;

                LoadExisting();

                if (string.IsNullOrWhiteSpace(txtContractNumber.Text))
                    txtContractNumber.Text = $"HD-{DateTime.Now:yyyyMMdd-HHmmss}";

                if (string.IsNullOrWhiteSpace(cboStatus.Text))
                    cboStatus.SelectedItem = "Active";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tham chiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExisting()
        {
            if (_existing == null) return;

            txtContractNumber.Text = _existing.Table.Columns.Contains("ContractNumber") ? _existing["ContractNumber"]?.ToString() : string.Empty;

            if (_existing.Table.Columns.Contains("TenantId") && cboTenant.ValueMember != null)
                cboTenant.SelectedValue = _existing["TenantId"];
            if (_existing.Table.Columns.Contains("RoomId") && cboRoom.ValueMember != null)
                cboRoom.SelectedValue = _existing["RoomId"];

            if (DateTime.TryParse(_existing["SignDate"]?.ToString(), out var sign))
            {
                dtSign.Value = sign;
                dtSign.Checked = true;
            }
            else dtSign.Checked = false;

            if (DateTime.TryParse(_existing["StartDate"]?.ToString(), out var start))
                dtStart.Value = start;
            if (DateTime.TryParse(_existing["EndDate"]?.ToString(), out var end))
                dtEnd.Value = end;

            if (decimal.TryParse(_existing["RentalPrice"]?.ToString(), out var rent))
                numRental.Value = Math.Min(numRental.Maximum, rent);
            if (decimal.TryParse(_existing["DepositRequired"]?.ToString(), out var dep))
                numDeposit.Value = Math.Min(numDeposit.Maximum, dep);

            string status = _existing.Table.Columns.Contains("Status") ? _existing["Status"]?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(status) && cboStatus.Items.Contains(status))
                cboStatus.SelectedItem = status;

            txtPdfPath.Text = _existing.Table.Columns.Contains("ContractPdfPath") ? _existing["ContractPdfPath"]?.ToString() : string.Empty;
            txtTerms.Text = _existing.Table.Columns.Contains("Terms") ? _existing["Terms"]?.ToString() : string.Empty;
        }

        private void BrowsePdf()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "PDF (*.pdf)|*.pdf|All files (*.*)|*.*";
                ofd.Title = "Chọn file hợp đồng (PDF)";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    txtPdfPath.Text = ofd.FileName;
                }
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string contractNumber = (txtContractNumber.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(contractNumber))
            {
                MessageBox.Show("Nhập số hợp đồng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboTenant.SelectedValue == null || cboRoom.SelectedValue == null)
            {
                MessageBox.Show("Chọn khách thuê và phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime start = dtStart.Value.Date;
            DateTime end = dtEnd.Value.Date;
            if (end < start)
            {
                MessageBox.Show("Ngày kết thúc phải >= ngày bắt đầu.", "Sai dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ContractNumber = contractNumber;
            TenantId = Convert.ToInt32(cboTenant.SelectedValue);
            RoomId = Convert.ToInt32(cboRoom.SelectedValue);
            SignDate = dtSign.Checked ? (DateTime?)dtSign.Value.Date : null;
            StartDate = start;
            EndDate = end;
            RentalPrice = numRental.Value > 0 ? (decimal?)numRental.Value : null;
            DepositRequired = numDeposit.Value > 0 ? (decimal?)numDeposit.Value : null;
            Terms = (txtTerms.Text ?? string.Empty).Trim();
            ContractPdfPath = (txtPdfPath.Text ?? string.Empty).Trim();
            Status = string.IsNullOrWhiteSpace(cboStatus.Text) ? "Active" : cboStatus.Text;

            if (_bll == null)
            {
                MessageBox.Show("Thiếu kết nối dữ liệu (AdminDataBLL).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (_existing == null)
                {
                    await _bll.AddContractAsync(ContractNumber, TenantId, RoomId, SignDate, StartDate, EndDate, RentalPrice, DepositRequired, Terms, ContractPdfPath, Status);
                }
                else
                {
                    int id = Convert.ToInt32(_existing["ContractId"]);
                    await _bll.UpdateContractAsync(id, ContractNumber, TenantId, RoomId, SignDate, StartDate, EndDate, RentalPrice, DepositRequired, Terms, ContractPdfPath, Status);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hợp đồng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
