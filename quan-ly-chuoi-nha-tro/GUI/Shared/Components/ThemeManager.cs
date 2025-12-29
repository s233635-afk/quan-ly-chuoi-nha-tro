using System;
using System.Drawing;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Theme manager for Light/Dark mode support
    /// </summary>
    public static class ThemeManager
    {
        public enum ThemeMode { Light, Dark }

        private static ThemeMode _currentTheme = ThemeMode.Light;
        public static ThemeMode CurrentTheme => _currentTheme;

        public static event EventHandler ThemeChanged;

        // Light theme colors
        public static class LightTheme
        {
            public static readonly Color Background = Color.White;
            public static readonly Color Surface = Color.FromArgb(248, 249, 250);
            public static readonly Color Primary = Color.FromArgb(0, 120, 215);
            public static readonly Color TextPrimary = Color.FromArgb(33, 33, 33);
            public static readonly Color TextSecondary = Color.FromArgb(117, 117, 117);
            public static readonly Color Border = Color.FromArgb(224, 224, 224);
            public static readonly Color Sidebar = Color.FromArgb(245, 246, 248);
            public static readonly Color SidebarText = Color.FromArgb(55, 65, 81);
            public static readonly Color CardBackground = Color.White;
            public static readonly Color InputBackground = Color.White;
        }

        // Dark theme colors
        public static class DarkTheme
        {
            public static readonly Color Background = Color.FromArgb(18, 18, 18);
            public static readonly Color Surface = Color.FromArgb(30, 30, 30);
            public static readonly Color Primary = Color.FromArgb(100, 181, 246);
            public static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
            public static readonly Color TextSecondary = Color.FromArgb(176, 176, 176);
            public static readonly Color Border = Color.FromArgb(66, 66, 66);
            public static readonly Color Sidebar = Color.FromArgb(24, 24, 24);
            public static readonly Color SidebarText = Color.FromArgb(200, 200, 200);
            public static readonly Color CardBackground = Color.FromArgb(40, 40, 40);
            public static readonly Color InputBackground = Color.FromArgb(45, 45, 45);
        }

        // Current theme colors
        public static Color Background => _currentTheme == ThemeMode.Light ? LightTheme.Background : DarkTheme.Background;
        public static Color Surface => _currentTheme == ThemeMode.Light ? LightTheme.Surface : DarkTheme.Surface;
        public static Color Primary => _currentTheme == ThemeMode.Light ? LightTheme.Primary : DarkTheme.Primary;
        public static Color TextPrimary => _currentTheme == ThemeMode.Light ? LightTheme.TextPrimary : DarkTheme.TextPrimary;
        public static Color TextSecondary => _currentTheme == ThemeMode.Light ? LightTheme.TextSecondary : DarkTheme.TextSecondary;
        public static Color Border => _currentTheme == ThemeMode.Light ? LightTheme.Border : DarkTheme.Border;
        public static Color Sidebar => _currentTheme == ThemeMode.Light ? LightTheme.Sidebar : DarkTheme.Sidebar;
        public static Color SidebarText => _currentTheme == ThemeMode.Light ? LightTheme.SidebarText : DarkTheme.SidebarText;
        public static Color CardBackground => _currentTheme == ThemeMode.Light ? LightTheme.CardBackground : DarkTheme.CardBackground;
        public static Color InputBackground => _currentTheme == ThemeMode.Light ? LightTheme.InputBackground : DarkTheme.InputBackground;

        /// <summary>
        /// Set the current theme
        /// </summary>
        public static void SetTheme(ThemeMode theme)
        {
            if (_currentTheme == theme) return;
            _currentTheme = theme;
            SaveThemePreference();
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Toggle between light and dark theme
        /// </summary>
        public static void ToggleTheme()
        {
            SetTheme(_currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light);
        }

        /// <summary>
        /// Load saved theme preference
        /// </summary>
        public static void LoadThemePreference()
        {
            try
            {
                var settingsPath = GetSettingsPath();
                if (System.IO.File.Exists(settingsPath))
                {
                    var theme = System.IO.File.ReadAllText(settingsPath).Trim();
                    _currentTheme = theme == "Dark" ? ThemeMode.Dark : ThemeMode.Light;
                }
            }
            catch { /* Use default */ }
        }

        /// <summary>
        /// Save theme preference
        /// </summary>
        private static void SaveThemePreference()
        {
            try
            {
                var settingsPath = GetSettingsPath();
                var dir = System.IO.Path.GetDirectoryName(settingsPath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(settingsPath, _currentTheme.ToString());
            }
            catch { /* Ignore */ }
        }

        private static string GetSettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return System.IO.Path.Combine(appData, "QuanLyChuoiNhaTro", "theme.txt");
        }

        /// <summary>
        /// Apply theme to a form
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            if (form == null) return;
            form.BackColor = Background;
            form.ForeColor = TextPrimary;
            ApplyThemeToControls(form.Controls);
        }

        /// <summary>
        /// Apply theme to control collection
        /// </summary>
        public static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyThemeToControl(control);
                if (control.HasChildren)
                    ApplyThemeToControls(control.Controls);
            }
        }

        /// <summary>
        /// Apply theme to single control
        /// </summary>
        public static void ApplyThemeToControl(Control control)
        {
            switch (control)
            {
                case TextBox txt:
                    txt.BackColor = InputBackground;
                    txt.ForeColor = TextPrimary;
                    break;

                case ComboBox cmb:
                    cmb.BackColor = InputBackground;
                    cmb.ForeColor = TextPrimary;
                    break;

                case DataGridView grid:
                    grid.BackgroundColor = Surface;
                    grid.DefaultCellStyle.BackColor = Background;
                    grid.DefaultCellStyle.ForeColor = TextPrimary;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Surface;
                    grid.GridColor = Border;
                    break;

                case Panel panel:
                    // Only change non-colored panels
                    if (panel.BackColor == SystemColors.Control || 
                        panel.BackColor == Color.White ||
                        panel.BackColor == LightTheme.Background ||
                        panel.BackColor == DarkTheme.Background)
                    {
                        panel.BackColor = Surface;
                    }
                    break;

                case Label lbl:
                    if (lbl.ForeColor == SystemColors.ControlText ||
                        lbl.ForeColor == Color.Black ||
                        lbl.ForeColor == LightTheme.TextPrimary ||
                        lbl.ForeColor == DarkTheme.TextPrimary)
                    {
                        lbl.ForeColor = TextPrimary;
                    }
                    break;

                case Button btn:
                    // Only style non-colored buttons
                    if (btn.BackColor == SystemColors.Control)
                    {
                        btn.BackColor = Surface;
                        btn.ForeColor = TextPrimary;
                    }
                    break;
            }
        }

        /// <summary>
        /// Create a theme toggle button
        /// </summary>
        public static Button CreateToggleButton()
        {
            var btn = new Button
            {
                Text = _currentTheme == ThemeMode.Light ? "🌙" : "☀️",
                Size = new Size(36, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (s, e) =>
            {
                ToggleTheme();
                btn.Text = _currentTheme == ThemeMode.Light ? "🌙" : "☀️";
            };

            return btn;
        }
    }
}
