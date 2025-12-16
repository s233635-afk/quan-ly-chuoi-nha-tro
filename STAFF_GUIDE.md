# HƯỚNG DẪN SỬ DỤNG HỆ THỐNG QUẢN LÝ NHÂN VIÊN

## 📋 TỔNG QUAN

Hệ thống quản lý nhân viên cung cấp đầy đủ công cụ để nhân viên có thể quản lý các hoạt động hàng ngày của chi nhánh nhà trọ:

- 🚪 Quản Lý Phòng
- 👥 Quản Lý Khách Thuê
- 📜 Quản Lý Hợp Đồng
- 💰 Quản Lý Tiền Cọc
- 🧾 Quản Lý Hóa Đơn
- 💳 Quản Lý Thanh Toán
- 💡 Quản Lý Tiện Ích (Điện/Nước)
- 🔧 Quản Lý Bảo Trì
- 📦 Quản Lý Tài Sản
- 📈 Báo Cáo & Thống Kê

---

## 🎨 GIAO DIỆN

### Sidebar (Thanh Bên Trái)

- **Màu sắc**: Xanh đậm (RGB 35, 47, 62)
- **Các nút chức năng**: 11 nút menu với icon
- **Nút Đăng Xuất**: Nằm ở phía dưới

### Header (Thanh Trên Cùng)

- **Tiêu đề trang**: Hiển thị chức năng hiện tại
- **Thông tin người dùng**: Tên nhân viên + Chi nhánh

### Main Content Area

- **Toolbar**: Các nút tìm kiếm, thêm, sửa, xóa
- **DataGridView**: Hiển thị dữ liệu dạng bảng
- **Responsive**: Tự động điều chỉnh kích thước

---

## ✨ CÁC MODULE CHÍNH

### 1. 🚪 QUẢN LÝ PHÒNG

#### Chức năng:

- ✅ Xem danh sách phòng (lọc theo chi nhánh)
- ✅ Tìm kiếm theo số phòng, loại phòng
- ✅ Lọc theo trạng thái (Trống, Đang ở, Bảo trì)
- ✅ Đổi trạng thái phòng
- ✅ Xem chi tiết phòng (giá, diện tích, tầng)
- ✅ Thống kê: Tổng phòng, Phòng đang ở

#### Trạng thái Phòng:

- 🟢 Trống - Phòng trống
- 🔴 Đang ở - Phòng có khách thuê
- 🟡 Đã cọc - Khách đã đặt cọc
- 🔵 Bảo trì - Đang sửa chữa
- 🟣 Vệ sinh - Đang dọn vệ sinh

---

### 2. 👥 QUẢN LÝ KHÁCH THUÊ

#### Chức năng:

- ✅ Xem danh sách khách thuê
- ✅ Tìm kiếm theo tên, CCCD, SĐT
- ✅ Thêm khách thuê mới
- ✅ Sửa thông tin khách thuê
- ✅ Xóa khách thuê
- ✅ Quản lý người ở chung
- ✅ Xem lịch sử ở nhà của khách

#### Thông tin Khách Thuê:

- Họ tên
- CCCD/Hộ chiếu
- Số điện thoại
- Email
- Ngày sinh
- Địa chỉ
- Hộ khẩu tạm trú

---

### 3. 📜 QUẢN LÝ HỢP ĐỒNG

#### Chức năng:

- ✅ Xem danh sách hợp đồng
- ✅ Tìm kiếm theo số HĐ, tên khách
- ✅ Xem chi tiết hợp đồng
- ✅ Trạng thái hợp đồng: Active, Extended, Terminated, Expired

#### Trạng thái Hợp Đồng:

- 🟢 Active - Đang có hiệu lực
- 🟡 Extended - Gia hạn
- 🔴 Terminated - Đã chấm dứt
- ⚫ Expired - Hết hạn

---

### 4. 💰 QUẢN LÝ TIỀN CỌC

#### Chức năng:

- ✅ Xem danh sách tiền cọc
- ✅ Theo dõi tình trạng cọc
- ✅ Quản lý hoàn cọc

#### Trạng thái Cọc:

- 🟡 Pending - Chờ xác nhận
- 🟢 Confirmed - Đã xác nhận
- 🔄 Returned - Đã hoàn
- ❌ Cancelled - Hủy

---

### 5. 🧾 QUẢN LÝ HÓA ĐƠN

#### Chức năng:

- ✅ Xem danh sách hóa đơn
- ✅ Chi tiết: Tiền phòng, Điện/Nước, Khác
- ✅ Theo dõi nợ
- ✅ Trạng thái thanh toán

#### Trạng thái Hóa Đơn:

- 📄 Draft - Bản nháp
- 📤 Issued - Đã phát hành
- ✅ Paid - Đã thanh toán
- 🟡 PartialPaid - Thanh toán từng phần
- ⚠️ Overdue - Quá hạn

---

### 6. 💳 QUẢN LÝ THANH TOÁN

#### Chức năng:

- ✅ Ghi nhận thanh toán
- ✅ Theo dõi hình thức: Tiền mặt, Chuyển khoản, QR, Séc
- ✅ Quản lý giao dịch

---

### 7. 💡 QUẢN LÝ TIỆN ÍCH

#### Chức năng:

- ✅ Nhập chỉ số điện, nước
- ✅ Tính toán chi phí
- ✅ Quản lý giá dịch vụ

#### Loại Tiện Ích:

- ⚡ Điện (kWh)
- 💧 Nước (m³)
- 📡 Internet (tháng)
- 🗑️ Rác thải (tháng)

---

### 8. 🔧 QUẢN LÝ BẢO TRÌ

#### Chức năng:

- ✅ Tạo ticket bảo trì
- ✅ Theo dõi trạng thái
- ✅ Quản lý ưu tiên

#### Trạng thái Ticket:

- 🆕 Created - Vừa tạo
- 🔨 InProgress - Đang sửa
- ✅ Completed - Hoàn thành
- ❌ Cancelled - Hủy

---

### 9. 📦 QUẢN LÝ TÀI SẢN

#### Chức năng:

- ✅ Theo dõi tài sản
- ✅ Quản lý tình trạng
- ✅ Ghi nhân bảo hành

#### Tình Trạng:

- 🟢 Good - Tốt
- 🟡 Fair - Bình thường
- 🔴 Poor - Tệ
- ⚫ Damaged - Hỏng

---

### 10. 📈 BÁO CÁO & THỐNG KÊ

#### Báo Cáo:

- 📊 Tổng quan hệ thống
- 🏘️ Tỷ lệ lấp phòng
- 💹 Doanh thu theo tháng
- 📋 Nợ phí theo khách
- 🔧 Ticket bảo trì

---

## 🔐 QUYỀN HẠN

| Module     | View | Add | Edit | Delete |
| ---------- | ---- | --- | ---- | ------ |
| Phòng      | ✅   | ❌  | ⚠️   | ❌     |
| Khách      | ✅   | ✅  | ✅   | ✅     |
| HĐ         | ✅   | ❌  | ⚠️   | ❌     |
| Cọc        | ✅   | ⚠️  | ✅   | ❌     |
| HĐơn       | ✅   | ⚠️  | ⚠️   | ❌     |
| Thanh Toán | ✅   | ✅  | ❌   | ⚠️     |
| Tiện Ích   | ✅   | ✅  | ✅   | ⚠️     |
| Bảo Trì    | ✅   | ✅  | ✅   | ✅     |
| Tài Sản    | ✅   | ⚠️  | ⚠️   | ⚠️     |
| Báo Cáo    | ✅   | ❌  | ❌   | ❌     |

---

## 🚀 HƯỚNG DẪN SỬ DỤNG NHANH

### Bước 1: Đăng Nhập

```
Username: nhanvien1 hoặc nhanvien2
Password: pass123
```

### Bước 2: Chọn Module

Nhấp vào nút trong Sidebar bên trái

### Bước 3: Thực Hiện Thao Tác

- 🔍 **Tìm**: Nhập từ khóa vào ô tìm kiếm
- ➕ **Thêm**: Nhấp nút "Thêm"
- ✎ **Sửa**: Chọn dòng + Nhấp "Sửa"
- 🗑️ **Xóa**: Chọn dòng + Nhấp "Xóa"

---

## 💡 MẸO HỮU DỤNG

1. **Tìm Kiếm Nhanh**: Gõ từ khóa + Enter
2. **Làm Mới**: Nhấp "Làm mới" để cập nhật dữ liệu
3. **Lộ Trình Chuyên Gia**: Sử dụng bộ lọc + Tìm kiếm kết hợp
4. **Xuất Excel**: Right-click DataGrid → Copy (sắp tới)

---

## ⚠️ LƯU Ý QUAN TRỌNG

- Không được xóa hợp đồng đang hoạt động
- Cần hoàn toán hết nợ trước khi cho khách rời đi
- Nhập chỉ số điện nước trước ngày 5 mỗi tháng
- Sao lưu hợp đồng PDF định kỳ

---

## 📞 HỖ TRỢ

Liên hệ quản lý chi nhánh nếu:

- Gặp lỗi không thể xử lý
- Cần phân quyền thêm
- Muốn thêm chức năng mới

---

**Phiên bản**: 1.0  
**Cập nhật**: 2024  
**Phát triển bởi**: Quản Lý Chuỗi Nhà Trọ Team
