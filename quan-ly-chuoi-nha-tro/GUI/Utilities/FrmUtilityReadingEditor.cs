using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Simplified Utility Reading Form - Enter electricity & water readings in one form
    /// </summary>
    public class FrmUtilityReadingEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;
        private readonly int? _preselectRoomId;
        private DataTable _roomTable;
        private DataTable _typeTable;
        private int _electricTypeId;
        private int _waterTypeId;
        // UI Controls
        private ComboBox cboRoom;
        private DateTimePicker dtMonth;
        private bool _isInitializing = true;
        
        // Electric Section
        private Label lblElectricOld;
        private NumericUpDown numElectricNew;
        private Label lblElectricUsage;
        private Label lblElectricCost;
        
        // Water Section
        private Label lblWaterOld;
        private NumericUpDown numWaterNew;
        private Label lblWaterUsage;
        private Label lblWaterCost;
        
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        // Data
        private decimal _electricOldReading;
        private decimal _waterOldReading;
        private decimal _electricPrice;
        private decimal _waterPrice;
        public FrmUtilityReadingEditor(AdminDataBLL bll, DataRow existingRow = null, int? preselectRoomId = null)
        {
            _bll = bll ?? new AdminDataBLL();
            _existingRow = existingRow;
            _preselectRoomId = preselectRoomId;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }
        private void InitializeComponent()
        {
            Text = "Ghi chỉ số điện/nước";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 520);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true,
                BackColor = Color.White
            };
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 12, 20, 12),
                BackColor = Color.FromArgb(245, 247, 250)
            };
            int top = 0;
            int labelWidth = 120;
            int inputWidth = 340;
            // Helper to create label
            Label MakeLabel(string text, int y) => new Label
            {
                Text = text,
                Location = new Point(0, y + 5),
                Width = labelWidth,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
            // Helper to create read-only label (for displaying values)
            Label MakeValueLabel(int y) => new Label
            {
                Location = new Point(labelWidth, y + 5),
                Width = inputWidth,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204)
            };
            // Room Selection
            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            cboRoom = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(labelWidth, top),
                Width = inputWidth
            };
            cboRoom.SelectedIndexChanged += OnSelectionChanged;
            pnlBody.Controls.Add(cboRoom);
            top += 40;
            // Month Selection
            pnlBody.Controls.Add(MakeLabel("Tháng", top));
            dtMonth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/yyyy",
                ShowUpDown = true,
                Value = DateTime.Today,
                Location = new Point(labelWidth, top),
                Width = inputWidth
            };
            dtMonth.ValueChanged += OnSelectionChanged;
            pnlBody.Controls.Add(dtMonth);
            top += 50;
            // ELECTRIC SECTION
            var grpElectric = new GroupBox
            {
                Text = "⚡ ĐIỆN",
                Location = new Point(0, top),
                Width = 460,
                Height = 135,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            
            int grpTop = 25;
            grpElectric.Controls.Add(new Label { Text = "Chỉ số cũ:", Location = new Point(10, grpTop + 5), Width = 100 });
            lblElectricOld = MakeValueLabel(grpTop);
            lblElectricOld.Location = new Point(110, grpTop + 5);
            lblElectricOld.Width = 200;
            lblElectricOld.Text = "0 kWh";
            grpElectric.Controls.Add(lblElectricOld);
            grpTop += 30;
            grpElectric.Controls.Add(new Label { Text = "Chỉ số mới (*):", Location = new Point(10, grpTop + 5), Width = 100 });
            numElectricNew = new NumericUpDown
            {
                Location = new Point(110, grpTop),
                Width = 150,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };
            numElectricNew.ValueChanged += (s, e) => RecalculateElectric();
            grpElectric.Controls.Add(numElectricNew);
            grpTop += 30;
            lblElectricUsage = new Label
            {
                Location = new Point(10, grpTop + 5),
                Width = 440,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            grpElectric.Controls.Add(lblElectricUsage);
            grpTop += 20;
            lblElectricCost = new Label
            {
                Location = new Point(10, grpTop + 5),
                Width = 440,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
            };
            grpElectric.Controls.Add(lblElectricCost);
            pnlBody.Controls.Add(grpElectric);
            top += 145;

            // WATER SECTION
            var grpWater = new GroupBox
            {
                Text = "💧 NƯỚC",
                Location = new Point(0, top),
                Width = 460,
                Height = 135,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 188, 156)
            };
            grpTop = 25;
            grpWater.Controls.Add(new Label { Text = "Chỉ số cũ:", Location = new Point(10, grpTop + 5), Width = 100 });
            lblWaterOld = MakeValueLabel(grpTop);
            lblWaterOld.Location = new Point(110, grpTop + 5);
            lblWaterOld.Width = 200;
            lblWaterOld.Text = "0 m³";
            grpWater.Controls.Add(lblWaterOld);
            grpTop += 30;
            grpWater.Controls.Add(new Label { Text = "Chỉ số mới (*):", Location = new Point(10, grpTop + 5), Width = 100 });
            numWaterNew = new NumericUpDown
            {
                Location = new Point(110, grpTop),
                Width = 150,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };
            numWaterNew.ValueChanged += (s, e) => RecalculateWater();
            grpWater.Controls.Add(numWaterNew);
            grpTop += 30;
            lblWaterUsage = new Label
            {
                Location = new Point(10, grpTop + 5),
                Width = 440,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            grpWater.Controls.Add(lblWaterUsage);
            grpTop += 20;
            lblWaterCost = new Label
            {
                Location = new Point(10, grpTop + 5),
                Width = 440,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113)
            };
            grpWater.Controls.Add(lblWaterCost);
            pnlBody.Controls.Add(grpWater);
            top += 145;
            // Notes
            pnlBody.Controls.Add(MakeLabel("Ghi chú", top));
            txtNotes = new TextBox
            {
                Multiline = true,
                Height = 60,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(labelWidth, top),
                Width = inputWidth
            };
            pnlBody.Controls.Add(txtNotes);
            // Buttons
            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnSave = new Button
            {
                Text = "Lưu",
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);
            btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 20, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 20, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }
        private async System.Threading.Tasks.Task LoadAsync()
        {
            _isInitializing = true;
            try
            {
                await LoadRoomsAsync();
                await LoadPricesAsync();
                if (_preselectRoomId.HasValue)
                {
                    try { cboRoom.SelectedValue = _preselectRoomId.Value; }
                    catch { }
                }
                _isInitializing = false;
                await LoadPreviousReadingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isInitializing = false;
            }
        }
        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            var dt = await _bll.GetRoomsAsync();
            var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
            var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
            dt = AdminBranchScope.FilterByBranchIds(dt, allowedIds);
            TextFixer.FixDataTable(dt, "RoomNumber", "BranchName");
            _roomTable = new DataTable();
            _roomTable.Columns.Add("RoomId", typeof(int));
            _roomTable.Columns.Add("RoomDisplay", typeof(string));
            _roomTable.Rows.Add(0, "— Chọn phòng —");
            if (dt != null && dt.Columns.Contains("RoomId"))
            {
                foreach (DataRow r in dt.Rows)
                {
                    int id = Convert.ToInt32(r["RoomId"]);
                    string roomNo = r["RoomNumber"]?.ToString() ?? "";
                    string branch = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : "";
                    string display = string.IsNullOrWhiteSpace(branch) ? roomNo : $"{roomNo} - {branch}";
                    _roomTable.Rows.Add(id, display);
                }
            }
            
            cboRoom.DisplayMember = "RoomDisplay";
            cboRoom.ValueMember = "RoomId";
            cboRoom.DataSource = _roomTable;
            cboRoom.SelectedValue = 0;
        }
        private async void OnSelectionChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            await LoadPreviousReadingsAsync();
        }
        private async System.Threading.Tasks.Task LoadPricesAsync()
        {
            _typeTable = await _bll.GetUtilityTypesAsync();
            
            if (_typeTable == null || !_typeTable.Columns.Contains("UtilityTypeId"))
                return;
            // Find Electric and Water utility types
            foreach (DataRow r in _typeTable.Rows)
            {
                string name = r["UtilityName"]?.ToString()?.ToLowerInvariant() ?? "";
                string code = r.Table.Columns.Contains("UtilityCode") ? r["UtilityCode"]?.ToString()?.ToUpperInvariant() : "";
                if (_electricTypeId == 0 && (code.Contains("ELEC") || name.Contains("điện") || name.Contains("dien")))
                {
                    _electricTypeId = Convert.ToInt32(r["UtilityTypeId"]);
                    _electricPrice = r.Table.Columns.Contains("DefaultPrice") ? Convert.ToDecimal(r["DefaultPrice"]) : 3000m;
                }
                if (_waterTypeId == 0 && (code.Contains("WATER") || name.Contains("nước") || name.Contains("nuoc")))
                {
                    _waterTypeId = Convert.ToInt32(r["UtilityTypeId"]);
                    _waterPrice = r.Table.Columns.Contains("DefaultPrice") ? Convert.ToDecimal(r["DefaultPrice"]) : 15000m;
                }
            }
            // Fallback prices
            if (_electricPrice == 0) _electricPrice = 3000m;
            if (_waterPrice == 0) _waterPrice = 15000m;
        }
        private async System.Threading.Tasks.Task LoadPreviousReadingsAsync()
        {
            int roomId = GetSelectedRoomId();
            if (roomId <= 0)
            {
                ResetReadings();
                return;
            }
            try
            {
                var readings = await _bll.GetUtilityReadingsAsync();
                if (readings == null || !readings.Columns.Contains("RoomId"))
                {
                    ResetReadings();
                    return;
                }
                DateTime selectedMonth = new DateTime(dtMonth.Value.Year, dtMonth.Value.Month, 1);
                // Get last reading for electric
                var electricReadings = readings.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["RoomId"]) == roomId && 
                               Convert.ToInt32(r["UtilityTypeId"]) == _electricTypeId &&
                               r.Table.Columns.Contains("ReadingDate") && 
                               DateTime.TryParse(r["ReadingDate"]?.ToString(), out var d) && 
                               d < selectedMonth)
                    .OrderByDescending(r => DateTime.Parse(r["ReadingDate"].ToString()))
                    .FirstOrDefault();
                _electricOldReading = electricReadings != null && electricReadings.Table.Columns.Contains("CurrentReading")
                    ? Convert.ToDecimal(electricReadings["CurrentReading"])
                    : 0m;
                // Get last reading for water
                var waterReadings = readings.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["RoomId"]) == roomId && 
                               Convert.ToInt32(r["UtilityTypeId"]) == _waterTypeId &&
                               r.Table.Columns.Contains("ReadingDate") && 
                               DateTime.TryParse(r["ReadingDate"]?.ToString(), out var d) && 
                               d < selectedMonth)
                    .OrderByDescending(r => DateTime.Parse(r["ReadingDate"].ToString()))
                    .FirstOrDefault();
                _waterOldReading = waterReadings != null && waterReadings.Table.Columns.Contains("CurrentReading")
                    ? Convert.ToDecimal(waterReadings["CurrentReading"])
                    : 0m;
                UpdateDisplay();
            }
            catch
            {
                ResetReadings();
            }
        }
        private void ResetReadings()
        {
            _electricOldReading = 0m;
            _waterOldReading = 0m;
            numElectricNew.Value = 0m;
            numWaterNew.Value = 0m;
            UpdateDisplay();
        }
        private void UpdateDisplay()
        {
            lblElectricOld.Text = $"{_electricOldReading:N0} kWh";
            lblWaterOld.Text = $"{_waterOldReading:N0} m³";
            RecalculateElectric();
            RecalculateWater();
        }
        private void RecalculateElectric()
        {
            decimal usage = numElectricNew.Value - _electricOldReading;
            if (usage < 0) usage = 0;
            decimal cost = usage * _electricPrice;
            lblElectricUsage.Text = $"→ Tiêu thụ: {usage:N0} kWh × {_electricPrice:N0}đ";
            lblElectricCost.Text = $"= {cost:N0}đ";
        }
        private void RecalculateWater()
        {
            decimal usage = numWaterNew.Value - _waterOldReading;
            if (usage < 0) usage = 0;
            decimal cost = usage * _waterPrice;
            lblWaterUsage.Text = $"→ Tiêu thụ: {usage:N0} m³ × {_waterPrice:N0}đ";
            lblWaterCost.Text = $"= {cost:N0}đ";
        }
        private async System.Threading.Tasks.Task SaveAsync()
        {
            int roomId = GetSelectedRoomId();
            if (roomId <= 0)
            {
                MessageBox.Show("Vui lòng chọn phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numElectricNew.Value < _electricOldReading)
            {
                MessageBox.Show("Chỉ số điện mới phải >= chỉ số cũ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numWaterNew.Value < _waterOldReading)
            {
                MessageBox.Show("Chỉ số nước mới phải >= chỉ số cũ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DateTime readingDate = new DateTime(dtMonth.Value.Year, dtMonth.Value.Month, DateTime.DaysInMonth(dtMonth.Value.Year, dtMonth.Value.Month));
            try
            {
                // Save Electric Reading
                decimal electricUsage = numElectricNew.Value - _electricOldReading;
                decimal electricCost = electricUsage * _electricPrice;
                await _bll.AddUtilityReadingAsync(
                    roomId, _electricTypeId, readingDate,
                    _electricOldReading, numElectricNew.Value, electricUsage,
                    _electricPrice, electricCost, txtNotes.Text.Trim());
                // Save Water Reading
                decimal waterUsage = numWaterNew.Value - _waterOldReading;
                decimal waterCost = waterUsage * _waterPrice;
                await _bll.AddUtilityReadingAsync(
                    roomId, _waterTypeId, readingDate,
                    _waterOldReading, numWaterNew.Value, waterUsage,
                    _waterPrice, waterCost, txtNotes.Text.Trim());
                AdminEvents.NotifyDataChanged();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu chỉ số: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int GetSelectedRoomId()
        {
            if (cboRoom.SelectedValue is int id && id > 0)
                return id;
            return 0;
        }
    }
}