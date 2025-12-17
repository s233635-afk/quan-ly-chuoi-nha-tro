# So Sánh Trước/Sau - Staff Dashboard

## Dashboard Tổng Quan (Overview)

### TRƯỚC:
```
┌─────────────────────────────────────────────┐
│ Công việc hôm nay                          │
│                                             │
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ │🏠 Phòng      │ │🧾 Công nợ    │ │⏰ Quá hạn    │
│ │0/10 dùng    │ │100,000₫      │ │2 hóa đơn     │
│ │Sử dụng/Tổng │ │Tiền nợ       │ │Overdue       │
│ └──────────────┘ └──────────────┘ └──────────────┘
│
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ │💳 Thu tháng  │ │📄 Sắp hết hạn│ │🔧 Bảo trì mở │
│ │50,000₫       │ │1 hợp đồng    │ │3 tickets     │
│ │Tiền đã thu   │ │7 ngày        │ │Chưa xong     │
│ └──────────────┘ └──────────────┘ └──────────────┘
└─────────────────────────────────────────────┘
```

### SAU:
```
┌──────────────────────────────────────────────────────┐
│ Xin chào, Trần Văn A! 👋                            │
│ Danh sách công việc cần xử lý hôm nay               │
│                                                      │
│ ┌────────────────┐ ┌────────────────┐ ┌──────────────┐ ┌──────────────┐
│ │🏠 Phòng Trống │ │💳 Thu tháng này│ │⏰ Quá hạn   │ │🧾 Công nợ   │
│ │5/10           │ │50,000₫         │ │2            │ │100,000₫      │
│ │Sẵn sàng thuê  │ │Tháng hiện tại  │ │Chưa thanh   │ │Còn nợ        │
│ └────────────────┘ └────────────────┘ │toán        │ └──────────────┘
│                                        │            │
│ ┌────────────────┐ ┌────────────────┐ └──────────────┘ ┌──────────────┐
│ │📄 Sắp hết hạn │ │🔧 Bảo trì mở   │                │⭐ Hiệu suất   │
│ │1              │ │3              │                │50%            │
│ │Hợp đồng/7 ngày│ │Chưa hoàn thành │                │Tỷ lệ cho thuê │
│ └────────────────┘ └────────────────┘                 └──────────────┘
│
│ ┌────────────────┐ (TẠM ẨN - ĐẦY TỬ ĐỦ 8 THẺ)
│ │👥 Phòng Có ng │
│ │5/10           │
│ │Đang cho thuê  │
│ └────────────────┘
└──────────────────────────────────────────────────────┘
```

## Menu Điều Hướng (Navigation)

### TRƯỚC:
```
┌──────────────────────┐
│ 🏠 Tổng quan         │
│ 🏠 Phòng             │
│ 👥 Khách thuê        │
│ 📄 Hợp đồng          │
│ 💰 Đặt cọc           │ ← Admin only
│ ⚡ Điện/Nước/DV      │
│ 🧾 Hóa đơn           │ ← Admin only
│ 💳 Thanh toán        │
│ 🔧 Bảo trì           │
│ 📦 Tài sản           │
│ 📊 Báo cáo           │
└──────────────────────┘
```

### SAU:
```
┌──────────────────────┐
│ 🏠 Tổng quan         │
│ ━━━━ QUẢN LÝ ━━━━    │ ← Phân loại
│ 🏠 Phòng             │
│ 👥 Khách thuê        │
│ 📄 Hợp đồng          │
│ ━━━━ TÀI CHÍNH ━━━━  │ ← Phân loại
│ ⚡ Điện/Nước/DV      │
│ 💳 Thanh toán        │
│ ━━ BẢO TRÌNHÁNG ━━   │ ← Phân loại
│ 🔧 Bảo trì           │
│ 📦 Tài sản           │
│ ━━━━ BÁOCÁO ━━━━     │ ← Phân loại
│ 📊 Báo cáo           │
│                      │
│ (💰 Đặt cọc → ẨN)   │ ← Không hiển thị
│ (🧾 Hóa đơn → ẨN)   │ ← Không hiển thị
└──────────────────────┘
```

## Chi Tiết Thay Đổi

### 1. **Tiêu đề Dashboard**
| Trước | Sau |
|-------|-----|
| `🏠 Tổng quan` | `🏠 Tổng quan - Công việc hôm nay` |
| `Công việc hôm nay` | `Xin chào, {TênNhânViên}! 👋` |
| - | `Danh sách công việc cần xử lý hôm nay` |

### 2. **Metrics Cards**
| Vị Trí | Trước | Sau |
|--------|-------|-----|
| Grid | 3 cột × 2 hàng (6 thẻ) | 4 cột × 2 hàng (8 thẻ) |
| (0,0) | `🏠 Phòng` (Occupied) | `🏠 Phòng Trống` (Empty) |
| (1,0) | `🧾 Công nợ` | `💳 Thu tháng này` |
| (2,0) | `⏰ Quá hạn` | `⏰ Quá hạn` |
| (0,1) | `💳 Thu tháng này` | `🧾 Công nợ` |
| (1,1) | `📄 Sắp hết hạn` | `📄 Sắp hết hạn` |
| (2,1) | `🔧 Bảo trì mở` | `🔧 Bảo trì mở` |
| (3,0) | - | `👥 Phòng Có người` |
| (3,1) | - | `⭐ Hiệu suất` |

### 3. **Menu Navigation**
- ✅ Thêm tiêu đề phân loại để tổ chức tốt hơn
- ❌ Ẩn `💰 Đặt cọc` - Không được quyền (Visible = false)
- ❌ Ẩn `🧾 Hóa đơn` - Không được quyền (Visible = false)

### 4. **Color Scheme**
| Card | Color | RGB |
|------|-------|-----|
| Phòng Trống | Teal | (0, 150, 136) |
| Thu tháng này | Green | (76, 175, 80) |
| Quá hạn | Orange | (255, 152, 0) |
| Công nợ | Red | (244, 67, 54) |
| Sắp hết hạn | Purple | (103, 58, 183) |
| Bảo trì | Magenta | (156, 39, 176) |
| Phòng Có người | Blue | (33, 150, 243) |
| Hiệu suất | Cyan | (0, 172, 193) |

## Dữ Liệu Được Lọc

Tất cả các module đều sử dụng:
```csharp
FilterByBranch(data, _branchId)
```

Đảm bảo nhân viên chỉ xem dữ liệu của chi nhánh được phân công.

## Các Quyền Được Phép

| Tính Năng | Trước | Sau | Chi Tiết |
|-----------|-------|-----|----------|
| Quản lý phòng | ✅ | ✅ | Xem + cập nhật trạng thái |
| Khách thuê | ✅ | ✅ | Xem, check-in/out, cập nhật |
| Hợp đồng | ✅ | ✅ | Tạo, gia hạn, kết thúc |
| Điện/Nước | ✅ | ✅ | Nhập chỉ số, cập nhật dịch vụ |
| Thanh toán | ✅ | ✅ | Thu tiền, xác nhận |
| Bảo trì | ✅ | ✅ | Lập phiếu, cập nhật, xác nhận |
| Tài sản | ✅ | ✅ | Kiểm tra, báo cáo hư hỏng |
| Báo cáo | ✅ | ✅ | Xem báo cáo chi nhánh |
| **Đặt cọc** | ✅ | ❌ | **ẨN - Admin only** |
| **Hóa đơn** | ✅ | ❌ | **ẨN - Admin only** |

---

**Phiên Bản**: 2.0  
**Ngày Cập Nhật**: 14/12/2025
