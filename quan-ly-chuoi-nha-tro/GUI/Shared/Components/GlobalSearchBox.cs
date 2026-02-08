using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Global search box with dropdown results
    /// </summary>
    public class GlobalSearchBox : Panel
    {
        public event EventHandler<SearchResultEventArgs> ResultSelected;

        private readonly TextBox _txtSearch;
        private readonly ListBox _lstResults;
        private readonly Panel _dropdownPanel;
        private readonly Debouncer _debouncer;
        private Func<string, List<SearchResult>> _searchFunction;

        public GlobalSearchBox()
        {
            Height = 36;
            Width = 300;
            BackColor = Color.White;

            // Search textbox
            _txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Text = "🔍 Tìm kiếm..."
            };

            _txtSearch.GotFocus += OnSearchFocus;
            _txtSearch.LostFocus += OnSearchBlur;
            _txtSearch.TextChanged += OnSearchTextChanged;
            _txtSearch.KeyDown += OnSearchKeyDown;

            // Container panel with border
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 8),
                BackColor = Color.White
            };
            container.Controls.Add(_txtSearch);

            // Border
            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            };

            Controls.Add(container);

            // Dropdown results
            _lstResults = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                ItemHeight = 36,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            _lstResults.DrawItem += OnDrawResultItem;
            _lstResults.Click += OnResultClick;
            _lstResults.KeyDown += OnResultKeyDown;

            _dropdownPanel = new Panel
            {
                Visible = false,
                BackColor = Color.White,
                Size = new Size(Width, 200),
                BorderStyle = BorderStyle.FixedSingle
            };
            _dropdownPanel.Controls.Add(_lstResults);

            _debouncer = new Debouncer(300);
        }

        /// <summary>
        /// Set the search function
        /// </summary>
        public void SetSearchFunction(Func<string, List<SearchResult>> searchFunc)
        {
            _searchFunction = searchFunc;
        }

        /// <summary>
        /// Show the dropdown below the search box
        /// </summary>
        public void ShowDropdown(Form parentForm)
        {
            if (!_dropdownPanel.Visible && parentForm != null)
            {
                var location = parentForm.PointToClient(PointToScreen(new Point(0, Height)));
                _dropdownPanel.Location = location;
                _dropdownPanel.Width = Width;
                _dropdownPanel.BringToFront();

                if (!parentForm.Controls.Contains(_dropdownPanel))
                    parentForm.Controls.Add(_dropdownPanel);

                _dropdownPanel.Visible = true;
            }
        }

        public void HideDropdown()
        {
            _dropdownPanel.Visible = false;
        }

        private void OnSearchFocus(object sender, EventArgs e)
        {
            if (_txtSearch.Text == "🔍 Tìm kiếm...")
            {
                _txtSearch.Text = "";
                _txtSearch.ForeColor = Color.Black;
            }
        }

        private void OnSearchBlur(object sender, EventArgs e)
        {
            // Delay to allow click on results
            var timer = new Timer { Interval = 200 };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                timer.Dispose();

                if (!_lstResults.Focused)
                {
                    HideDropdown();
                    if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                    {
                        _txtSearch.Text = "🔍 Tìm kiếm...";
                        _txtSearch.ForeColor = Color.Gray;
                    }
                }
            };
            timer.Start();
        }

        private async void OnSearchTextChanged(object sender, EventArgs e)
        {
            var query = _txtSearch.Text.Trim();
            if (query.Length < 2 || query == "🔍 Tìm kiếm...")
            {
                HideDropdown();
                return;
            }

            await _debouncer.DebounceAsync(() =>
            {
                if (_searchFunction != null)
                {
                    var results = _searchFunction(query);
                    UpdateResults(results);
                }
            });
        }

        private void UpdateResults(List<SearchResult> results)
        {
            _lstResults.Items.Clear();

            if (results == null || results.Count == 0)
            {
                _lstResults.Items.Add(new SearchResult { Title = "Không tìm thấy kết quả", Category = "" });
            }
            else
            {
                foreach (var result in results)
                {
                    _lstResults.Items.Add(result);
                }
            }

            var parentForm = FindForm();
            ShowDropdown(parentForm);
        }

        private void OnDrawResultItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var result = _lstResults.Items[e.Index] as SearchResult;
            if (result == null) return;

            var g = e.Graphics;
            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var bgColor = isSelected ? Color.FromArgb(230, 243, 255) : Color.White;
            var textColor = isSelected ? Color.FromArgb(0, 120, 215) : Color.FromArgb(33, 33, 33);

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, e.Bounds);
            }

            // Category icon
            using (var font = new Font("Segoe UI", 10))
            using (var brush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                g.DrawString(result.Icon, font, brush, e.Bounds.X + 8, e.Bounds.Y + 10);
            }

            // Title
            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(textColor))
            {
                g.DrawString(result.Title, font, brush, e.Bounds.X + 32, e.Bounds.Y + 4);
            }

            // Category
            using (var font = new Font("Segoe UI", 8))
            using (var brush = new SolidBrush(Color.FromArgb(130, 130, 130)))
            {
                g.DrawString(result.Category, font, brush, e.Bounds.X + 32, e.Bounds.Y + 20);
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _lstResults.Items.Count > 0)
            {
                _lstResults.Focus();
                _lstResults.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideDropdown();
                e.Handled = true;
            }
        }

        private void OnResultKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _lstResults.SelectedItem is SearchResult result)
            {
                SelectResult(result);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideDropdown();
                _txtSearch.Focus();
                e.Handled = true;
            }
        }

        private void OnResultClick(object sender, EventArgs e)
        {
            if (_lstResults.SelectedItem is SearchResult result)
            {
                SelectResult(result);
            }
        }

        private void SelectResult(SearchResult result)
        {
            if (result.Id == 0) return; // No result placeholder

            HideDropdown();
            _txtSearch.Text = "";
            _txtSearch.Text = "🔍 Tìm kiếm...";
            _txtSearch.ForeColor = Color.Gray;

            ResultSelected?.Invoke(this, new SearchResultEventArgs(result));
        }
    }

    /// <summary>
    /// Search result item
    /// </summary>
    public class SearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Icon { get; set; } = "🔹";
        public object Data { get; set; }
    }

    /// <summary>
    /// Event args for search result selection
    /// </summary>
    public class SearchResultEventArgs : EventArgs
    {
        public SearchResult Result { get; }
        public SearchResultEventArgs(SearchResult result) { Result = result; }
    }
}
