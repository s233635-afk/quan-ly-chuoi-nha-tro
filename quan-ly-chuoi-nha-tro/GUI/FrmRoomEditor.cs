using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmRoomEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private TextBox txtRoomNumber;
        private ComboBox cboBranch;
        private ComboBox cboSection;
        private ComboBox cboRoomType;
        private ComboBox cboStatus;
        private Button btnAddSection;
        private NumericUpDown numPrice;
        private NumericUpDown numFloor;
        private NumericUpDown numArea;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;
        private Button btnAutoPrice;

        private DataTable _branchTable;
        private DataTable _sectionTable;
        private DataTable _typeTable;
        private DataTable _statusTable;

        public int? SavedRoomId { get; private set; }

        public FrmRoomEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) => await LoadLookupAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "Thêm phòng" : "Cập nhật phòng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 520);
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

            int labelWidth = 170;
            int inputWidth = 470;
            int top = 6;
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

            txtRoomNumber = new TextBox();
            cboBranch = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboSection = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoomType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

            cboBranch.SelectedIndexChanged += async (s, e) => await ReloadSectionsAsync();
            cboRoomType.SelectedIndexChanged += (s, e) => TryAutoFillPrice();

            numPrice = new NumericUpDown { Minimum = 0, Maximum = 100000000000m, DecimalPlaces = 0, Increment = 100000m, ThousandsSeparator = true };
            numFloor = new NumericUpDown { Minimum = 0, Maximum = 1000, DecimalPlaces = 0, Increment = 1, ThousandsSeparator = true };
            numArea = new NumericUpDown { Minimum = 0, Maximum = 1000000m, DecimalPlaces = 2, Increment = 0.5m, ThousandsSeparator = true };

            btnAutoPrice = new Button { Text = "Lấy giá", Width = 90, Height = 28 };
            btnAutoPrice.Click += (s, e) => ForceAutoFillPrice();
            btnAutoPrice.FlatStyle = FlatStyle.Flat;
            btnAutoPrice.FlatAppearance.BorderSize = 1;

            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, AutoSize = true };

            pnlBody.Controls.Add(MakeLabel("Số phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(txtRoomNumber, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Chi nhánh (*)", top));
            pnlBody.Controls.Add(MakeInput(cboBranch, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Khu/Dãy (*)", top));
            var pnlSection = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 30 };
            cboSection.Parent = pnlSection;
            cboSection.Location = new Point(0, 0);
            cboSection.Width = inputWidth - 110;
            btnAddSection = new Button { Text = "Thêm khu/dãy", Width = 100, Height = 28 };
            btnAddSection.Parent = pnlSection;
            btnAddSection.Location = new Point(inputWidth - btnAddSection.Width, 1);
            btnAddSection.FlatStyle = FlatStyle.Flat;
            btnAddSection.FlatAppearance.BorderSize = 1;
            btnAddSection.Click += async (s, e) => await AddSectionAsync();
            pnlSection.Resize += (s, e) =>
            {
                cboSection.Width = pnlSection.Width - btnAddSection.Width - 10;
                btnAddSection.Location = new Point(pnlSection.Width - btnAddSection.Width, 1);
            };
            pnlBody.Controls.Add(pnlSection);
            top += line;

            pnlBody.Controls.Add(MakeLabel("Loại phòng (*)", top));
            pnlBody.Controls.Add(MakeInput(cboRoomType, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Trạng thái", top));
            pnlBody.Controls.Add(MakeInput(cboStatus, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Tầng", top));
            pnlBody.Controls.Add(MakeInput(numFloor, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Diện tích (m²)", top));
            pnlBody.Controls.Add(MakeInput(numArea, top));
            top += line;

            pnlBody.Controls.Add(MakeLabel("Giá phòng", top));
            var pnlPrice = new Panel { Location = new Point(left + labelWidth, top), Width = inputWidth, Height = 30 };
            numPrice.Parent = pnlPrice;
            numPrice.Location = new Point(0, 0);
            numPrice.Width = inputWidth - btnAutoPrice.Width - 10;
            btnAutoPrice.Parent = pnlPrice;
            btnAutoPrice.Location = new Point(inputWidth - btnAutoPrice.Width, 1);
            pnlPrice.Resize += (s, e) =>
            {
                numPrice.Width = pnlPrice.Width - btnAutoPrice.Width - 10;
                btnAutoPrice.Location = new Point(pnlPrice.Width - btnAutoPrice.Width, 1);
            };
            pnlBody.Controls.Add(pnlPrice);
            top += line;

            chkActive.Location = new Point(left + labelWidth, top + 4);
            pnlBody.Controls.Add(chkActive);

            btnSave = new Button
            {
                Text = "cập nhật",
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
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;

            btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
            btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(pnlBottom.ClientSize.Width - btnCancel.Width, 12);
                btnSave.Location = new Point(btnCancel.Left - btnSave.Width - 10, 12);
            };

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
        }

        private async System.Threading.Tasks.Task LoadLookupAsync()
        {
            try
            {
                _branchTable = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                _typeTable = await _bll.GetRoomTypesAsync();
                _statusTable = await _bll.GetRoomStatusesAsync();

                BindBranches();
                BindRoomTypes();
                BindStatuses();

                LoadExistingIntoControls();

                await ReloadSectionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tham chiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindBranches()
        {
            if (_branchTable == null) return;

            var dt = _branchTable.Copy();
            if (!dt.Columns.Contains("BranchDisplay"))
                dt.Columns.Add("BranchDisplay", typeof(string));

            TextFixer.FixDataTable(dt, "BranchCode", "BranchName", "Address");

            foreach (DataRow r in dt.Rows)
            {
                string code = dt.Columns.Contains("BranchCode") ? r["BranchCode"]?.ToString() : null;
                string name = dt.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null;
                string id = dt.Columns.Contains("BranchId") ? r["BranchId"]?.ToString() : null;
                string display = $"{code} - {name}".Trim(' ', '-');
                r["BranchDisplay"] = string.IsNullOrWhiteSpace(display) ? ("Chi nhánh " + id) : display;
            }

            cboBranch.DataSource = dt;
            cboBranch.DisplayMember = "BranchDisplay";
            cboBranch.ValueMember = "BranchId";
        }

        private void BindRoomTypes()
        {
            if (_typeTable == null) return;
            var filtered = RoomTypeCatalog.FilterToCanonicalTypes(_typeTable);
            cboRoomType.DataSource = filtered;
            cboRoomType.DisplayMember = filtered.Columns.Contains("RoomTypeName") ? "RoomTypeName" : filtered.Columns[0].ColumnName;
            cboRoomType.ValueMember = filtered.Columns.Contains("RoomTypeId") ? "RoomTypeId" : filtered.Columns[0].ColumnName;
        }

        private void BindStatuses()
        {
            if (_statusTable == null) return;
            TextFixer.FixDataTable(_statusTable, "StatusName", "Description");
            cboStatus.DataSource = _statusTable;
            cboStatus.DisplayMember = _statusTable.Columns.Contains("StatusName") ? "StatusName" : _statusTable.Columns[0].ColumnName;
            cboStatus.ValueMember = _statusTable.Columns.Contains("StatusId") ? "StatusId" : _statusTable.Columns[0].ColumnName;
        }

        private async System.Threading.Tasks.Task ReloadSectionsAsync()
        {
            try
            {
                int? branchId = cboBranch.SelectedValue is int b && b > 0 ? (int?)b : null;
                _sectionTable = await _bll.GetBranchSectionsAsync(branchId);

                if (_sectionTable == null)
                {
                    cboSection.DataSource = null;
                    return;
                }

                var dt = BranchSectionCatalog.NormalizeForRoomEditor(_sectionTable, branchId);

                // add placeholder row (id=0)
                var none = dt.Clone();
                var placeholder = none.NewRow();
                if (none.Columns.Contains("SectionId")) placeholder["SectionId"] = 0;
                if (none.Columns.Contains("BranchId")) placeholder["BranchId"] = branchId ?? 0;
                if (none.Columns.Contains("SectionCode")) placeholder["SectionCode"] = "";
                if (none.Columns.Contains("SectionName")) placeholder["SectionName"] = "";
                if (none.Columns.Contains("Description")) placeholder["Description"] = "";
                if (none.Columns.Contains("IsActive")) placeholder["IsActive"] = true;
                if (none.Columns.Contains("SectionDisplay")) placeholder["SectionDisplay"] = "Chọn khu/dãy...";
                none.Rows.Add(placeholder);

                foreach (DataRow r in dt.Rows) none.ImportRow(r);

                cboSection.DataSource = none;
                cboSection.DisplayMember = "SectionDisplay";
                cboSection.ValueMember = "SectionId";

                if (_existingRow != null && _existingRow.Table.Columns.Contains("SectionId"))
                {
                    if (int.TryParse(_existingRow["SectionId"]?.ToString(), out var sid) && sid > 0)
                        cboSection.SelectedValue = sid;
                    else
                        cboSection.SelectedValue = 0;
                }
                else
                {
                    cboSection.SelectedValue = 0;
                }
            }
            catch
            {
                // ignore lookup errors
            }
        }

        private async System.Threading.Tasks.Task AddSectionAsync()
        {
            int? branchId = cboBranch.SelectedValue is int b && b > 0 ? (int?)b : null;
            using (var frm = new FrmBranchSectionEditor(_bll, null, branchId))
            {
                if (frm.ShowDialog(this) != DialogResult.OK) return;
                await ReloadSectionsAsync();
                if (frm.SavedSectionId.HasValue)
                    cboSection.SelectedValue = frm.SavedSectionId.Value;
            }
        }

        private void LoadExistingIntoControls()
        {
            if (_existingRow == null) return;

            if (_existingRow.Table.Columns.Contains("RoomNumber"))
                txtRoomNumber.Text = _existingRow["RoomNumber"]?.ToString();

            if (_existingRow.Table.Columns.Contains("BranchId") && int.TryParse(_existingRow["BranchId"]?.ToString(), out var bid))
                cboBranch.SelectedValue = bid;

            if (_existingRow.Table.Columns.Contains("RoomTypeId") && int.TryParse(_existingRow["RoomTypeId"]?.ToString(), out var tid))
                cboRoomType.SelectedValue = tid;

            if (_existingRow.Table.Columns.Contains("CurrentStatusId") && int.TryParse(_existingRow["CurrentStatusId"]?.ToString(), out var sid))
                cboStatus.SelectedValue = sid;

            if (_existingRow.Table.Columns.Contains("Floor") && int.TryParse(_existingRow["Floor"]?.ToString(), out var fl) && fl >= 0)
                numFloor.Value = Math.Min(numFloor.Maximum, fl);

            if (_existingRow.Table.Columns.Contains("Area") && decimal.TryParse(_existingRow["Area"]?.ToString(), out var ar) && ar >= 0)
                numArea.Value = Math.Min(numArea.Maximum, ar);

            if (_existingRow.Table.Columns.Contains("RoomPrice") && decimal.TryParse(_existingRow["RoomPrice"]?.ToString(), out var pr) && pr > 0)
                numPrice.Value = Math.Min(numPrice.Maximum, pr);

            if (_existingRow.Table.Columns.Contains("IsActive"))
            {
                try { chkActive.Checked = Convert.ToBoolean(_existingRow["IsActive"]); } catch { }
            }
        }

        private void TryAutoFillPrice()
        {
            if (_existingRow != null) return;
            if (numPrice.Value > 0) return;
            ForceAutoFillPrice();
        }

        private void ForceAutoFillPrice()
        {
            if (cboRoomType.SelectedItem is DataRowView drv)
            {
                var row = drv.Row;
                if (row.Table.Columns.Contains("DefaultPrice") && decimal.TryParse(row["DefaultPrice"]?.ToString(), out var p) && p > 0)
                {
                    numPrice.Value = Math.Min(numPrice.Maximum, p);
                }
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string roomNumber = (txtRoomNumber.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                MessageBox.Show("Vui lòng nhập Số phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!(cboBranch.SelectedValue is int branchId) || branchId <= 0)
            {
                MessageBox.Show("Vui lòng chọn Chi nhánh.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? sectionId = cboSection.SelectedValue is int sid && sid > 0 ? (int?)sid : null;
            int? roomTypeId = cboRoomType.SelectedValue is int tid && tid > 0 ? (int?)tid : null;
            int? statusId = cboStatus.SelectedValue is int stid && stid > 0 ? (int?)stid : null;

            if (!sectionId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn Khu/Dãy.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!roomTypeId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn Loại phòng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? floor = numFloor.Value > 0 ? (int?)Convert.ToInt32(numFloor.Value) : null;
            decimal? area = numArea.Value > 0 ? (decimal?)numArea.Value : null;
            decimal? price = numPrice.Value > 0 ? (decimal?)numPrice.Value : null;
            bool? isActive = chkActive.Checked;

            try
            {
                if (_existingRow == null)
                {
                    var newId = await _bll.AddRoomAsync(roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive);
                    SavedRoomId = newId;
                }
                else
                {
                    int id = _existingRow.Table.Columns.Contains("RoomId") ? Convert.ToInt32(_existingRow["RoomId"]) : 0;
                    await _bll.UpdateRoomAsync(id, roomNumber, branchId, sectionId, roomTypeId, price, statusId, floor, area, isActive, null);
                    SavedRoomId = id;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

