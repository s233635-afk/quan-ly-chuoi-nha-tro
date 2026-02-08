using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;
using quan_ly_chuoi_nha_tro.GUI.Shared.Components;

namespace quan_ly_chuoi_nha_tro.GUI
{
    public class FrmBranchBankSettingsEditor : Form
    {
        private readonly AdminDataBLL _bll;
        private ComboBox _cboBranch;
        private TextBox _txtBankId;
        private TextBox _txtAccountNumber;
        private TextBox _txtAccountName;
        private TextBox _txtTemplate;
        private Button _btnSave;
        private Button _btnClose;
        private bool _loading;

        public FrmBranchBankSettingsEditor(AdminDataBLL bll)
        {
            _bll = bll ?? new AdminDataBLL();
            InitializeComponent();
            Load += async (s, e) => await LoadBranchesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Cấu hình QR theo chi nhánh";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 340);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            int left = 16;
            int labelWidth = 160;
            int inputWidth = 300;
            int top = 20;
            int line = 36;

            Controls.Add(MakeLabel("Chi nhánh (*)", left, top, labelWidth));
            _cboBranch = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(left + labelWidth + 8, top),
                Width = inputWidth
            };
            _cboBranch.SelectedIndexChanged += async (s, e) => await LoadBranchSettingsAsync();
            Controls.Add(_cboBranch);
            top += line;

            Controls.Add(MakeLabel("Ngân hàng (BankId) (*)", left, top, labelWidth));
            _txtBankId = MakeInput("", left + labelWidth + 8, top, inputWidth);
            Controls.Add(_txtBankId);
            top += line;

            Controls.Add(MakeLabel("Số tài khoản (*)", left, top, labelWidth));
            _txtAccountNumber = MakeInput("", left + labelWidth + 8, top, inputWidth);
            Controls.Add(_txtAccountNumber);
            top += line;

            Controls.Add(MakeLabel("Chủ tài khoản (*)", left, top, labelWidth));
            _txtAccountName = MakeInput("", left + labelWidth + 8, top, inputWidth);
            Controls.Add(_txtAccountName);
            top += line;

            Controls.Add(MakeLabel("Template QR", left, top, labelWidth));
            _txtTemplate = MakeInput("compact", left + labelWidth + 8, top, inputWidth);
            Controls.Add(_txtTemplate);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 246, 248)
            };

            _btnSave = new ModernButton
            {
                Text = "Lưu",
                Width = 100,
                Height = 32,
                BaseColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            _btnSave.Click += async (s, e) => await SaveAsync();

            _btnClose = new ModernButton
            {
                Text = "Đóng",
                Width = 100,
                Height = 32,
                BaseColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black
            };
            _btnClose.Click += (s, e) => Close();

            pnlBottom.Controls.Add(_btnSave);
            pnlBottom.Controls.Add(_btnClose);
            _btnSave.Location = new Point(pnlBottom.Width - 220, 12);
            _btnClose.Location = new Point(pnlBottom.Width - 110, 12);
            pnlBottom.Resize += (s, e) =>
            {
                _btnSave.Location = new Point(pnlBottom.Width - 220, 12);
                _btnClose.Location = new Point(pnlBottom.Width - 110, 12);
            };

            Controls.Add(pnlBottom);
            AcceptButton = _btnSave;
            CancelButton = _btnClose;
        }

        private async Task LoadBranchesAsync()
        {
            try
            {
                _loading = true;
                var branches = await _bll.GetBranchesAsync();
                if (branches == null)
                {
                    _cboBranch.DataSource = null;
                    return;
                }

                var view = branches.AsEnumerable()
                    .Where(r => r.Table.Columns.Contains("BranchId"))
                    .Select(r => new
                    {
                        BranchId = int.TryParse(r["BranchId"]?.ToString(), out var id) ? id : 0,
                        BranchName = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : null
                    })
                    .Where(r => r.BranchId > 0)
                    .ToList();

                _cboBranch.DataSource = view;
                _cboBranch.DisplayMember = "BranchName";
                _cboBranch.ValueMember = "BranchId";
                if (_cboBranch.Items.Count > 0)
                    _cboBranch.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải chi nhánh: " + ex.Message);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task LoadBranchSettingsAsync()
        {
            if (_loading) return;
            if (!(_cboBranch.SelectedValue is int branchId) || branchId <= 0) return;

            try
            {
                var current = await _bll.GetBranchBankSettingsAsync(branchId);
                if (current.HasValue)
                {
                    _txtBankId.Text = current.Value.BankId ?? "";
                    _txtAccountNumber.Text = current.Value.AccountNumber ?? "";
                    _txtAccountName.Text = current.Value.AccountName ?? "";
                    _txtTemplate.Text = string.IsNullOrWhiteSpace(current.Value.Template) ? "compact" : current.Value.Template;
                }
                else
                {
                    _txtBankId.Text = "";
                    _txtAccountNumber.Text = "";
                    _txtAccountName.Text = "";
                    _txtTemplate.Text = "compact";
                }
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi tải cấu hình QR: " + ex.Message);
            }
        }

        private async Task SaveAsync()
        {
            if (!(_cboBranch.SelectedValue is int branchId) || branchId <= 0)
            {
                ModernDialog.Error("Vui lòng chọn chi nhánh.");
                return;
            }

            string bankId = (_txtBankId.Text ?? string.Empty).Trim();
            string accountNo = (_txtAccountNumber.Text ?? string.Empty).Trim();
            string accountName = (_txtAccountName.Text ?? string.Empty).Trim();
            string template = string.IsNullOrWhiteSpace(_txtTemplate.Text) ? "compact" : _txtTemplate.Text.Trim();

            if (string.IsNullOrWhiteSpace(bankId) || string.IsNullOrWhiteSpace(accountNo) || string.IsNullOrWhiteSpace(accountName))
            {
                ModernDialog.Error("Vui lòng nhập ngân hàng, số tài khoản và chủ tài khoản.");
                return;
            }

            try
            {
                bool ok = await _bll.UpsertBranchBankSettingsAsync(branchId, bankId, accountNo, accountName, template);
                if (!ok)
                {
                    ModernDialog.Error("Không thể lưu cấu hình QR chi nhánh.");
                    return;
                }

                ToastNotification.Success("Đã lưu cấu hình QR chi nhánh.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ModernDialog.Error("Lỗi lưu cấu hình: " + ex.Message);
            }
        }

        private static Label MakeLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 4),
                Width = width,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static TextBox MakeInput(string value, int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Text = value ?? string.Empty
            };
        }
    }
}
