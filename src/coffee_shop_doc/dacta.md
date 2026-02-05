# ĐẶC TẢ YÊU CẦU PHẦN MỀM

**Tên đề tài:** Xây dựng website quản lý đặt hàng và bán cà phê online

## 1. Tổng quan dự án

Dự án nhằm mục đích xây dựng một hệ thống website thương mại điện tử chuyên biệt cho việc bán cà phê. Hệ thống phục vụ hai đối tượng chính: Khách hàng (tìm kiếm, đặt mua sản phẩm) và Quản trị viên (quản lý sản phẩm, đơn hàng, khách hàng).

## 2. Công nghệ sử dụng

- **Backend:** ASP.NET Core MVC (C#) + Entity Framework Core
- **Frontend:** Razor Pages / Views + Tailwind CSS + JavaScript
- **Database:** MySQL

## 3. Chức năng chi tiết

### 3.1. Phân hệ Khách hàng (Frontend)

Dành cho người dùng vãng lai (Guest) và thành viên (Member).

**Nhóm chức năng Tài khoản & Hệ thống:**

- **Đăng ký thành viên:** Người dùng có thể tạo tài khoản mới.
- **Đăng nhập / Đăng xuất:** Xác thực người dùng vào hệ thống.
- **Quản lý thông tin cá nhân:** Cập nhật hồ sơ, đổi mật khẩu, xem lịch sử mua hàng.

**Nhóm chức năng Sản phẩm:**

- **Trang chủ:** Hiển thị sản phẩm nổi bật, sản phẩm mới, banner quảng cáo.
- **Danh sách sản phẩm:** Hiển thị sản phẩm theo danh mục (Cà phê hạt, Cà phê bột, Dụng cụ pha chế...).
- **Chi tiết sản phẩm:** Xem thông tin chi tiết, hình ảnh, giá bán, mô tả, đánh giá (nếu có).
- **Tìm kiếm & Lọc:** Tìm kiếm sản phẩm theo tên; lọc theo giá, danh mục.
- **Thanh tìm kiếm sản phẩm (Chi tiết):**
  - Hỗ trợ tìm kiếm realtime khi người dùng gõ (debounce 300ms để tối ưu hiệu suất).
  - Hiển thị dropdown gợi ý sản phẩm ngay dưới thanh tìm kiếm.
  - Mỗi kết quả gợi ý hiển thị: Ảnh thu nhỏ, Tên sản phẩm, Giá.
  - Tìm kiếm theo nhiều trường: tên sản phẩm, mô tả, tên danh mục.
  - Giới hạn 5-8 kết quả gợi ý để tối ưu UX.
  - Click vào kết quả gợi ý chuyển đến trang chi tiết sản phẩm.
  - Nhấn Enter hoặc nút tìm kiếm chuyển đến trang danh sách kết quả tìm kiếm.

**Nhóm chức năng Giỏ hàng & Đặt hàng:**

- **Thêm vào giỏ hàng:** Thêm sản phẩm với số lượng tùy chọn.
- **Xem giỏ hàng:** Xem danh sách sản phẩm đã chọn, cập nhật số lượng, xóa sản phẩm.
- **Thanh toán (Checkout):** Nhập thông tin giao hàng, chọn phương thức thanh toán (COD, Chuyển khoản, Thanh toán QR).
- **Thanh toán qua QR Code:**
  - Hỗ trợ thanh toán qua VNPay QR, MoMo, ZaloPay hoặc QR ngân hàng (VietQR).
  - Hiển thị mã QR động chứa thông tin đơn hàng (số tiền, mã đơn).
  - Tự động xác nhận thanh toán thành công sau khi nhận callback từ cổng thanh toán.
  - Ghi log giao dịch để đối soát.
- **Theo dõi đơn hàng:** Kiểm tra trạng thái đơn hàng (Mới, Đang xử lý, Đang giao, Hoàn tất).

### 3.2. Phân hệ Quản trị (Backend - Admin)

Dành cho Quản trị viên (Admin) và Nhân viên (Staff).

**Nhóm chức năng Quản lý Danh mục & Sản phẩm:**

- **Quản lý Danh mục:** Thêm, Sửa, Xóa danh mục sản phẩm.
- **Quản lý Sản phẩm:** Thêm mới, cập nhật thông tin, thay đổi giá, quản lý ảnh, ẩn/hiện sản phẩm.

**Nhóm chức năng Quản lý Đơn hàng:**

- **Danh sách đơn hàng:** Xem tất cả đơn hàng với các trạng thái khác nhau.
- **Chi tiết đơn hàng:** Xem thông tin người mua, sản phẩm mua, tổng tiền.
- **Cập nhật trạng thái:** Duyệt đơn, xác nhận giao hàng, hủy đơn, hoàn tất.

**Nhóm chức năng Quản lý Người dùng:**

- **Danh sách người dùng:** Xem danh sách khách hàng đã đăng ký.
- **Quản lý nhân viên:** (Nếu có) Phân quyền quản trị.

**Nhóm chức năng Báo cáo & Thống kê:**

- **Thống kê doanh thu:** Theo ngày, tháng, năm.
- **Thống kê sản phẩm bán chạy:** Top sản phẩm được mua nhiều nhất.

## 4. Thiết kế Cơ sở dữ liệu (Dự kiến - Database Schema)

Sử dụng MySQL.

**1. Bảng `Users` (Người dùng)**

- `Id` (PK), `Username`, `PasswordHash`, `FullName`, `Email`, `PhoneNumber`, `Address`, `Role` (Admin/Customer), `CreatedAt`.

**2. Bảng `Categories` (Danh mục)**

- `Id` (PK), `Name`, `Description`, `IsActive`.

**3. Bảng `Products` (Sản phẩm)**

- `Id` (PK), `CategoryId` (FK), `Name`, `Description`, `Price`, `StockQuantity`, `ImageUrl`, `CreatedAt`.

**4. Bảng `Orders` (Đơn hàng)**

- `Id` (PK), `UserId` (FK), `OrderDate`, `TotalAmount`, `Status` (Pending, Processing, Shipped, Delivered, Cancelled), `ShippingAddress`, `PaymentMethod`.

**5. Bảng `OrderDetails` (Chi tiết đơn hàng)**

- `Id` (PK), `OrderId` (FK), `ProductId` (FK), `Quantity`, `UnitPrice`, `TotalPrice`.

**6. Bảng `Carts` (Giỏ hàng)**

- `Id` (PK), `UserId` (FK), `SessionId` (cho guest), `CreatedAt`, `UpdatedAt`.

**7. Bảng `CartItems` (Chi tiết giỏ hàng)**

- `Id` (PK), `CartId` (FK), `ProductId` (FK), `Quantity`, `CreatedAt`.

**8. Bảng `Reviews` (Đánh giá sản phẩm)**

- `Id` (PK), `ProductId` (FK), `UserId` (FK), `Rating` (1-5 sao), `Comment`, `IsApproved`, `CreatedAt`.

**9. Bảng `Payments` (Thanh toán)**

- `Id` (PK), `OrderId` (FK), `Amount`, `PaymentMethod`, `Status`, `TransactionId`, `PaidAt`, `ResponseData`, `CreatedAt`.

## 5. Yêu cầu phi chức năng

- Giao diện thân thiện, sáng/tối, Responsive (tương thích mobile/desktop) sử dụng Tailwind CSS.
- Tốc độ tải trang nhanh, phân trang.
- Bảo mật thông tin khách hàng và dữ liệu đơn hàng.
- Mã nguồn tổ chức rõ ràng theo mô hình MVC.
