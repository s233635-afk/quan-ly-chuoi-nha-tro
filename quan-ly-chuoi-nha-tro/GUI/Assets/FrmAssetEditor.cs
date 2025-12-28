using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmAssetEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private readonly DataRow _existingRow;

        private DataTable _roomTable;

        private TextBox txtCode;
        private TextBox txtName;
        private TextBox txtCategory;
        private ComboBox cboRoom;
        private NumericUpDown numQty;
        private ComboBox cboCondition;
        private DateTimePicker dtPurchase;
        private NumericUpDown numPrice;
        private CheckBox chkActive;
        private TextBox txtDesc;

        private Button btnSave;
        private Button btnCancel;

        public FrmAssetEditor(AdminDataBLL bll, DataRow existingRow = null)
        {
            _bll = bll;
            _existingRow = existingRow;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Text = _existingRow == null ? "➕ Thêm Tài Sản" : "✎ Cập Nhật Tài Sản";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(900, 680);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            // ===== HEADER =====
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(20, 12, 20, 12)
            };
            var lblTitle = new Label
            {
                Text = _existingRow == null ? "Thêm Tài Sản Mới" : "Chỉnh Sửa Tài Sản",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // ===== BOTTOM BUTTONS =====
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 12, 20, 12),
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnCancel = new ModernButton
            {
                Text = "❌ Hủy",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BaseColor = Color.White,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new ModernButton
            {
                Text = "✓ Lưu",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BaseColor = Color.FromArgb(0, 122, 204),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
            btnSave.Click += async (s, e) => await SaveAsync();

            var btnGroup = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            btnGroup.Controls.Add(btnCancel);
            btnGroup.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnGroup);

            // ===== BODY CONTENT =====
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 24, 24, 20),
                AutoScroll = true,
                BackColor = Color.White
            };

            txtCode = new TextBox { BorderStyle = BorderStyle.FixedSingle };
            txtName = new TextBox { BorderStyle = BorderStyle.FixedSingle };
            txtCategory = new TextBox { BorderStyle = BorderStyle.FixedSingle };
            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White };
            numQty = new NumericUpDown { Minimum = 1, Maximum = 100000, DecimalPlaces = 0, Value = 1, ThousandsSeparator = true, BorderStyle = BorderStyle.FixedSingle };
            cboCondition = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White };
            cboCondition.Items.AddRange(new object[] { "Tốt", "Bình thường", "Kém", "Hư hỏng" });
            cboCondition.SelectedIndex = 0;
            dtPurchase = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            numPrice = new NumericUpDown { Minimum = 0, Maximum = 100000000000, DecimalPlaces = 0, ThousandsSeparator = true, BorderStyle = BorderStyle.FixedSingle };
            chkActive = new CheckBox { Text = "✓ Kích hoạt", AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10) };
            txtDesc = new TextBox { Multiline = true, Height = 120, ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.FixedSingle };

            var tblBody = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 150F),
                    new ColumnStyle(SizeType.Percent, 100F)
                },
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            void AddField(string labelText, Control control)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Margin = new Padding(0, 12, 0, 0)
                };

                control.Margin = new Padding(0, 8, 0, 0);
                control.Anchor = control is CheckBox ? (AnchorStyles.Left | AnchorStyles.Top) : (AnchorStyles.Left | AnchorStyles.Right);
                if (control is TextBox tb)
                {
                    tb.Height = tb.Multiline ? 120 : 34;
                }
                else if (control is CheckBox chk)
                {
                    chk.Height = chk.PreferredSize.Height;
                }
                else
                {
                    control.Height = 34;
                }
                control.Dock = control is CheckBox ? DockStyle.None : DockStyle.Fill;

                int rowIndex = tblBody.RowCount;
                tblBody.RowCount++;
                tblBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tblBody.Controls.Add(lbl, 0, rowIndex);
                tblBody.Controls.Add(control, 1, rowIndex);
            }

            AddField("Mã Tài Sản (*)", txtCode);
            AddField("Tên Tài Sản (*)", txtName);
            AddField("Nhóm/Loại", txtCategory);
            AddField("Phòng", cboRoom);
            AddField("Số Lượng", numQty);
            AddField("Tình Trạng", cboCondition);
            AddField("Ngày Mua", dtPurchase);
            AddField("Giá Mua", numPrice);
            AddField("Trạng Thái", chkActive);
            AddField("Mô Tả", txtDesc);

            pnlBody.Controls.Add(tblBody);

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlHeader);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            await LoadRoomsAsync();

            if (_existingRow == null) return;

            txtCode.Text = ReadString(_existingRow, "AssetCode");
            txtName.Text = TextFixer.FixUtf8Mojibake(ReadString(_existingRow, "AssetName"));
            txtCategory.Text = TextFixer.FixUtf8Mojibake(ReadString(_existingRow, "Category"));

            int roomId = ReadInt(_existingRow, "RoomId");
            if (roomId > 0) { try { cboRoom.SelectedValue = roomId; } catch { } }

            int qty = ReadInt(_existingRow, "Quantity");
            if (qty >= numQty.Minimum && qty <= numQty.Maximum) numQty.Value = qty;

            string cond = ReadString(_existingRow, "Condition");
            if (!string.IsNullOrWhiteSpace(cond))
            {
                string vnCond = TextFixer.ToVietnameseCondition(cond);
                int idx = cboCondition.FindStringExact(vnCond);
                if (idx >= 0) cboCondition.SelectedIndex = idx;
                else
                {
                    // If not found in exact items, it might be a custom string or mixed case
                    idx = cboCondition.FindString(vnCond);
                    if (idx >= 0) cboCondition.SelectedIndex = idx;
                }
            }

            if (DateTime.TryParse(ReadString(_existingRow, "PurchaseDate"), out var pd))
            {
                dtPurchase.Checked = true;
                dtPurchase.Value = pd.Date;
            }
            else dtPurchase.Checked = false;

            decimal price = ReadDecimal(_existingRow, "PurchasePrice");
            if (price >= numPrice.Minimum && price <= numPrice.Maximum) numPrice.Value = price;

            chkActive.Checked = ReadBool(_existingRow, "IsActive") ?? true;
            txtDesc.Text = TextFixer.FixUtf8Mojibake(ReadString(_existingRow, "Description"));
        }

        private async System.Threading.Tasks.Task LoadRoomsAsync()
        {
            try
            {
                var dt = await _bll.GetRoomsAsync();
                TextFixer.FixDataTable(dt, "RoomNumber", "BranchName", "SectionName", "RoomTypeName");

                if (AdminBranchScope.IsEnabled && dt != null && dt.Columns.Contains("BranchId"))
                {
                    var branches = AdminBranchScope.Apply(await _bll.GetBranchesAsync());
                    var allowed = branches?.AsEnumerable()
                        .Select(r => r["BranchId"]?.ToString())
                        .Where(s => int.TryParse(s, out var id) && id > 0)
                        .Select(int.Parse)
                        .ToHashSet();

                    if (allowed != null && allowed.Count > 0)
                    {
                        var filtered = dt.Clone();
                        foreach (DataRow r in dt.Rows)
                        {
                            if (!int.TryParse(r["BranchId"]?.ToString(), out var bid)) continue;
                            if (!allowed.Contains(bid)) continue;
                            filtered.ImportRow(r);
                        }
                        dt = filtered;
                    }
                }

                _roomTable = new DataTable();
                _roomTable.Columns.Add("RoomId", typeof(int));
                _roomTable.Columns.Add("RoomDisplay", typeof(string));
                _roomTable.Rows.Add(0, "— Không chọn —");

                if (dt != null && dt.Columns.Contains("RoomId"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int id = 0;
                        try { id = Convert.ToInt32(r["RoomId"]); } catch { }
                        string roomNo = r.Table.Columns.Contains("RoomNumber") ? TextFixer.FixUtf8Mojibake(r["RoomNumber"]?.ToString()) : null;
                        string branch = r.Table.Columns.Contains("BranchName") ? TextFixer.FixUtf8Mojibake(r["BranchName"]?.ToString()) : null;
                        string display = roomNo;
                        if (!string.IsNullOrWhiteSpace(branch)) display = $"{roomNo} - {branch}";
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
                cboRoom.Items.Add("— Không chọn —");
                cboRoom.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên tài sản.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int roomId = GetSelectedId(cboRoom);
            int? roomIdNullable = roomId > 0 ? (int?)roomId : null;

            DateTime? purchaseDate = dtPurchase.Checked ? (DateTime?)dtPurchase.Value.Date : null;
            decimal? purchasePrice = numPrice.Value > 0 ? (decimal?)numPrice.Value : null;

            try
            {
                if (_existingRow == null)
                {
                    await _bll.AddAssetAsync(
                        code,
                        name,
                        txtCategory.Text.Trim(),
                        roomIdNullable,
                        Convert.ToInt32(numQty.Value),
                        cboCondition.Text,
                        purchaseDate,
                        purchasePrice,
                        txtDesc.Text.Trim(),
                        chkActive.Checked);
                }
                else
                {
                    int id = Convert.ToInt32(_existingRow["AssetId"]);
                    await _bll.UpdateAssetAsync(
                        id,
                        code,
                        name,
                        txtCategory.Text.Trim(),
                        roomIdNullable,
                        Convert.ToInt32(numQty.Value),
                        cboCondition.Text,
                        purchaseDate,
                        purchasePrice,
                        txtDesc.Text.Trim(),
                        chkActive.Checked);
                }

                AdminEvents.NotifyDataChanged();
                DataSyncManager.NotifyRoomsChanged();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu tài sản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return string.Empty;
            var v = row[col];
            return v == null || v == DBNull.Value ? string.Empty : v.ToString();
        }

        private static int ReadInt(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return 0;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0;
            if (int.TryParse(v.ToString(), out var i)) return i;
            try { return Convert.ToInt32(v); } catch { return 0; }
        }

        private static bool? ReadBool(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            if (v == null || v == DBNull.Value) return null;
            if (bool.TryParse(v.ToString(), out var b)) return b;
            try { return Convert.ToBoolean(v); } catch { return null; }
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
