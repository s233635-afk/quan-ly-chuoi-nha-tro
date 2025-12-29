using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Rooms.Components
{
    /// <summary>
    /// Room filter panel component - search, status filter, display limit
    /// Extracted from FrmRoomManager InitializeComponent (lines 124-163, 614-637)
    /// </summary>
    public class RoomFilterPanel : Panel
    {
        public event EventHandler FilterChanged;

        private const string SearchPlaceholder = "TĂ¬m theo sá»‘ phĂ²ng/loáº¡i...";
        
        private TextBox _txtSearch;
        private ComboBox _cboStatus;
        private NumericUpDown _numDisplayLimit;
        private Button _btnSearch;
        private Button _btnRefresh;

        public string SearchText
        {
            get
            {
                var text = (_txtSearch.Text ?? string.Empty).Trim();
                return text == SearchPlaceholder ? string.Empty : text;
            }
        }

        public int? SelectedStatusId
        {
            get
            {
                if (_cboStatus.SelectedValue is int statusId && statusId > 0)
                    return statusId;
                return null;
            }
        }

        public int DisplayLimit => (int)_numDisplayLimit.Value;

        public RoomFilterPanel(int defaultDisplayLimit = 20)
        {
            InitializePanel(defaultDisplayLimit);
        }

        private void InitializePanel(int defaultDisplayLimit)
        {
            Dock = DockStyle.Top;
            Height = 80;
            Padding = new Padding(12, 10, 12, 10);
            BackColor = Color.White;

            var toolbarLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var filters = CreateFiltersPanel(defaultDisplayLimit);
            var actions = CreateActionsPanel();

            toolbarLayout.Controls.Add(filters, 0, 0);
            toolbarLayout.Controls.Add(actions, 1, 0);

            Controls.Add(toolbarLayout);
        }

        private FlowLayoutPanel CreateFiltersPanel(int defaultDisplayLimit)
        {
            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var lblSearch = new Label
            {
                Text = "TĂ¬m kiáº¿m:",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            };

            _txtSearch = new TextBox
            {
                Width = 200,
                ForeColor = Color.Gray,
                Text = SearchPlaceholder,
                Margin = new Padding(0, 4, 10, 0)
            };
            _txtSearch.GotFocus += OnSearchGotFocus;
            _txtSearch.LostFocus += OnSearchLostFocus;
            _txtSearch.KeyDown += OnSearchKeyDown;

            var lblStatus = new Label
            {
                Text = "Tráº¡ng thĂ¡i:",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            };

            _cboStatus = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 10, 0)
            };
            _cboStatus.SelectedIndexChanged += (s, e) => OnFilterChanged();

            var lblLimit = new Label
            {
                Text = "Hiá»ƒn thá»‹:",
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 0)
            };

            _numDisplayLimit = new NumericUpDown
            {
                Width = 60,
                Minimum = 0,
                Maximum = 9999,
                Value = defaultDisplayLimit,
                Margin = new Padding(0, 4, 10, 0),
                ThousandsSeparator = true
            };
            _numDisplayLimit.ValueChanged += (s, e) => OnFilterChanged();

            filters.Controls.Add(lblSearch);
            filters.Controls.Add(_txtSearch);
            filters.Controls.Add(lblStatus);
            filters.Controls.Add(_cboStatus);
            filters.Controls.Add(lblLimit);
            filters.Controls.Add(_numDisplayLimit);

            return filters;
        }

        private FlowLayoutPanel CreateActionsPanel()
        {
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 6, 0)
            };

            _btnSearch = new ModernButton
            {
                Text = "TĂ¬m",
                Width = 80,
                Height = 32,
                BaseColor = Color.FromArgb(0, 122, 204),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 6, 0)
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += (s, e) => OnFilterChanged();

            _btnRefresh = new ModernButton
            {
                Text = "LĂ m má»›i",
                Width = 88,
                Height = 32,
                BaseColor = Color.FromArgb(40, 167, 69),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 6, 0)
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;

            actions.Controls.Add(_btnSearch);
            actions.Controls.Add(_btnRefresh);

            return actions;
        }

        public void PopulateStatuses(DataTable statuses)
        {
            var dt = new DataTable();
            dt.Columns.Add("StatusId", typeof(int));
            dt.Columns.Add("StatusName", typeof(string));
            dt.Rows.Add(0, "Táº¥t cáº£");

            if (statuses != null && statuses.Columns.Contains("StatusId"))
            {
                foreach (DataRow r in statuses.Rows)
                {
                    if (!int.TryParse(r["StatusId"]?.ToString(), out var id)) continue;
                    dt.Rows.Add(id, r["StatusName"]?.ToString());
                }
            }

            _cboStatus.DisplayMember = "StatusName";
            _cboStatus.ValueMember = "StatusId";
            _cboStatus.DataSource = dt;
            if (dt.Rows.Count > 0)
            {
                _cboStatus.SelectedValue = 0;
            }
        }

        public void AttachRefreshHandler(EventHandler handler)
        {
            _btnRefresh.Click += handler;
        }

        private void OnSearchGotFocus(object sender, EventArgs e)
        {
            if (_txtSearch.Text == SearchPlaceholder)
            {
                _txtSearch.Text = string.Empty;
                _txtSearch.ForeColor = Color.Black;
            }
        }

        private void OnSearchLostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtSearch.Text))
            {
                _txtSearch.Text = SearchPlaceholder;
                _txtSearch.ForeColor = Color.Gray;
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OnFilterChanged();
        }

        private void OnFilterChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
