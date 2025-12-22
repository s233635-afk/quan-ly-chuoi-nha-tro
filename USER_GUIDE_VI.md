# 📖 HƯỚNG DẪN SỬ DỤNG - HÓA ĐƠN & THANH TOÁN

## 🎯 TỔNG QUAN

Hệ thống quản lý hóa đơn và thanh toán đã được **gộp thành 1 chức năng duy nhất** với giao diện hiện đại, dễ sử dụng, và đầy đủ tính năng.

---

## 🚀 CÁCH TRUY CẬP

### **Từ Sidebar Menu (Admin & Staff)**

1. **Mở ứng dụng** → Login bằng admin/staff account
2. **Tìm menu** → `💳 Hóa Đơn & Thanh Toán` (ở sidebar bên trái)
3. **Click vào** → Form mới hiện ra với 2 tab

---

## 📊 GIAO DIỆN CHÍNH

### **Form: "Quản Lý Hóa Đơn & Thanh Toán"**

Có 2 tab chính:

```
┌────────────────────────────────────────────────────────┐
│ [📋 Hóa Đơn]  [💳 Thanh Toán]                          │
├────────────────────────────────────────────────────────┤
│                                                        │
│  [Tìm kiếm]  [Lọc]  [Nút hành động]                   │
│                                                        │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Bảng dữ liệu (DataGrid)                        │ │
│  │  - Hóa đơn 1                                    │ │
│  │  - Hóa đơn 2                                    │ │
│  │  - ...                                          │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  Tóm tắt: Tổng tiền | Đã thu | Còn nợ               │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## 📋 TAB 1: "HÓA ĐƠN"

### 🎯 Mục Đích
Xem, tạo, sửa, xóa, và quản lý toàn bộ hóa đơn

### 🔍 TÌM KIẾM & LỌC
- **Ô tìm kiếm**: Gõ để tìm theo:
  - Số hóa đơn (INV-2024-08-004)
  - Tên khách thuê (Hoa Tiếu)
  - Số phòng (A202)

- **Dropdown "Trạng thái"**: Lọc theo:
  - Tất cả
  - Issued (Đã lập)
  - PartialPaid (Thanh toán một phần)
  - Paid (Đã thanh toán)
  - Overdue (Quá hạn)

### 🔘 NÚT HÀNH ĐỘNG

| Nút | Chức năng | Ghi chú |
|-----|----------|---------|
| **➕ Thêm** | Tạo hóa đơn mới | Form nhập chi tiết hóa đơn |
| **✏️ Sửa** | Chỉnh sửa hóa đơn | Phải chọn 1 hóa đơn trước |
| **🗑️ Xóa** | Xóa hóa đơn | Xác nhận nếu có thanh toán |
| **💵 Thu tiền** | **MỚI!** Thu tiền từ hóa đơn | → Mở form thanh toán nâng cao |
| **📅 Tạo tháng** | Tạo hóa đơn cho 1 tháng | Dialog chọn tháng/năm |
| **📊 Xuất CSV** | Export dữ liệu | Tất cả hóa đơn hiện tại |
| **🔄 Tải lại** | Làm mới dữ liệu | Refresh từ database |

### 📊 BỘ PHẬN TÓMLỲ

```
Tổng: 11                              ← Số lượng hóa đơn
Tổng tiền: 69,290,000 | Đã thu: 77,686,526 | Còn nợ: -8,396,526
```

**Giải thích:**
- **Tổng tiền**: Tổng tiền tất cả hóa đơn
- **Đã thu**: Tổng tiền đã thanh toán
- **Còn nợ**: Số tiền còn lại (Tổng - Đã thu)

---

## 💳 TAB 2: "THANH TOÁN"

### 🎯 Mục Đích
Xem lịch sử thanh toán, quản lý, và export dữ liệu thanh toán

### 🔍 TÌM KIẾM & LỌC

1. **Ô tìm kiếm**: Gõ để tìm theo:
   - Số hóa đơn
   - Tên khách
   - Số phòng

2. **Dropdown "Hình thức"**: Lọc theo phương thức:
   - Tất cả
   - Cash (Tiền mặt)
   - Transfer (Chuyển khoản)
   - Check (Séc)
   - Card (Thẻ)

3. **Ngày "Từ - Đến"**: Chọn khoảng thời gian
   - Default: Tháng trước đến hôm nay
   - Có thể thay đổi bằng DatePicker

### 🔘 NÚT HÀNH ĐỘNG

| Nút | Chức năng |
|-----|----------|
| **➕ Thêm** | Hướng dẫn chọn hóa đơn rồi thu tiền |
| **✏️ Sửa** | (Sẽ được cập nhật sớm) |
| **🗑️ Xóa** | Xóa ghi nhận thanh toán |
| **📊 Xuất CSV** | Export lịch sử thanh toán |
| **🔄 Tải lại** | Làm mới dữ liệu |

### 📊 BỘ PHẬN TÓMLỲ

```
Tổng: 15
Tổng thu: 77,686,526
```

---

## 💰 THU TIỀN - FORM THANH TOÁN NÂNG CAO

### 🎯 Khi Nào Sử Dụng?
Khi bạn muốn thu tiền từ khách thuê:

1. **Cách 1**: Tab "Hóa đơn" → Chọn hóa đơn → Click **"💵 Thu tiền"**
2. **Cách 2**: Menu Sidebar → **"💵 Thu tiền"** → Select hóa đơn

### 📋 NỘI DUNG FORM

Form chia thành 3 section:

#### **Section 1: 👤 THÔNG TIN KHÁCH THUÊ**

Hiển thị đầy đủ thông tin:
- **ID Khách**: 8
- **Tên**: Hoa Tiếu
- **Địa chỉ**: [Địa chỉ khách thuê]
- **CMND**: [Số CMND]
- **Điện thoại**: [Số ĐT]
- **Email**: [Email]
- **Phòng**: A202

**✏️ NÚT "Chỉnh sửa thông tin khách"**
- Click để sửa thông tin khách ngay trên form
- Mở form FrmTenantEditor
- Thay đổi được: Tên, ĐC, ĐT, Email, v.v.

#### **Section 2: 📋 THÔNG TIN HÓA ĐƠN**

Hiển thị:
```
Hóa đơn: INV-2024-08-004 | Ngày lập: 08/08/2024 | Hạn: 31/08/2024

┌─────────────────────────────────────┐
│ Tiền phòng       : 3,500,000        │
│ Tiền dịch vụ     : 850,000          │
│ Chi phí khác     : 0                │
│ Tổng tiền        : 4,350,000        │
│ Đã thanh toán    : 0                │
│ Còn nợ           : 4,350,000        │
└─────────────────────────────────────┘
```

#### **Section 3: 💳 THÔNG TIN THANH TOÁN**

Nhập thông tin thanh toán:

| Trường | Mô tả | Ghi chú |
|--------|-------|--------|
| **Ngày thanh toán** | DatePicker | Default: Hôm nay |
| **Số tiền** | NumericUpDown | Auto-fill: Còn nợ |
| **[Thu đủ]** | Button | Tự động điền số tiền còn nợ |
| **Hình thức** | Dropdown | Cash, Transfer, Check, Card |
| **Tham chiếu** | TextBox | Mã chuyển khoản, séc, etc. |
| **Ghi chú** | TextBox (Multiline) | Ghi chú thêm |

### 📤 XUẤT HÓA ĐƠN

Sau khi nhập đủ thông tin:

1. Click nút **"💾 Lưu"**
2. Dialog hỏi: **"Bạn có muốn xuất hóa đơn không?"**
3. **Yes** → Chọn vị trí lưu file → File .txt được tạo
4. **No** → Đóng form, thanh toán đã được lưu

**File hóa đơn (.txt) chứa:**
```
╔════════════════════════════════════════╗
║     HÓA ĐƠN THANH TOÁN                ║
╚════════════════════════════════════════╝

━━━ THÔNG TIN HÓA ĐƠN ━━━
Số hóa đơn  : INV-2024-08-004
Ngày lập   : 15/08/2024
Hạn thanh  : 31/08/2024

━━━ THÔNG TIN KHÁCH THUÊ ━━━
Tên khách  : Hoa Tiếu
CMND       : 123456789
Địa chỉ    : ...
Điện thoại : ...
Email      : ...

━━━ CHI TIẾT TIỀN ━━━
Tiền phòng      : 3,500,000 đ
Tiền dịch vụ    : 850,000 đ
Chi phí khác    : 0 đ
─────────────────────────
Tổng tiền       : 4,350,000 đ
Đã thanh toán   : 4,350,000 đ
Còn nợ          : 0 đ

Ngày xuất: 21/12/2025 14:30:45

─────────────────────────────────────
Cảm ơn quý khách đã thanh toán.
─────────────────────────────────────
```

---

## 🎯 WORKFLOW ĐIỂN HÌNH

### **Scenario 1: Tạo hóa đơn mới**
```
1. Sidebar → 💳 Hóa Đơn & Thanh Toán
2. Tab "📋 Hóa Đơn" → Click "➕ Thêm"
3. Form FrmInvoiceEditor hiện ra
4. Nhập: Khách thuê, Phòng, Tiền phòng, Tiền dịch vụ, ...
5. Click "Lưu"
6. Hóa đơn xuất hiện trong danh sách
```

### **Scenario 2: Thu tiền từ khách**
```
1. Tab "📋 Hóa Đơn" → Tìm/chọn hóa đơn
2. Click "💵 Thu tiền"
3. Form thanh toán nâng cao mở ra
4. Xem thông tin khách & hóa đơn
5. Nhập: Ngày TT, Số tiền, Hình thức, Tham chiếu, Ghi chú
6. Click "💾 Lưu"
7. Chọn "Yes" để xuất hóa đơn
8. Chọn vị trí lưu file
9. File hóa đơn được tạo
```

### **Scenario 3: Xem lịch sử thanh toán**
```
1. Tab "💳 Thanh Toán" → Tất cả lịch sử hiện ra
2. Tìm kiếm: Gõ tên khách/hóa đơn
3. Lọc: Chọn hình thức (Cash, Transfer, etc.)
4. Chọn khoảng thời gian
5. Click "📊 Xuất CSV" để export dữ liệu
```

### **Scenario 4: Sửa thông tin khách khi thanh toán**
```
1. Click "💵 Thu tiền"
2. Form mở → Section 1: Thông tin khách
3. Click "✏️ Chỉnh sửa thông tin khách"
4. Form edit khách thuê mở
5. Sửa: Tên, ĐC, ĐT, Email, etc.
6. Click "Lưu" trong form edit
7. Trở lại form thanh toán, thông tin đã cập nhật
8. Tiếp tục nhập thanh toán & click "Lưu"
```

---

## 🔍 TÍNH NĂNG NỔBẬT

### 📊 Tìm Kiếm Thông Minh
- **Real-time**: Kết quả cập nhật khi bạn gõ
- **Multi-field**: Tìm theo nhiều trường
- **Placeholder**: Hướng dẫn gõ gì

### 🎨 Lọc Dữ Liệu
- **Combo Box**: Lọc nhanh theo danh mục
- **Date Picker**: Chọn khoảng thời gian
- **Multiple criteria**: Kết hợp nhiều bộ lọc

### 💾 Export Dữ Liệu
- **CSV format**: Mở được bằng Excel
- **Tất cả dữ liệu**: Export đầy đủ thông tin
- **Timestamp**: Tên file có ngày giờ tự động

### 💰 Tính Toán Tự Động
- **Tóm tắt tiền**: Tính tổng, đã thu, còn nợ
- **Auto-fill**: Số tiền còn nợ tự động điền
- **Currency format**: Định dạng tiền tệ

### 🎫 Xuất Hóa Đơn
- **Format đẹp**: Hiển thị chuyên nghiệp
- **Đầy đủ thông tin**: Khách, hóa đơn, tiền
- **Có thể in**: Mở file rồi in ra giấy
- **Save to file**: Lưu cho tài liệu

---

## ⚙️ CẤU HÌNH & SETTINGS

### **Chi Nhánh (Branch Scope)**
- Chỉ xem dữ liệu của chi nhánh được phép
- Admin: Thấy tất cả chi nhánh
- Staff: Thấy chi nhánh được assign

### **Quyền Truy Cập**
- **Admin**: Toàn bộ chức năng
- **Staff**: Thanh toán + xuất dữ liệu

### **Định Dạng Tiền Tệ**
- **Default**: VND (Việt Nam Đồng)
- **Ký hiệu**: N0 (ví dụ: 4,350,000)

---

## ⚠️ LƯU Ý & LƯỚI

### ✅ NÊN LÀM
- ✅ Nhập đầy đủ thông tin thanh toán
- ✅ Chọn đúng hình thức thanh toán
- ✅ Lưu hóa đơn sau khi xuất
- ✅ Kiểm tra lại thông tin trước khi xóa
- ✅ Export CSV để backup định kỳ

### ❌ KHÔNG NÊN LÀM
- ❌ Xóa hóa đơn đã có thanh toán (yêu cầu xác nhận)
- ❌ Nhập số tiền âm
- ❌ Để ô tìm kiếm rỗng lúc không dùng

---

## 🆘 TROUBLESHOOTING

### **Vấn đề**: Không thấy hóa đơn trong danh sách
**Giải pháp**:
1. Click "🔄 Tải lại"
2. Kiểm tra bộ lọc (Status)
3. Xóa text tìm kiếm
4. Kiểm tra quyền chi nhánh

### **Vấn đề**: Nút "Thu tiền" bị disable
**Giải pháp**:
1. Chọn 1 hóa đơn trong danh sách
2. Kiểm tra "Còn nợ" > 0
3. Hóa đơn phải tồn tại trong DB

### **Vấn đề**: File hóa đơn không được tạo
**Giải pháp**:
1. Kiểm tra quyền thư mục lưu
2. Đảm bảo đủ dung lượng ổ cứng
3. Thử chọn vị trí khác

### **Vấn đề**: Data không cập nhật sau lưu
**Giải pháp**:
1. Click "🔄 Tải lại"
2. Kiểm tra connection DB
3. Khóa form cũ nếu có
4. Reload dữ liệu manual

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra log file (nếu có)
2. Xem Error message chi tiết
3. Screenshot lỗi
4. Liên hệ admin/developer

---

## 📚 LIÊN QUAN

- **Quản lý khách thuê**: FrmTenantManager
- **Quản lý hợp đồng**: FrmContractManager
- **Quản lý tiện ích**: FrmUtilityManager
- **Báo cáo doanh thu**: FrmReportManager

---

**🎉 Chúc bạn sử dụng hệ thống thành công!**

*Cập nhật: 21/12/2025*
