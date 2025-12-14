using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmUtilityReadingEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private DataTable _roomTable;
        private DataTable _typeTable;

        private ComboBox cboRoom;
        private ComboBox cboType;
        private DateTimePicker dtReadingDate;
        private NumericUpDown numPrev;
        private NumericUpDown numCurr;
        private NumericUpDown numUsage;
        private NumericUpDown numUnitPrice;
        private NumericUpDown numTotal;
        private TextBox txtNotes;

        private Label lblCalcHint;

        private Button btnSave;
        private Button btnCancel;

        public FrmUtilityReadingEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
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
            cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.SelectedIndexChanged += (s, e) => ApplyDefaultPriceFromType();

            dtReadingDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = true, Value = DateTime.Today };

            numPrev = MakeNumber();
            numCurr = MakeNumber();
            numUsage = MakeNumber(readOnly: true);
            numUnitPrice = MakeMoney();
            numTotal = MakeMoney(readOnly: true);

            numPrev.ValueChanged += (s, e) => Recalc();
            numCurr.ValueChanged += (s, e) => Recalc();
            numUnitPrice.ValueChanged += (s, e) => Recalc();

            txtNotes = new TextBox { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };
            lblCalcHint = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Tiêu thụ = chỉ số mới - chỉ số cũ; Thành tiền = tiêu thụ × đơn giá" };

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

                cboRoom.DataSource = _roomTable;
                cboRoom.DisplayMember = "RoomDisplay";
                cboRoom.ValueMember = "RoomId";
                cboRoom.SelectedValue = 0;
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

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu chỉ số: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
