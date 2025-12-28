using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmSystemSettingsManager : Form
    {
        private const string SearchPlaceholder = "Tìm theo key/mô tả...";

        private readonly AdminDataBLL _bll = new AdminDataBLL();

        private DataTable _rawTable;
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Label _lblCount;
        private Label _lblEmpty;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        public FrmSystemSettingsManager()
        {
            InitializeComponent();
            AdminEvents.DataChanged += HandleAdminDataChanged;
            FormClosing += (s, e) => AdminEvents.DataChanged -= HandleAdminDataChanged;
        }

        private void InitializeComponent()
        {
            Text = "Cấu hình Hệ thống";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1180;
            Height = 680;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            _grid = MakeGrid();
            _grid.Dock = DockStyle.Fill;
            _grid.DoubleClick += async (s, e) => await EditSelectedAsync();

            _txtSearch = MakeSearchBox(SearchPlaceholder, () => ApplyFilter());
            _lblCount = new Label { AutoSize = true, Text = "Tổng: 0", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) };
            _lblEmpty = new Label
            {
                Text = "Chưa có cấu hình nào. Nhấn \"Thêm\" để tạo mới.",
                AutoSize = true,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Visible = false
            };

            _btnAdd = MakeButton("➕ Thêm", Color.FromArgb(0, 122, 204), async (s, e) => await AddNewAsync());
            _btnEdit = MakeButton("✎ Sửa", Color.FromArgb(0, 122, 204), async (s, e) => await EditSelectedAsync());
            _btnDelete = MakeButton("🗑 Xóa", Color.FromArgb(211, 47, 47), async (s, e) => await DeleteSelectedAsync());
            _btnRefresh = MakeButton("⟳ Tải lại", Color.FromArgb(0, 122, 204), async (s, e) => await LoadAsync());

            // Toolbar with title
            var pnlToolbar = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 70, 
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(0)
            };

            // Title bar
            var pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 8, 20, 8),
                BorderStyle = BorderStyle.None
            };
            var lblTitle = new Label
            {
                Text = "⚙️ Cấu hình Hệ thống",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Dock = DockStyle.Left
            };
            pnlTitleBar.Controls.Add(lblTitle);
            pnlToolbar.Controls.Add(pnlTitleBar);

            // Actions bar
            var pnlActionBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(12, 6, 12, 6),
                BorderStyle = BorderStyle.FixedSingle
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            actions.Controls.Add(_btnAdd);
            actions.Controls.Add(_btnEdit);
            actions.Controls.Add(_btnDelete);
            actions.Controls.Add(_btnRefresh);

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 2, 0, 0)
            };
            filters.Controls.Add(new Label { Text = "🔍 Tìm:", AutoSize = true, Margin = new Padding(0, 4, 6, 0), Font = new Font("Segoe UI", 9) });
            filters.Controls.Add(_txtSearch);
            filters.Controls.Add(new Label { Text = "  ", AutoSize = true });
            filters.Controls.Add(_lblCount);

            var barLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            barLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            barLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            barLayout.Controls.Add(actions, 0, 0);
            barLayout.Controls.Add(filters, 1, 0);
            filters.Anchor = AnchorStyles.Right;
            barLayout.Padding = new Padding(0);

            pnlActionBar.Controls.Add(barLayout);
            pnlToolbar.Controls.Add(pnlActionBar);

            var gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(12)
            };
            var gridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8)
            };
            gridCard.Controls.Add(_grid);
            _lblEmpty.Parent = gridCard;
            _lblEmpty.Location = new Point(0, 0);
            _lblEmpty.Anchor = AnchorStyles.None;
            gridCard.Resize += (s, e) =>
            {
                _lblEmpty.Location = new Point(
                    (gridCard.Width - _lblEmpty.Width) / 2,
                    (gridCard.Height - _lblEmpty.Height) / 2);
            };
            gridHost.Controls.Add(gridCard);

            Controls.Add(gridHost);
            Controls.Add(pnlToolbar);
            Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                _rawTable = await _bll.GetSystemSettingsAsync();
                _grid.DataSource = _rawTable;
                ApplyGridPresentation();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải cấu hình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void HandleAdminDataChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                await LoadAsync();
            }
            catch
            {
                // ignore refresh errors
            }
        }

        private void ApplyGridPresentation()
        {
            SetHeader("SettingKey", "Key");
            SetHeader("SettingValue", "Giá trị");
            SetHeader("Description", "Mô tả");
            SetDisplayOrder("SettingKey", "SettingValue", "Description");

            if (_grid.Columns.Contains("Description"))
                _grid.Columns["Description"].FillWeight = 200;
        }

        private void ApplyFilter()
        {
            if (_rawTable == null) return;

            string rawKeyword = (_txtSearch.Text ?? string.Empty).Trim();
            if (rawKeyword == SearchPlaceholder) rawKeyword = string.Empty;
            string keyword = rawKeyword.ToLowerInvariant();

            var rows = _rawTable.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
                rows = rows.Where(r => Contains(r, "SettingKey", keyword) || Contains(r, "Description", keyword));

            var filtered = rows.Any() ? rows.CopyToDataTable() : _rawTable.Clone();
            _grid.DataSource = filtered;
            _lblCount.Text = $"Tổng: {filtered.Rows.Count}";
            UpdateEmptyState(filtered);
        }

        private void UpdateEmptyState(DataTable table)
        {
            bool isEmpty = table == null || table.Rows.Count == 0;
            _lblEmpty.Visible = isEmpty;
            _grid.Visible = !isEmpty;
        }

        private async System.Threading.Tasks.Task AddNewAsync()
        {
            using (var frm = new FrmSystemSettingEditor(_bll))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAsync();
            }
        }

        private async System.Threading.Tasks.Task EditSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmSystemSettingEditor(_bll, row))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    await LoadAsync();
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            var row = GetCurrentRow();
            if (row == null)
            {
                MessageBox.Show("Chọn một dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string key = ReadString(row, "SettingKey");
            if (string.IsNullOrWhiteSpace(key)) return;

            if (MessageBox.Show($"Xóa cấu hình \"{key}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                await _bll.DeleteSystemSettingAsync(key);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa cấu hình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataRow GetCurrentRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.DataBoundItem == null) return null;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            return drv?.Row;
        }

        private static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 28 }
            };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 50);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(179, 211, 247);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.GridColor = Color.FromArgb(220, 230, 240);
            return g;
        }

        private static Button MakeButton(string text, Color backColor, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 110,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 6, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ColorAdjust(backColor, 10);
            b.Click += onClick;
            return b;
        }

        private static Color ColorAdjust(Color c, int delta)
        {
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, c.R + delta)),
                Math.Max(0, Math.Min(255, c.G + delta)),
                Math.Max(0, Math.Min(255, c.B + delta))
            );
        }

        private static TextBox MakeSearchBox(string placeholder, Action onChanged)
        {
            var tb = new TextBox
            {
                Width = 280,
                Height = 32,
                ForeColor = Color.Gray,
                Text = placeholder,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = string.Empty;
                    tb.ForeColor = Color.Black;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
            tb.TextChanged += (s, e) => onChanged?.Invoke();
            return tb;
        }

        private void SetHeader(string columnName, string headerText)
        {
            if (_grid.Columns.Contains(columnName))
                _grid.Columns[columnName].HeaderText = headerText;
        }

        private void SetDisplayOrder(params string[] order)
        {
            int idx = 0;
            foreach (var name in order)
            {
                if (_grid.Columns.Contains(name))
                {
                    _grid.Columns[name].DisplayIndex = idx;
                    idx++;
                }
            }
        }

        private static bool Contains(DataRow row, string column, string keywordLower)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column)) return false;
            var v = row[column];
            if (v == null || v == DBNull.Value) return false;
            return v.ToString().ToLowerInvariant().Contains(keywordLower);
        }

        private static string ReadString(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col)) return null;
            var v = row[col];
            return v == null || v == DBNull.Value ? null : v.ToString();
        }
    }
}

