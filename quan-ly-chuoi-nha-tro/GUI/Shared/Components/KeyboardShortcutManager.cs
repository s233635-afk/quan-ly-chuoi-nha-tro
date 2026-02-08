using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Manages keyboard shortcuts for forms
    /// Provides consistent shortcut handling across the application
    /// </summary>
    public class KeyboardShortcutManager
    {
        private readonly Form _form;
        private readonly Dictionary<Keys, Action> _shortcuts = new Dictionary<Keys, Action>();
        private readonly Dictionary<Keys, string> _descriptions = new Dictionary<Keys, string>();

        public KeyboardShortcutManager(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _form.KeyPreview = true;
            _form.KeyDown += OnKeyDown;
        }

        /// <summary>
        /// Register a keyboard shortcut
        /// </summary>
        public KeyboardShortcutManager Register(Keys keys, Action action, string description = null)
        {
            _shortcuts[keys] = action;
            if (!string.IsNullOrEmpty(description))
                _descriptions[keys] = description;
            return this;
        }

        /// <summary>
        /// Register common CRUD shortcuts
        /// </summary>
        public KeyboardShortcutManager RegisterCrudShortcuts(
            Action onNew = null,
            Action onSave = null,
            Action onEdit = null,
            Action onDelete = null,
            Action onRefresh = null,
            Action onSearch = null)
        {
            if (onNew != null)
                Register(Keys.Control | Keys.N, onNew, "Thêm mới");
            if (onSave != null)
                Register(Keys.Control | Keys.S, onSave, "Lưu");
            if (onEdit != null)
                Register(Keys.Control | Keys.E, onEdit, "Chỉnh sửa");
            if (onDelete != null)
                Register(Keys.Delete, onDelete, "Xóa");
            if (onRefresh != null)
                Register(Keys.F5, onRefresh, "Làm mới");
            if (onSearch != null)
                Register(Keys.Control | Keys.F, onSearch, "Tìm kiếm");
            
            return this;
        }

        /// <summary>
        /// Get shortcut description for tooltip
        /// </summary>
        public string GetShortcutText(Keys keys)
        {
            var parts = new List<string>();
            
            if ((keys & Keys.Control) == Keys.Control)
                parts.Add("Ctrl");
            if ((keys & Keys.Shift) == Keys.Shift)
                parts.Add("Shift");
            if ((keys & Keys.Alt) == Keys.Alt)
                parts.Add("Alt");
            
            var keyCode = keys & Keys.KeyCode;
            parts.Add(keyCode.ToString());
            
            return string.Join("+", parts);
        }

        /// <summary>
        /// Get all registered shortcuts with descriptions
        /// </summary>
        public Dictionary<string, string> GetAllShortcuts()
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in _descriptions)
            {
                result[GetShortcutText(kvp.Key)] = kvp.Value;
            }
            return result;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var keys = e.KeyData;
            if (_shortcuts.TryGetValue(keys, out var action))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                action?.Invoke();
            }
        }

        /// <summary>
        /// Show shortcuts help dialog
        /// </summary>
        public void ShowShortcutsHelp()
        {
            var shortcuts = GetAllShortcuts();
            if (shortcuts.Count == 0)
            {
                MessageBox.Show("Không có phím tắt nào được đăng ký.", "Phím tắt", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var lines = new List<string>();
            foreach (var kvp in shortcuts)
            {
                lines.Add($"{kvp.Key,-15} : {kvp.Value}");
            }

            MessageBox.Show(
                string.Join("\n", lines),
                "Danh sách phím tắt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public void Dispose()
        {
            if (_form != null)
            {
                _form.KeyDown -= OnKeyDown;
            }
            _shortcuts.Clear();
            _descriptions.Clear();
        }
    }
}
