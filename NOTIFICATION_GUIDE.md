# 📬 THÔNG BÁO & NHẮC NHỞ - HƯỚNG DẪN SỬ DỤNG

**Ngày cập nhật**: 20/12/2025  
**Phiên bản**: 3.0 - Giao Diện Đẹp + Hệ Thống Nhắc Nhở Tự Động Hoàn Chỉnh  
**Trạng thái**: ✅ HOÀN THÀNH & SẴN DÙNG

---

## 🎯 TỔNG QUAN TÍNH NĂNG

### ✨ 4 TAB CHÍNH

#### **Tab 1: 📬 Quản Lý Thông Báo**
- Thêm thông báo mới
- Sửa thông báo hiện có
- Xóa thông báo
- Gửi thông báo cho nhân viên
- Tìm kiếm theo tiêu đề
- Lọc theo trạng thái (Tất cả, Chưa đọc, Đã đọc, Đã gửi)
- Thống kê số lượng

**Giao diện**:
```
┌─ Toolbar (Cao 70px) ──────────────────────────┐
│ [➕Thêm] [✏️Sửa] [🗑️Xóa] [📤Gửi] [🔄Tải]    │
│                      🔍 Tìm: [       ] [Tất cả] │
├──────────────────────────────────────────────────┤
│ DataGridView (Danh sách thông báo)              │
│ - NotificationId, Title, Status, CreatedDate    │
│ - Message, UserId                               │
└──────────────────────────────────────────────────┘
```

---

#### **Tab 2: 🔔 Nhắc Nhở Tự Động**
Hệ thống **tự động phát hiện và nhắc nhở** 3 loại sự kiện quan trọng:

##### **1️⃣ Công Nợ Quá Hạn (💰)**
- **Điều kiện**: Hóa đơn có DueDate < hôm nay và RemainingAmount > 0
- **Tạo thông báo**: "⚠️ NHẮC CÔNG NỢ QUÁ HẠN"
- **Nội dung**: "Công nợ HĐ {number} (quá {days} ngày, nợ {amount}) đã quá hạn thanh toán!"

**Ví dụ**:
```
💰 HĐ INV-2025-001 (quá 5 ngày, nợ 2,500,000 đ)
```

---

##### **2️⃣ Hợp Đồng Hết Hạn (📜)**
- **Điều kiện**: EndDate >= hôm nay && EndDate <= hôm nay + 30 ngày && Status != "Hủy bỏ"
- **Tạo thông báo**: "⚠️ NHẮC HẬN HỢP ĐỒNG"
- **Nội dung**: "Hợp đồng {number} (còn {days} ngày) sắp hết hạn!"

**Ví dụ**:
```
📜 HĐ CT-2025-001 (còn 15 ngày)
📜 HĐ CT-2025-002 (còn 3 ngày)
```

---

##### **3️⃣ Chưa Nhập Chỉ Số Điện Nước (💡)**
- **Điều kiện**: Phòng là "Active" nhưng chưa có UtilityReading nào trong tháng hiện tại
- **Tạo thông báo**: "⚠️ NHẮC NHẬP CHỈ SỐ ĐIỆN NƯỚC"
- **Nội dung**: "Phòng {number} chưa cập nhật chỉ số tháng này!"

**Ví dụ**:
```
💡 Phòng 101
💡 Phòng 205
💡 Phòng 312
```

---

**Cách sử dụng**:
1. Bấm **[🎯 Tạo Nhắc Nhở]** → Hệ thống tự động:
   - Quét database tìm công nợ quá hạn
   - Quét database tìm hợp đồng sắp hết hạn
   - Quét database tìm phòng chưa nhập chỉ số
   - Tạo thông báo tự động cho từng sự kiện
   - Hiển thị trong ListBox

2. Bấm **[📋 Xem Chi Tiết]** → Xem danh sách nhắc nhở đã tạo

---

#### **Tab 3: 📢 Thông Báo Nội Bộ**
Gửi thông báo điều hành cho toàn hệ thống

**Form nhập**:
```
┌──────────────────────────────────────────┐
│ 📝 Tiêu Đề Thông Báo (*)                │
│ [                                      ]  │
│                                          │
│ 💬 Nội Dung                             │
│ [                                      ]  │
│ [                                      ]  │
│ [                                      ]  │
│                                          │
│ 🏷️ Gửi Tới                              │
│ [Toàn hệ thống ▼]                       │
│ - Tất cả nhân viên                      │
│ - Tất cả quản lý                        │
│ - Toàn hệ thống                         │
│                                          │
│ [📤 Gửi Thông Báo]                      │
└──────────────────────────────────────────┘
```

**Ví dụ thông báo nội bộ**:
- "Lịch họp cuối năm sẽ diễn ra vào thứ 6 lúc 14h"
- "Cập nhật lệ phí dịch vụ từ 01/01/2026"
- "Yêu cầu kiểm tra và báo cáo tình trạng phòng trước 25/12"

---

#### **Tab 4: 📜 Lịch Sử Gửi**
Danh sách tất cả thông báo đã gửi (Status = "Đã gửi")

---

## 🎨 THIẾT KẾ GIAO DIỆN

### **Bảng Màu**
| Loại | Màu | RGB |
|------|-----|-----|
| Chính | Xanh Dương | (0, 120, 215) |
| Thành Công | Xanh Lá | (46, 204, 113) |
| Lỗi | Đỏ | (231, 76, 60) |
| Cảnh Báo | Cam | (230, 126, 34) |
| Bình Thường | Xám | (149, 165, 166) |
| Tím | Tím | (155, 89, 182) |

### **Typography**
- **Font**: Segoe UI
- **Tiêu đề Form**: 14px, Bold
- **Nhãn Label**: 10px, Bold
- **Input Text**: 10px, Regular
- **Grid Header**: 10px, Bold, Trắng trên Xanh

### **Emoji Icons**
- 📬 Quản Lý Thông Báo
- 🔔 Nhắc Nhở Tự Động
- 📢 Thông Báo Nội Bộ
- 📜 Lịch Sử Gửi
- 💰 Công Nợ
- 📜 Hợp Đồng
- 💡 Chỉ Số Điện Nước
- ✅ Lưu / ❌ Hủy
- 📤 Gửi
- 🔄 Tải Lại

---

## 🔧 HƯỚNG DẪN QUẢN TRỊ

### **Thêm Thông Báo Mới**
1. Tab "Quản Lý Thông Báo" → Bấm **[➕ Thêm]**
2. Form "Thêm Thông Báo Mới" mở ra
3. Nhập:
   - **👤 Nhân Viên**: ID nhân viên (0 = Tất cả)
   - **📝 Tiêu Đề**: Tiêu đề thông báo (bắt buộc)
   - **🏷️ Trạng Thái**: Chưa đọc / Đã đọc / Đã gửi
   - **💬 Nội Dung**: Nội dung chi tiết
4. Bấm **[✅ Lưu]**

---

### **Sửa Thông Báo**
1. Tab "Quản Lý Thông Báo"
2. Double-click dòng cần sửa (hoặc chọn + bấm **[✏️ Sửa]**)
3. Form "Sửa Thông Báo" mở ra với dữ liệu hiện tại
4. Chỉnh sửa các trường cần thiết
5. Bấm **[✅ Lưu]**

---

### **Xóa Thông Báo**
1. Tab "Quản Lý Thông Báo"
2. Chọn thông báo cần xóa
3. Bấm **[🗑️ Xóa]**
4. Xác nhận "Bạn chắc chắn muốn xóa?" → **Yes**
5. Thông báo bị xóa khỏi database

---

### **Gửi Thông Báo**
1. Tab "Quản Lý Thông Báo"
2. Chọn thông báo cần gửi
3. Bấm **[📤 Gửi]**
4. Hệ thống cập nhật Status → "Đã gửi"
5. Thông báo sẽ xuất hiện trong Tab "Lịch Sử Gửi"

---

### **Tạo Nhắc Nhở Tự Động**
1. Tab "Nhắc Nhở Tự Động"
2. Xem các thẻ thống kê trên cùng:
   - 💰 Công Nợ Quá Hạn: {số lượng}
   - 📜 Hợp Đồng Hết Hạn: {số lượng}
   - 💡 Chưa Nhập Chỉ Số: {số lượng}
3. Bấm **[🎯 Tạo Nhắc Nhở]**
   - Hệ thống quét database
   - Tạo thông báo tự động
   - Hiển thị danh sách trong ListBox
4. Kiểm tra Tab "Quản Lý Thông Báo" để thấy các thông báo mới

---

### **Gửi Thông Báo Nội Bộ**
1. Tab "Thông Báo Nội Bộ"
2. Nhập:
   - **📝 Tiêu Đề**: Tiêu đề (bắt buộc)
   - **💬 Nội Dung**: Nội dung (tùy chọn)
   - **🏷️ Gửi Tới**: Chọn đối tượng
3. Bấm **[📤 Gửi Thông Báo]**
4. Thông báo được lưu vào database

---

### **Xem Lịch Sử Gửi**
1. Tab "Lịch Sử Gửi"
2. Hiển thị tất cả thông báo với Status = "Đã gửi"
3. Các cột: NotificationId, Title, Status, CreatedDate, UserId, Message

---

## 📊 DỮ LIỆU LIÊN QUAN

### **Bảng Notifications**
```sql
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL,  -- NULL = Tất cả
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX),
    Status NVARCHAR(50),  -- 'Chưa đọc', 'Đã đọc', 'Đã gửi'
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

### **Dữ Liệu Liên Quan**
- **Invoices**: Để tìm công nợ quá hạn
  - DueDate: Ngày hạn thanh toán
  - RemainingAmount: Số tiền còn nợ
  
- **Contracts**: Để tìm hợp đồng sắp hết hạn
  - EndDate: Ngày kết thúc hợp đồng
  - Status: Trạng thái hợp đồng
  
- **Rooms**: Để tìm phòng chưa nhập chỉ số
  - IsActive: Phòng hoạt động
  - RoomId: ID phòng
  
- **UtilityReadings**: Để kiểm tra chỉ số điện nước
  - ReadingDate: Ngày nhập chỉ số
  - RoomId: Phòng
  - UtilityTypeId: Loại tiện ích

---

## 🐛 XỬ LÝ LỖI

### **Lỗi: Không Load được dữ liệu**
- ✅ Kiểm tra kết nối database
- ✅ Kiểm tra App.config connectionString
- ✅ Bấm [🔄 Tải] để tải lại

### **Lỗi: Thêm thông báo thất bại**
- ✅ Kiểm tra tiêu đề không được để trống
- ✅ Kiểm tra kết nối database
- ✅ Xem message lỗi chi tiết

### **Lỗi: Không tạo được nhắc nhở**
- ✅ Kiểm tra database có dữ liệu (Invoices, Contracts, Rooms, UtilityReadings)
- ✅ Kiểm tra kết nối database
- ✅ Xem message lỗi chi tiết

---

## 📝 NGÔN NGỮ

✅ **100% Tiếng Việt**
- Tất cả label, button, message, thông báo đều bằng tiếng Việt
- Emoji icons để dễ nhận diện chức năng

---

## 🚀 CÁC FILE ĐƯỢC CẬP NHẬT

### **Files Thay Đổi**
1. **FrmNotificationManager.cs** (850 dòng)
   - ✅ 4 Tab hoàn chỉnh
   - ✅ Giao diện đẹp với emoji
   - ✅ Nhắc nhở tự động
   - ✅ Thông báo nội bộ
   - ✅ Lịch sử gửi

2. **FrmNotificationEditor.cs** (200 dòng)
   - ✅ Form thêm/sửa đẹp
   - ✅ Validation đầu vào
   - ✅ Message feedback rõ ràng

### **Files Không Thay Đổi (Nhưng Sẵn Dùng)**
- AdminDataBLL.cs - Đã có các method cần thiết
- DatabaseHelper.cs - Đã có các method query
- App.config - Đã cấu hình connectionString

---

## ✅ CHECKLIST HOÀN THÀNH

- ✅ Quản lý thông báo (Thêm, Sửa, Xóa, Gửi)
- ✅ Tìm kiếm & Lọc thông báo
- ✅ Nhắc công nợ quá hạn (tự động)
- ✅ Nhắc hợp đồng hết hạn (tự động)
- ✅ Nhắc nhập chỉ số điện nước (tự động)
- ✅ Thông báo nội bộ
- ✅ Lịch sử gửi
- ✅ Giao diện đẹp & chuyên nghiệp
- ✅ Toàn bộ tiếng Việt
- ✅ Không lỗi syntax
- ✅ Emoji icons

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra lỗi trong message box
2. Kiểm tra database connection
3. Xem các file implementation

---

**🎉 Hệ thống Thông Báo & Nhắc Nhở hoàn thành!**
