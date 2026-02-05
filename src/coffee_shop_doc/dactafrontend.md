# ĐẶC TẢ UX/UI - FRONTEND
**Dự án:** Website quản lý đặt hàng và bán cà phê online

## 1. Công nghệ Frontend
- **View Engine:** Razor Pages / Views (.cshtml)
- **CSS Framework:** Tailwind CSS
- **JavaScript:** Vanilla JS / jQuery (nếu cần)
- **Responsive:** Mobile-first design
- **Theme:** Hỗ trợ chế độ Sáng/Tối (Light/Dark Mode)

---

## 2. Cấu trúc Layout

### 2.1. Layout Khách hàng (Customer Layout)
```
┌─────────────────────────────────────────────────────┐
│                    HEADER                           │
│  Logo | Menu (Trang chủ, Sản phẩm, Giới thiệu)     │
│  Tìm kiếm | Giỏ hàng | Đăng nhập/Tài khoản         │
├─────────────────────────────────────────────────────┤
│                                                     │
│                   MAIN CONTENT                      │
│                                                     │
├─────────────────────────────────────────────────────┤
│                    FOOTER                           │
│  Thông tin liên hệ | Chính sách | Mạng xã hội      │
└─────────────────────────────────────────────────────┘
```

### 2.2. Layout Admin (Admin Layout)
```
┌──────────┬──────────────────────────────────────────┐
│          │              TOP BAR                     │
│          │  Tên Admin | Thông báo | Đăng xuất       │
│ SIDEBAR  ├──────────────────────────────────────────┤
│          │                                          │
│ Dashboard│              MAIN CONTENT                │
│ Sản phẩm │                                          │
│ Đơn hàng │                                          │
│ Khách hàng│                                         │
│ Báo cáo  │                                          │
│          │                                          │
└──────────┴──────────────────────────────────────────┘
```

---

## 3. Thiết kế các trang chính

### 3.1. Trang chủ (Home)
| Thành phần | Mô tả |
|------------|-------|
| Hero Banner | Slider ảnh quảng cáo, nút CTA "Mua ngay" |
| Sản phẩm nổi bật | Grid 4 cột, hiển thị 8 sản phẩm |
| Danh mục | Icon + tên danh mục, click để xem sản phẩm |
| Sản phẩm mới | Carousel sản phẩm mới nhất |
| Giới thiệu ngắn | Về thương hiệu cà phê |

### 3.2. Trang danh sách sản phẩm
| Thành phần | Mô tả |
|------------|-------|
| Sidebar lọc | Lọc theo: Danh mục, Khoảng giá, Sắp xếp |
| Product Grid | 3-4 cột, phân trang (12 sản phẩm/trang) |
| Product Card | Ảnh, Tên, Giá, Nút "Thêm giỏ hàng" |

### 3.3. Trang chi tiết sản phẩm
| Thành phần | Mô tả |
|------------|-------|
| Gallery ảnh | Ảnh chính + thumbnail nhỏ |
| Thông tin SP | Tên, Giá, Mô tả, Còn hàng/Hết hàng |
| Số lượng | Input số lượng (+/-) |
| Nút CTA | "Thêm vào giỏ hàng", "Mua ngay" |
| Sản phẩm liên quan | Carousel 4 sản phẩm cùng danh mục |

### 3.4. Trang giỏ hàng
| Thành phần | Mô tả |
|------------|-------|
| Bảng sản phẩm | Ảnh, Tên, Đơn giá, Số lượng, Thành tiền, Xóa |
| Tổng tiền | Tạm tính, Phí ship, Tổng cộng |
| Nút CTA | "Tiếp tục mua", "Thanh toán" |

### 3.5. Trang thanh toán (Checkout)
| Thành phần | Mô tả |
|------------|-------|
| Form thông tin | Họ tên, SĐT, Email, Địa chỉ giao hàng |
| Phương thức thanh toán | Radio: COD, Chuyển khoản, QR Code |
| QR Payment | Hiển thị mã QR động khi chọn thanh toán QR |
| Tóm tắt đơn hàng | Danh sách SP, Tổng tiền |
| Nút "Đặt hàng" | Submit đơn hàng |

### 3.6. Trang tài khoản
- Thông tin cá nhân (xem/sửa)
- Đổi mật khẩu
- Lịch sử đơn hàng (danh sách + chi tiết)

### 3.7. Thanh tìm kiếm sản phẩm (Search Bar)
| Thành phần | Mô tả |
|------------|-------|
| Vị trí | Header, giữa Menu và phần Actions (Giỏ hàng, Đăng nhập) |
| Kích thước | Desktop: 300-400px, Tablet: 250px, Mobile: Icon toggle |
| Input | Placeholder: "Tìm kiếm sản phẩm...", icon kính lúp |
| Dropdown | Hiện khi có kết quả, max 5 items, z-index cao |
| Item kết quả | Ảnh 40x40px, Tên sản phẩm (truncate), Giá |
| Loading | Spinner khi đang fetch, ẩn khi hoàn tất |
| Empty State | Thông báo "Không tìm thấy sản phẩm" |
| Animation | Focus: border highlight, Dropdown: fade-in 0.2s |
| Mobile | Hiển thị icon, click mở fullscreen search overlay |

**Trạng thái (States):**
- **Default:** Input với placeholder, icon kính lúp bên trái
- **Focus:** Border đổi màu primary (`#6F4E37`), shadow nhẹ
- **Typing:** Hiển thị loading spinner (nếu đang fetch)
- **Has Results:** Dropdown với danh sách sản phẩm
- **No Results:** Dropdown với thông báo không tìm thấy
- **Mobile Expanded:** Overlay fullscreen với input và nút đóng

---

## 4. Thiết kế trang Admin

### 4.1. Dashboard
- Thống kê nhanh: Doanh thu hôm nay, Đơn hàng mới, Tổng sản phẩm
- Biểu đồ doanh thu (Line chart - 7 ngày gần nhất)
- Đơn hàng gần đây (bảng 5 đơn mới nhất)

### 4.2. Quản lý sản phẩm
- Bảng danh sách: ID, Ảnh, Tên, Danh mục, Giá, Tồn kho, Trạng thái
- Nút: Thêm mới, Sửa, Xóa
- Form thêm/sửa: Upload ảnh, nhập thông tin

### 4.3. Quản lý đơn hàng
- Bảng: Mã đơn, Khách hàng, Ngày đặt, Tổng tiền, Trạng thái
- Lọc theo trạng thái: Tất cả, Chờ xử lý, Đang giao, Hoàn thành, Đã hủy
- Chi tiết đơn: Thông tin khách, Danh sách SP, Cập nhật trạng thái

### 4.4. Quản lý khách hàng
- Bảng: ID, Họ tên, Email, SĐT, Ngày đăng ký
- Xem chi tiết: Thông tin + Lịch sử mua hàng

### 4.5. Báo cáo thống kê
- Doanh thu theo ngày/tháng/năm (biểu đồ)
- Top sản phẩm bán chạy
- Xuất báo cáo Excel/PDF

---

## 5. Bảng màu & Typography

### Bảng màu chính (Coffee Theme)
| Tên | Mã màu | Sử dụng |
|-----|--------|---------|
| Primary | `#6F4E37` | Nút CTA, Header |
| Secondary | `#C4A484` | Hover, Border |
| Accent | `#D4A574` | Highlight, Badge |
| Background | `#FDF5E6` | Nền sáng |
| Dark BG | `#1A1A1A` | Nền tối |
| Text | `#333333` | Văn bản chính |
| Text Light | `#FFFFFF` | Văn bản trên nền tối |

### Typography
- **Font chính:** Inter, Roboto hoặc Nunito (Google Fonts)
- **Heading:** Bold, size 24-32px
- **Body:** Regular, size 14-16px

---

## 6. Responsive Breakpoints
| Breakpoint | Width | Mô tả |
|------------|-------|-------|
| Mobile | < 640px | 1 cột, menu hamburger |
| Tablet | 640px - 1024px | 2 cột |
| Desktop | > 1024px | 3-4 cột, sidebar hiện |

---

## 7. Hiệu ứng & Animation
- Hover trên product card: Scale nhẹ (1.02), shadow
- Transition mượt cho menu, modal (0.3s ease)
- Loading spinner khi fetch data
- Toast notification cho thông báo (thêm giỏ hàng, đặt hàng thành công)
