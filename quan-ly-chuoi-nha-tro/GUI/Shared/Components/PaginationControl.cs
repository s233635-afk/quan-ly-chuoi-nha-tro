using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Pagination control for DataGridView
    /// Provides navigation buttons and page info display
    /// </summary>
    public class PaginationControl : Panel
    {
        public event EventHandler PageChanged;

        private readonly Button _btnFirst;
        private readonly Button _btnPrev;
        private readonly Button _btnNext;
        private readonly Button _btnLast;
        private readonly Label _lblPageInfo;
        private readonly ComboBox _cboPageSize;
        private readonly Label _lblPageSize;

        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalRecords = 0;
        private int _pageSize = 50;

        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;
        public int TotalRecords => _totalRecords;
        public int PageSize => _pageSize;

        public PaginationControl()
        {
            Height = 40;
            Dock = DockStyle.Bottom;
            BackColor = Color.FromArgb(248, 249, 250);
            Padding = new Padding(8, 4, 8, 4);

            // Page size selector
            _lblPageSize = new Label
            {
                Text = "Hiển thị:",
                AutoSize = true,
                Location = new Point(8, 12),
                Font = new Font("Segoe UI", 9)
            };

            _cboPageSize = new ComboBox
            {
                Width = 65,
                Location = new Point(60, 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            _cboPageSize.Items.AddRange(new object[] { 20, 50, 100, 200 });
            _cboPageSize.SelectedItem = 50;
            _cboPageSize.SelectedIndexChanged += OnPageSizeChanged;

            // Navigation buttons
            _btnFirst = CreateNavButton("⏮", "Trang đầu");
            _btnFirst.Click += (s, e) => GoToPage(1);

            _btnPrev = CreateNavButton("◀", "Trang trước");
            _btnPrev.Click += (s, e) => GoToPage(_currentPage - 1);

            _btnNext = CreateNavButton("▶", "Trang sau");
            _btnNext.Click += (s, e) => GoToPage(_currentPage + 1);

            _btnLast = CreateNavButton("⏭", "Trang cuối");
            _btnLast.Click += (s, e) => GoToPage(_totalPages);

            // Page info label
            _lblPageInfo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(60, 60, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls
            Controls.Add(_lblPageSize);
            Controls.Add(_cboPageSize);
            Controls.Add(_btnFirst);
            Controls.Add(_btnPrev);
            Controls.Add(_lblPageInfo);
            Controls.Add(_btnNext);
            Controls.Add(_btnLast);

            // Position controls on resize
            Resize += OnResize;
            OnResize(this, EventArgs.Empty);
            UpdateDisplay();
        }

        private Button CreateNavButton(string text, string tooltip)
        {
            var btn = new Button
            {
                Text = text,
                Width = 32,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(0, 120, 215),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);

            var tt = new ToolTip();
            tt.SetToolTip(btn, tooltip);

            return btn;
        }

        private void OnResize(object sender, EventArgs e)
        {
            // Center navigation controls
            var centerX = Width / 2;
            var btnWidth = 32;
            var spacing = 4;
            var totalWidth = btnWidth * 4 + spacing * 3 + 150; // 150 for label

            var startX = centerX - totalWidth / 2;

            _btnFirst.Location = new Point(startX, 6);
            _btnPrev.Location = new Point(startX + btnWidth + spacing, 6);
            _lblPageInfo.Location = new Point(startX + (btnWidth + spacing) * 2, 12);
            _btnNext.Location = new Point(startX + (btnWidth + spacing) * 2 + 150, 6);
            _btnLast.Location = new Point(startX + (btnWidth + spacing) * 3 + 150, 6);
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (_cboPageSize.SelectedItem is int size)
            {
                _pageSize = size;
                _currentPage = 1;
                CalculateTotalPages();
                UpdateDisplay();
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Update pagination with new data count
        /// </summary>
        public void UpdateData(int totalRecords)
        {
            _totalRecords = totalRecords;
            CalculateTotalPages();
            
            // Adjust current page if needed
            if (_currentPage > _totalPages)
                _currentPage = _totalPages > 0 ? _totalPages : 1;
            
            UpdateDisplay();
        }

        /// <summary>
        /// Go to specific page
        /// </summary>
        public bool GoToPage(int page)
        {
            if (page < 1 || page > _totalPages) return false;
            if (page == _currentPage) return true;

            _currentPage = page;
            UpdateDisplay();
            PageChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void CalculateTotalPages()
        {
            _totalPages = _pageSize > 0 
                ? (int)Math.Ceiling((double)_totalRecords / _pageSize) 
                : 1;
            if (_totalPages < 1) _totalPages = 1;
        }

        private void UpdateDisplay()
        {
            // Update page info
            if (_totalRecords == 0)
            {
                _lblPageInfo.Text = "Không có dữ liệu";
            }
            else
            {
                int start = (_currentPage - 1) * _pageSize + 1;
                int end = Math.Min(_currentPage * _pageSize, _totalRecords);
                _lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} ({start}-{end}/{_totalRecords})";
            }

            // Update button states
            _btnFirst.Enabled = _currentPage > 1;
            _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled = _currentPage < _totalPages;
            _btnLast.Enabled = _currentPage < _totalPages;

            // Update button colors
            UpdateButtonState(_btnFirst);
            UpdateButtonState(_btnPrev);
            UpdateButtonState(_btnNext);
            UpdateButtonState(_btnLast);
        }

        private void UpdateButtonState(Button btn)
        {
            btn.ForeColor = btn.Enabled 
                ? Color.FromArgb(0, 120, 215) 
                : Color.FromArgb(180, 180, 180);
        }

        /// <summary>
        /// Get skip count for query
        /// </summary>
        public int GetSkip() => (_currentPage - 1) * _pageSize;

        /// <summary>
        /// Get take count for query
        /// </summary>
        public int GetTake() => _pageSize;
    }
}
