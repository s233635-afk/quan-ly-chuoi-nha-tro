using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmUtilityReadingEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private DataTable _roomTable;
        private DataView _roomView;
        private DataTable _typeTable;

        private TextBox txtRoomSearch;
        private ComboBox cboRoom;
        private ComboBox cboType;
        private DateTimePicker dtReadingDate;
        private NumericUpDown numPrev;
        private NumericUpDown numCurr;
        private NumericUpDown numUsage;
        private NumericUpDown numUnitPrice;
        private NumericUpDown numTotal;
        private TextBox txtNotes;

        private PictureBox picMeterElectric;
        private PictureBox picMeterWater;
        private Label lblImageHintElectric;
        private Label lblImageHintWater;
        private Button btnSelectImageElectric;
        private Button btnSelectImageWater;
        private Button btnClearImageElectric;
        private Button btnClearImageWater;
        private string _selectedElectricImagePath;
        private string _selectedWaterImagePath;
        private bool _removeElectricImageRequested;
        private bool _removeWaterImageRequested;

        private Label lblCalcHint;

        private Button btnSave;
        private Button btnCancel;
        private readonly int? _preselectRoomId;

        public FrmUtilityReadingEditor(AdminDataBLL bll, DataRow existingRow = null, int? preselectRoomId = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            _preselectRoomId = preselectRoomId;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm chỉ số" : "Cập nhật chỉ số";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(760, 520);
            BackColor = Color.White;

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

            int labelWidth = 190;
            int inputWidth = 460;
            int top = 10;
            int left = 6;
            int line = 34;

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

            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom.SelectedIndexChanged += (s, e) => LoadImageForDate();
            cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.SelectedIndexChanged += (s, e) => ApplyDefaultPriceFromType();
            cboType.SelectedIndexChanged += (s, e) => LoadImageForDate();

            dtReadingDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = true, Value = DateTime.Today };
            dtReadingDate.ValueChanged += (s, e) => LoadImageForDate();

            numPrev = MakeNumber();
            numCurr = MakeNumber();
            numUsage = MakeNumber(readOnly: true);
            numUnitPrice = MakeMoney();
            numTotal = MakeMoney(readOnly: true);

            numPrev.ValueChanged += (s, e) => Recalc();
            numCurr.ValueChanged += (s, e) => Recalc();
            numUnitPrice.ValueChanged += (s, e) => Recalc();

            txtNotes = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };
            lblCalcHint = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Tiêu thụ = chỉ số mới - chỉ số cũ; Thành tiền = tiêu thụ x đơn giá" };

            pnlBody.Controls.Add(MakeLabel("Tìm phòng", top));
            txtRoomSearch = new TextBox { Width = inputWidth };
            txtRoomSearch.TextChanged += (s, e) => ApplyRoomFilter();
            pnlBody.Controls.Add(MakeInput(txtRoomSearch, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(cboRoom, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Loại dịch vụ (*)", top));
            pnlBody.Controls.Add(MakeInput(cboType, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Ngày ghi", top));
            pnlBody.Controls.Add(MakeInput(dtReadingDate, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Chỉ số cũ", top));
            pnlBody.Controls.Add(MakeInput(numPrev, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Chỉ số mới (*)", top));
            pnlBody.Controls.Add(MakeInput(numCurr, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tiêu thụ", top));
            pnlBody.Controls.Add(MakeInput(numUsage, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Đơn giá", top));
            pnlBody.Controls.Add(MakeInput(numUnitPrice, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Thành tiền", top));
            pnlBody.Controls.Add(MakeInput(numTotal, top));
            top += line;

            lblCalcHint.Location = new Point(left + labelWidth, top + 2);
            pnlBody.Controls.Add(lblCalcHint);
            top += 26;

            pnlBody.Controls.Add(MakeLabel("Ghi chú", top));
            pnlBody.Controls.Add(MakeInput(txtNotes, top));
            top += 110;

            pnlBody.Controls.Add(MakeLabel("Ảnh đồng hồ điện (theo tháng)", top));
            picMeterElectric = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 200,
                Height = 140
            };
            picMeterElectric.Location = new Point(left + labelWidth, top);
            pnlBody.Controls.Add(picMeterElectric);

            lblImageHintElectric = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "Chọn ảnh đồng hồ điện để lưu theo tháng.",
                Location = new Point(left + labelWidth + 210, top + 4)
            };
            pnlBody.Controls.Add(lblImageHintElectric);

            btnSelectImageElectric = new Button
            {
                Text = "Chọn ảnh",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Location = new Point(left + labelWidth + 210, top + 34)
            };
            btnSelectImageElectric.FlatAppearance.BorderSize = 0;
            btnSelectImageElectric.Click += (s, e) => SelectImage(isElectric: true);
            pnlBody.Controls.Add(btnSelectImageElectric);

            btnClearImageElectric = new Button
            {
                Text = "Xóa ảnh",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                Location = new Point(left + labelWidth + 210, top + 74)
            };
            btnClearImageElectric.FlatAppearance.BorderSize = 0;
            btnClearImageElectric.Click += (s, e) => ClearSelectedImage(isElectric: true);
            pnlBody.Controls.Add(btnClearImageElectric);
            top += 160;

            pnlBody.Controls.Add(MakeLabel("Ảnh đồng hồ nước (theo tháng)", top));
            picMeterWater = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 200,
                Height = 140
            };
            picMeterWater.Location = new Point(left + labelWidth, top);
            pnlBody.Controls.Add(picMeterWater);

            lblImageHintWater = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "Chọn ảnh đồng hồ nước để lưu theo tháng.",
                Location = new Point(left + labelWidth + 210, top + 4)
            };
            pnlBody.Controls.Add(lblImageHintWater);

            btnSelectImageWater = new Button
            {
                Text = "Chọn ảnh",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Location = new Point(left + labelWidth + 210, top + 34)
            };
            btnSelectImageWater.FlatAppearance.BorderSize = 0;
            btnSelectImageWater.Click += (s, e) => SelectImage(isElectric: false);
            pnlBody.Controls.Add(btnSelectImageWater);

            btnClearImageWater = new Button
            {
                Text = "Xóa ảnh",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                Location = new Point(left + labelWidth + 210, top + 74)
            };
            btnClearImageWater.FlatAppearance.BorderSize = 0;
            btnClearImageWater.Click += (s, e) => ClearSelectedImage(isElectric: false);
            pnlBody.Controls.Add(btnClearImageWater);

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Lưu",
                Width = 110,
                Height = 34,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);
            btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.Width - btnCancel.Width - 12, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            await LoadRoomsAsync();
            await LoadTypesAsync();

            if (_existingRow == null)
            {
                ApplyDefaultPriceFromType(force: true);
                Recalc();
                LoadImageForDate();
                if (_preselectRoomId.HasValue)
                {
                    try { cboRoom.SelectedValue = _preselectRoomId.Value; } catch { }
                }
                return;
            }

            int roomId = ReadInt(_existingRow, "RoomId");
            int typeId = ReadInt(_existingRow, "UtilityTypeId");
            if (roomId > 0) { try { cboRoom.SelectedValue = roomId; } catch { } }
            if (typeId > 0) { try { cboType.SelectedValue = typeId; } catch { } }

            DateTime rd;
            if (DateTime.TryParse(ReadString(_existingRow, "ReadingDate"), out rd))
            {
                dtReadingDate.Checked = true;
                dtReadingDate.Value = rd.Date;
            }
            else
            {
                dtReadingDate.Checked = false;
            }

            numPrev.Value = Clamp(numPrev, ReadDecimal(_existingRow, "PreviousReading"));
            numCurr.Value = Clamp(numCurr, ReadDecimal(_existingRow, "CurrentReading"));
            numUsage.Value = Clamp(numUsage, ReadDecimal(_existingRow, "UsageAmount"));
            numUnitPrice.Value = Clamp(numUnitPrice, ReadDecimal(_existingRow, "UnitPrice"));
            numTotal.Value = Clamp(numTotal, ReadDecimal(_existingRow, "TotalCost"));

            txtNotes.Text = ReadString(_existingRow, "Notes") ?? string.Empty;

            Recalc();
            LoadImageForDate();
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            try
            {
                var dt = await _bll.GetRoomsAsync();

                var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                var allowedIds = AdminBranchScope.GetAllowedBranchIds(branches);
                dt = AdminBranchScope.FilterByBranchIds(dt, allowedIds);
                TextFixer.FixDataTable(dt, "RoomNumber", "BranchName", "StatusName");

                _roomTable = new DataTable();
                _roomTable.Columns.Add("RoomId", typeof(int));
                _roomTable.Columns.Add("RoomDisplay", typeof(string));
                _roomTable.Rows.Add(0, "— Chọn phòng —");

                if (dt != null && dt.Columns.Contains("RoomId"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["RoomId"]); } catch { }
                        string roomNo = r.Table.Columns.Contains("RoomNumber") ? r["RoomNumber"]?.ToString() : null;
                        string branch = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                        string status = r.Table.Columns.Contains("StatusName") ? r["StatusName"]?.ToString() : null;
                        string display = roomNo;
                        if (!string.IsNullOrWhiteSpace(branch)) display = $"{roomNo} - {branch}";
                        if (!string.IsNullOrWhiteSpace(status)) display = $"{display} ({status})";
                        if (string.IsNullOrWhiteSpace(display)) display = "Phòng " + id;
                        _roomTable.Rows.Add(id, display);
                    }
                }

                _roomView = _roomTable.DefaultView;
                cboRoom.DataSource = _roomView;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = "RoomId";
                cboRoom.SelectedValue = 0;
                ApplyRoomFilter();
            }
            catch
            {
                cboRoom.Items.Clear();
                cboRoom.Items.Add("— Chọn phòng —");
                cboRoom.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task LoadTypesAsync()
        {
            try
            {
                _typeTable = await _bll.GetUtilityTypesAsync();
                var view = _typeTable?.DefaultView;
                if (view != null && _typeTable.Columns.Contains("IsActive"))
                    view.RowFilter = "IsActive = true";

                var typeSelect = new DataTable();
                typeSelect.Columns.Add("UtilityTypeId", typeof(int));
                typeSelect.Columns.Add("TypeDisplay", typeof(string));
                typeSelect.Columns.Add("DefaultPrice", typeof(decimal));

                typeSelect.Rows.Add(0, "— Chọn loại —", 0m);
            if (_typeTable != null && _typeTable.Columns.Contains("UtilityTypeId"))
            {
                DataRow elecRow = null;
                DataRow waterRow = null;
                DataRow internetRow = null;

                foreach (DataRow r in view?.ToTable()?.Rows ?? _typeTable.Rows)
                {
                    string name = r.Table.Columns.Contains("UtilityName") ? r["UtilityName"]?.ToString() : null;
                    string code = r.Table.Columns.Contains("UtilityCode") ? r["UtilityCode"]?.ToString() : null;
                    string codeUpper = (code ?? string.Empty).ToUpperInvariant();
                    string nameLower = (name ?? string.Empty).ToLowerInvariant();

                    if (internetRow == null && (codeUpper.Contains("INTERNET") || codeUpper == "NET" || nameLower.Contains("internet")))
                        internetRow = r;
                    if (elecRow == null && (codeUpper.Contains("ELEC") || nameLower.Contains("dien")))
                        elecRow = r;
                    if (waterRow == null && (codeUpper.Contains("WATER") || nameLower.Contains("nuoc")))
                        waterRow = r;
                }

                var mainRow = elecRow ?? waterRow;
                if (mainRow != null)
                {
                    int id = 0;
                    try { id = Convert.ToInt32(mainRow["UtilityTypeId"]); } catch { }
                    decimal def = ReadDecimal(mainRow, "DefaultPrice");
                    typeSelect.Rows.Add(id, "Điện/Nước", def);
                }

                if (internetRow != null)
                {
                    int id = 0;
                    try { id = Convert.ToInt32(internetRow["UtilityTypeId"]); } catch { }
                    decimal def = ReadDecimal(internetRow, "DefaultPrice");
                    typeSelect.Rows.Add(id, "Internet", def);
                }

                if (typeSelect.Rows.Count <= 1)
                {
                    foreach (DataRow r in view?.ToTable()?.Rows ?? _typeTable.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["UtilityTypeId"]); } catch { }
                        string name = r.Table.Columns.Contains("UtilityName") ? r["UtilityName"]?.ToString() : null;
                        string code = r.Table.Columns.Contains("UtilityCode") ? r["UtilityCode"]?.ToString() : null;
                        decimal def = ReadDecimal(r, "DefaultPrice");
                        string display = string.IsNullOrWhiteSpace(code) ? name : $"{name} ({code})";
                        if (string.IsNullOrWhiteSpace(display)) display = "Loại " + id;
                        typeSelect.Rows.Add(id, display, def);
                        if (typeSelect.Rows.Count >= 3) break;
                    }
                }
            }

            cboType.DataSource = typeSelect;
                cboType.DisplayMember = "TypeDisplay";
                cboType.ValueMember = "UtilityTypeId";
                cboType.SelectedValue = 0;
            }
            catch
            {
                cboType.Items.Clear();
                cboType.Items.Add("— Chọn loại —");
                cboType.SelectedIndex = 0;
            }
        }

        private void ApplyDefaultPriceFromType(bool force = false)
        {
            if (cboType.SelectedItem is DataRowView drv && drv.Row.Table.Columns.Contains("DefaultPrice"))
            {
                decimal def = 0m;
                try { def = Convert.ToDecimal(drv.Row["DefaultPrice"]); } catch { }
                if (def > 0 && (force || numUnitPrice.Value == 0))
                    numUnitPrice.Value = Clamp(numUnitPrice, def);
            }
        }

        private void Recalc()
        {
            decimal prev = numPrev.Value;
            decimal curr = numCurr.Value;
            decimal usage = curr - prev;
            if (usage < 0) usage = 0;
            numUsage.Value = Clamp(numUsage, usage);

            decimal total = usage * numUnitPrice.Value;
            numTotal.Value = Clamp(numTotal, total);
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            int roomId = GetSelectedId(cboRoom);
            int typeId = GetSelectedId(cboType);

            if (roomId <= 0)
            {
                MessageBox.Show("Vui lòng chọn phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (typeId <= 0)
            {
                MessageBox.Show("Vui lòng chọn loại dịch vụ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numCurr.Value < numPrev.Value)
            {
                MessageBox.Show("Chỉ số mới phải >= chỉ số cũ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime? readingDate = dtReadingDate.Checked ? (DateTime?)dtReadingDate.Value.Date : null;
            decimal prev = numPrev.Value;
            decimal curr = numCurr.Value;
            decimal usage = curr - prev;
            decimal unitPrice = numUnitPrice.Value;
            decimal total = usage * unitPrice;

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddUtilityReadingAsync(
                        roomId, typeId, readingDate,
                        prev, curr, usage, unitPrice, total,
                        txtNotes.Text.Trim());
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["ReadingId"]);
                    await _bll.UpdateUtilityReadingAsync(
                        id, roomId, typeId, readingDate,
                        prev, curr, usage, unitPrice, total,
                        txtNotes.Text.Trim());
                }

                SaveImagesForDate(roomId, readingDate);
                AdminEvents.NotifyDataChanged();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu chỉ số: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyRoomFilter()
        {
            if (_roomView == null) return;
            string keyword = (txtRoomSearch?.Text ?? string.Empty).Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _roomView.RowFilter = string.Empty;
                return;
            }

            _roomView.RowFilter = $"RoomDisplay LIKE '%{keyword}%'";
        }

        private void LoadImageForDate()
        {
            int roomId = GetSelectedId(cboRoom);
            DateTime? date = dtReadingDate.Checked ? (DateTime?)dtReadingDate.Value.Date : null;
            if (roomId <= 0 || !date.HasValue)
            {
                SetMeterPreview(picMeterElectric, null);
                SetMeterPreview(picMeterWater, null);
                return;
            }

            string existingElectric = FindImagePathByKind(roomId, "elec", date.Value);
            string existingWater = FindImagePathByKind(roomId, "water", date.Value);

            if (string.IsNullOrWhiteSpace(existingElectric))
            {
                string legacy = FindLegacyImagePath(roomId, date.Value, isElectric: true);
                if (!string.IsNullOrWhiteSpace(legacy)) existingElectric = legacy;
            }
            if (string.IsNullOrWhiteSpace(existingWater))
            {
                string legacy = FindLegacyImagePath(roomId, date.Value, isElectric: false);
                if (!string.IsNullOrWhiteSpace(legacy)) existingWater = legacy;
            }

            _selectedElectricImagePath = existingElectric;
            _selectedWaterImagePath = existingWater;
            _removeElectricImageRequested = false;
            _removeWaterImageRequested = false;
            SetMeterPreview(picMeterElectric, existingElectric);
            SetMeterPreview(picMeterWater, existingWater);
        }

        private void SelectImage(bool isElectric)
        {
            using (var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = isElectric ? "Chọn ảnh đồng hồ điện" : "Chọn ảnh đồng hồ nước"
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                if (isElectric)
                {
                    _selectedElectricImagePath = ofd.FileName;
                    _removeElectricImageRequested = false;
                    SetMeterPreview(picMeterElectric, _selectedElectricImagePath);
                }
                else
                {
                    _selectedWaterImagePath = ofd.FileName;
                    _removeWaterImageRequested = false;
                    SetMeterPreview(picMeterWater, _selectedWaterImagePath);
                }
            }
        }

        private void ClearSelectedImage(bool isElectric)
        {
            if (isElectric)
            {
                _selectedElectricImagePath = null;
                _removeElectricImageRequested = true;
                SetMeterPreview(picMeterElectric, null);
            }
            else
            {
                _selectedWaterImagePath = null;
                _removeWaterImageRequested = true;
                SetMeterPreview(picMeterWater, null);
            }
        }

        private void SaveImagesForDate(int roomId, DateTime? readingDate)
        {
            if (roomId <= 0 || !readingDate.HasValue) return;
            SaveImageByKind(roomId, readingDate.Value, "elec", ref _selectedElectricImagePath, ref _removeElectricImageRequested);
            SaveImageByKind(roomId, readingDate.Value, "water", ref _selectedWaterImagePath, ref _removeWaterImageRequested);
        }

        private void SaveImageByKind(int roomId, DateTime readingDate, string kind, ref string selectedPath, ref bool removeRequested)
        {
            string existing = FindImagePathByKind(roomId, kind, readingDate);
            if (removeRequested && !string.IsNullOrWhiteSpace(existing) && File.Exists(existing))
            {
                try { File.Delete(existing); } catch { }
                removeRequested = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedPath) || !File.Exists(selectedPath))
                return;

            string folder = GetImageFolder();
            string key = BuildImageKey(roomId, kind, readingDate);
            string ext = Path.GetExtension(selectedPath);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            string dest = Path.Combine(folder, key + ext);

            try
            {
                if (string.Equals(selectedPath, dest, System.StringComparison.OrdinalIgnoreCase))
                    return;
                File.Copy(selectedPath, dest, true);
            }
            catch
            {
                // ignore copy errors
            }
        }

        private void SetMeterPreview(PictureBox target, string path)
        {
            if (target == null) return;
            try
            {
                if (target.Image != null)
                {
                    var old = target.Image;
                    target.Image = null;
                    old.Dispose();
                }
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    target.Image = null;
                    return;
                }

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    target.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                target.Image = null;
            }
        }

        private static string GetImageFolder()
        {
            string root = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "UtilityMeterImages");
            Directory.CreateDirectory(root);
            return root;
        }

        private static string BuildImageKey(int roomId, string kind, DateTime date)
        {
            return $"{roomId}_{kind}_{date:yyyyMM}";
        }

        private static string FindImagePathByKind(int roomId, string kind, DateTime date)
        {
            string folder = GetImageFolder();
            string key = BuildImageKey(roomId, kind, date);
            var files = Directory.GetFiles(folder, key + ".*");
            return files.Length > 0 ? files[0] : null;
        }

        private static string BuildLegacyImageKey(int roomId, int typeId, DateTime date)
        {
            return $"{roomId}_{typeId}_{date:yyyyMM}";
        }

        private static string FindLegacyImagePath(int roomId, int typeId, DateTime date)
        {
            string folder = GetImageFolder();
            string key = BuildLegacyImageKey(roomId, typeId, date);
            var files = Directory.GetFiles(folder, key + ".*");
            return files.Length > 0 ? files[0] : null;
        }

        private string FindLegacyImagePath(int roomId, DateTime date, bool isElectric)
        {
            int typeId = FindUtilityTypeId(isElectric);
            if (typeId <= 0) return null;
            return FindLegacyImagePath(roomId, typeId, date);
        }

        private int FindUtilityTypeId(bool isElectric)
        {
            if (_typeTable == null) return 0;
            foreach (DataRow r in _typeTable.Rows)
            {
                string name = r.Table.Columns.Contains("UtilityName") ? r["UtilityName"]?.ToString() : null;
                string code = r.Table.Columns.Contains("UtilityCode") ? r["UtilityCode"]?.ToString() : null;
                if (isElectric && IsElectric(code, name)) return ReadInt(r, "UtilityTypeId");
                if (!isElectric && IsWater(code, name)) return ReadInt(r, "UtilityTypeId");
            }
            return 0;
        }

        private static bool IsElectric(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("ELEC") || nameLower.Contains("dien") || nameLower.Contains("điện");
        }

        private static bool IsWater(string code, string name)
        {
            string codeUpper = (code ?? string.Empty).ToUpperInvariant();
            string nameLower = (name ?? string.Empty).ToLowerInvariant();
            return codeUpper.Contains("WATER") || nameLower.Contains("nuoc") || nameLower.Contains("nước");
        }

        private static NumericUpDown MakeNumber(bool readOnly = false)
        {
            var n = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100000000000,
                DecimalPlaces = 2,
                ThousandsSeparator = true,
                ReadOnly = readOnly,
                Increment = 1
            };
            return n;
        }

        private static NumericUpDown MakeMoney(bool readOnly = false)
        {
            var n = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100000000000,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                ReadOnly = readOnly,
                Increment = 1000
            };
            return n;
        }

        private static decimal Clamp(NumericUpDown n, decimal value)
        {
            if (value < n.Minimum) return n.Minimum;
            if (value > n.Maximum) return n.Maximum;
            return value;
        }

        private static int GetSelectedId(ComboBox cbo)
        {
            try
            {
                if (cbo.SelectedValue != null && int.TryParse(cbo.SelectedValue.ToString(), out var id))
                    return id;
            }
            catch { }
            return 0;
        }

        private static string ReadString(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }

        private static int ReadInt(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0;
            if (int.TryParse(v.ToString(), out var i)) return i;
            try { return Convert.ToInt32(v); } catch { return 0; }
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

    }
}
