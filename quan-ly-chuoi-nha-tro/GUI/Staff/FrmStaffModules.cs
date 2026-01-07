using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyNhaTro.BLL;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Các form quản lý cho Nhân viên - Phiên bản gọn nhẹ
    /// </summary>

    // ========== HÓA ĐƠN ==========
    public class FrmInvoiceManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmInvoiceManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Hóa Đơn";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "📊 Quản Lý Hóa Đơn - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== THANH TOÁN ==========
    public class FrmPaymentManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmPaymentManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Thanh Toán";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "💳 Quản Lý Thanh Toán - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== TIỀN CỌC ==========
    public class FrmDepositManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmDepositManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Tiền Cọc";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "💰 Quản Lý Tiền Cọc - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== TIỆN ÍCH ==========
    public class FrmUtilityManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmUtilityManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Tiện Ích";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "💡 Quản Lý Tiện Ích (Điện/Nước) - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== BẢO TRÌ ==========
    public class FrmMaintenanceManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmMaintenanceManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Bảo Trì";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "🔧 Quản Lý Bảo Trì - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== TÀI SẢN ==========
    public class FrmAssetManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmAssetManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Quản Lý Tài Sản";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "📦 Quản Lý Tài Sản - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }

    // ========== BÁO CÁO ==========
    public class FrmReportManager : Form
    {
        private readonly StaffBLL _bll;
        private readonly int? _branchId;

        public FrmReportManager(StaffBLL bll, int? branchId = null)
        {
            _bll = bll ?? new StaffBLL();
            _branchId = branchId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Báo Cáo & Thống Kê";
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);
            Padding = new Padding(15);

            var lbl = new Label { Text = "📈 Báo Cáo & Thống Kê - Chức năng đang phát triển", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 50 };
            Controls.Add(lbl);
        }
    }
}
