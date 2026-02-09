# ĐẶC TẢ BACKEND

**Dự án:** Website quản lý đặt hàng và bán cà phê online

## 1. Công nghệ Backend

- **Framework:** ASP.NET Core MVC 8.0 (C#)
- **ORM:** Entity Framework Core
- **Database:** MySQL 8.0
- **Authentication:** ASP.NET Core Identity / JWT Token
- **Payment Gateway:** VNPay, MoMo API

---

## 2. Kiến trúc hệ thống

### 2.1. Mô hình MVC

```
┌─────────────────────────────────────────────────────┐
│                    CLIENT (Browser)                 │
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│                   CONTROLLERS                       │
│  HomeController, ProductController, OrderController │
│  AccountController, AdminController, PaymentController│
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│                    SERVICES                         │
│  ProductService, OrderService, UserService          │
│  PaymentService, CartService, ReportService         │
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│                  REPOSITORIES                       │
│  IProductRepository, IOrderRepository, IUserRepository│
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│              ENTITY FRAMEWORK CORE                  │
│                   DbContext                         │
└─────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│                    MySQL Database                   │
└─────────────────────────────────────────────────────┘
```

### 2.2. Cấu trúc thư mục Project

```
CoffeeShop/
├── Controllers/
│   ├── HomeController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   ├── OrderController.cs
│   ├── AccountController.cs
│   ├── PaymentController.cs
│   └── Admin/
│       ├── DashboardController.cs
│       ├── ProductManageController.cs
│       ├── OrderManageController.cs
│       ├── CustomerController.cs
│       └── ReportController.cs
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   ├── OrderDetail.cs
│   │   └── PaymentTransaction.cs
│   └── ViewModels/
│       ├── ProductViewModel.cs
│       ├── CartViewModel.cs
│       ├── CheckoutViewModel.cs
│       └── OrderViewModel.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IProductService.cs
│   │   ├── IOrderService.cs
│   │   ├── ICartService.cs
│   │   └── IPaymentService.cs
│   └── Implementations/
│       ├── ProductService.cs
│       ├── OrderService.cs
│       ├── CartService.cs
│       └── PaymentService.cs
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Data/
│   └── ApplicationDbContext.cs
├── Views/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── appsettings.json
└── Program.cs
```

---

## 3. Thiết kế Database chi tiết

### 3.1. Bảng `Users`

| Trường       | Kiểu dữ liệu                     | Ràng buộc                 | Mô tả                |
| ------------ | -------------------------------- | ------------------------- | -------------------- |
| Id           | INT                              | PK, AUTO_INCREMENT        | Mã người dùng        |
| Username     | VARCHAR(50)                      | UNIQUE, NOT NULL          | Tên đăng nhập        |
| PasswordHash | VARCHAR(255)                     | NOT NULL                  | Mật khẩu mã hóa      |
| FullName     | NVARCHAR(100)                    | NOT NULL                  | Họ tên               |
| Email        | VARCHAR(100)                     | UNIQUE, NOT NULL          | Email                |
| PhoneNumber  | VARCHAR(15)                      | NULL                      | Số điện thoại        |
| Address      | NVARCHAR(255)                    | NULL                      | Địa chỉ              |
| Role         | ENUM('Admin','Staff','Customer') | DEFAULT 'Customer'        | Vai trò              |
| IsActive     | BOOLEAN                          | DEFAULT TRUE              | Trạng thái hoạt động |
| CreatedAt    | DATETIME                         | DEFAULT CURRENT_TIMESTAMP | Ngày tạo             |
| UpdatedAt    | DATETIME                         | NULL                      | Ngày cập nhật        |

### 3.2. Bảng `Categories`

| Trường      | Kiểu dữ liệu  | Ràng buộc          | Mô tả             |
| ----------- | ------------- | ------------------ | ----------------- |
| Id          | INT           | PK, AUTO_INCREMENT | Mã danh mục       |
| Name        | NVARCHAR(100) | NOT NULL           | Tên danh mục      |
| Slug        | VARCHAR(100)  | UNIQUE             | URL-friendly name |
| Description | NVARCHAR(500) | NULL               | Mô tả             |
| ImageUrl    | VARCHAR(255)  | NULL               | Ảnh danh mục      |
| IsActive    | BOOLEAN       | DEFAULT TRUE       | Hiển thị/Ẩn       |
| SortOrder   | INT           | DEFAULT 0          | Thứ tự hiển thị   |

### 3.3. Bảng `Products`

| Trường        | Kiểu dữ liệu  | Ràng buộc                 | Mô tả                  |
| ------------- | ------------- | ------------------------- | ---------------------- |
| Id            | INT           | PK, AUTO_INCREMENT        | Mã sản phẩm            |
| CategoryId    | INT           | FK → Categories(Id)       | Mã danh mục            |
| Name          | NVARCHAR(200) | NOT NULL                  | Tên sản phẩm           |
| Slug          | VARCHAR(200)  | UNIQUE                    | URL-friendly name      |
| Description   | TEXT          | NULL                      | Mô tả chi tiết         |
| Price         | DECIMAL(18,2) | NOT NULL                  | Giá bán                |
| OriginalPrice | DECIMAL(18,2) | NULL                      | Giá gốc (nếu giảm giá) |
| StockQuantity | INT           | DEFAULT 0                 | Số lượng tồn kho       |
| ImageUrl      | VARCHAR(255)  | NULL                      | Ảnh chính              |
| Images        | JSON          | NULL                      | Danh sách ảnh phụ      |
| IsFeatured    | BOOLEAN       | DEFAULT FALSE             | Sản phẩm nổi bật       |
| IsActive      | BOOLEAN       | DEFAULT TRUE              | Hiển thị/Ẩn            |
| ViewCount     | INT           | DEFAULT 0                 | Lượt xem               |
| CreatedAt     | DATETIME      | DEFAULT CURRENT_TIMESTAMP | Ngày tạo               |
| UpdatedAt     | DATETIME      | NULL                      | Ngày cập nhật          |

### 3.4. Bảng `Orders`

| Trường          | Kiểu dữ liệu                                                | Ràng buộc                 | Mô tả                          |
| --------------- | ----------------------------------------------------------- | ------------------------- | ------------------------------ |
| Id              | INT                                                         | PK, AUTO_INCREMENT        | Mã đơn hàng                    |
| OrderCode       | VARCHAR(20)                                                 | UNIQUE, NOT NULL          | Mã đơn hàng (DH20260115001)    |
| UserId          | INT                                                         | FK → Users(Id), NULL      | Mã khách hàng (NULL nếu guest) |
| CustomerName    | NVARCHAR(100)                                               | NOT NULL                  | Tên người nhận                 |
| CustomerPhone   | VARCHAR(15)                                                 | NOT NULL                  | SĐT người nhận                 |
| CustomerEmail   | VARCHAR(100)                                                | NULL                      | Email                          |
| ShippingAddress | NVARCHAR(255)                                               | NOT NULL                  | Địa chỉ giao hàng              |
| SubTotal        | DECIMAL(18,2)                                               | NOT NULL                  | Tạm tính                       |
| ShippingFee     | DECIMAL(18,2)                                               | DEFAULT 0                 | Phí vận chuyển                 |
| Discount        | DECIMAL(18,2)                                               | DEFAULT 0                 | Giảm giá                       |
| TotalAmount     | DECIMAL(18,2)                                               | NOT NULL                  | Tổng tiền                      |
| PaymentMethod   | ENUM('COD','BankTransfer','QRCode')                         | NOT NULL                  | Phương thức thanh toán         |
| PaymentStatus   | ENUM('Pending','Paid','Failed')                             | DEFAULT 'Pending'         | Trạng thái thanh toán          |
| OrderStatus     | ENUM('New','Processing','Shipping','Delivered','Cancelled') | DEFAULT 'New'             | Trạng thái đơn                 |
| Note            | NVARCHAR(500)                                               | NULL                      | Ghi chú                        |
| OrderDate       | DATETIME                                                    | DEFAULT CURRENT_TIMESTAMP | Ngày đặt                       |
| UpdatedAt       | DATETIME                                                    | NULL                      | Ngày cập nhật                  |

### 3.5. Bảng `OrderDetails`

| Trường      | Kiểu dữ liệu  | Ràng buộc          | Mô tả             |
| ----------- | ------------- | ------------------ | ----------------- |
| Id          | INT           | PK, AUTO_INCREMENT | ID                |
| OrderId     | INT           | FK → Orders(Id)    | Mã đơn hàng       |
| ProductId   | INT           | FK → Products(Id)  | Mã sản phẩm       |
| ProductName | NVARCHAR(200) | NOT NULL           | Tên SP (snapshot) |
| UnitPrice   | DECIMAL(18,2) | NOT NULL           | Đơn giá           |
| Quantity    | INT           | NOT NULL           | Số lượng          |
| TotalPrice  | DECIMAL(18,2) | NOT NULL           | Thành tiền        |

### 3.6. Bảng `Carts` (Giỏ hàng)

| Trường    | Kiểu dữ liệu | Ràng buộc                 | Mô tả                          |
| --------- | ------------ | ------------------------- | ------------------------------ |
| Id        | INT          | PK, AUTO_INCREMENT        | ID giỏ hàng                    |
| UserId    | INT          | FK → Users(Id), NULL      | Mã người dùng (NULL nếu guest) |
| SessionId | VARCHAR(100) | NULL                      | Session ID cho guest           |
| CreatedAt | DATETIME     | DEFAULT CURRENT_TIMESTAMP | Ngày tạo                       |
| UpdatedAt | DATETIME     | NULL                      | Ngày cập nhật                  |

### 3.7. Bảng `CartItems` (Chi tiết giỏ hàng)

| Trường    | Kiểu dữ liệu | Ràng buộc                 | Mô tả       |
| --------- | ------------ | ------------------------- | ----------- |
| Id        | INT          | PK, AUTO_INCREMENT        | ID          |
| CartId    | INT          | FK → Carts(Id)            | Mã giỏ hàng |
| ProductId | INT          | FK → Products(Id)         | Mã sản phẩm |
| Quantity  | INT          | NOT NULL, DEFAULT 1       | Số lượng    |
| CreatedAt | DATETIME     | DEFAULT CURRENT_TIMESTAMP | Ngày thêm   |

### 3.8. Bảng `Payments` (Thanh toán)

| Trường        | Kiểu dữ liệu  | Ràng buộc                 | Mô tả                                |
| ------------- | ------------- | ------------------------- | ------------------------------------ |
| Id            | INT           | PK, AUTO_INCREMENT        | Mã thanh toán                        |
| OrderId       | INT           | FK → Orders(Id)           | Mã đơn hàng                          |
| Amount        | DECIMAL(18,2) | NOT NULL                  | Số tiền thanh toán                   |
| PaymentMethod | VARCHAR(50)   | NOT NULL                  | Phương thức (COD/VNPay/MoMo/ZaloPay) |
| Status        | VARCHAR(50)   | NOT NULL                  | Trạng thái (Pending/Success/Failed)  |
| TransactionId | VARCHAR(255)  | NULL, UNIQUE              | Mã giao dịch từ cổng thanh toán      |
| PaidAt        | DATETIME      | NULL                      | Thời gian thanh toán thành công      |
| ResponseData  | JSON          | NULL                      | Dữ liệu response từ cổng TT          |
| CreatedAt     | DATETIME      | DEFAULT CURRENT_TIMESTAMP | Ngày tạo                             |

### 3.9. Bảng `Reviews` (Đánh giá sản phẩm)

| Trường     | Kiểu dữ liệu   | Ràng buộc                 | Mô tả             |
| ---------- | -------------- | ------------------------- | ----------------- |
| Id         | INT            | PK, AUTO_INCREMENT        | ID đánh giá       |
| ProductId  | INT            | FK → Products(Id)         | Mã sản phẩm       |
| UserId     | INT            | FK → Users(Id)            | Mã người dùng     |
| Rating     | TINYINT        | NOT NULL, CHECK (1-5)     | Số sao (1-5)      |
| Comment    | NVARCHAR(1000) | NULL                      | Nội dung đánh giá |
| IsApproved | BOOLEAN        | DEFAULT FALSE             | Đã duyệt hiển thị |
| CreatedAt  | DATETIME       | DEFAULT CURRENT_TIMESTAMP | Ngày đánh giá     |

---

## 4. API Endpoints

### 4.1. Public APIs (Khách hàng)

| Method | Endpoint                 | Mô tả                  |
| ------ | ------------------------ | ---------------------- |
| GET    | `/`                      | Trang chủ              |
| GET    | `/products`              | Danh sách sản phẩm     |
| GET    | `/products/{slug}`       | Chi tiết sản phẩm      |
| GET    | `/category/{slug}`       | Sản phẩm theo danh mục |
| GET    | `/search?q={keyword}`    | Tìm kiếm               |
| POST   | `/cart/add`              | Thêm vào giỏ hàng      |
| GET    | `/cart`                  | Xem giỏ hàng           |
| POST   | `/cart/update`           | Cập nhật số lượng      |
| POST   | `/cart/remove`           | Xóa sản phẩm khỏi giỏ  |
| GET    | `/checkout`              | Trang thanh toán       |
| POST   | `/checkout`              | Đặt hàng               |
| GET    | `/order/tracking/{code}` | Theo dõi đơn hàng      |

### 4.2. Authentication APIs

| Method | Endpoint            | Mô tả               |
| ------ | ------------------- | ------------------- |
| GET    | `/account/login`    | Trang đăng nhập     |
| POST   | `/account/login`    | Xử lý đăng nhập     |
| GET    | `/account/register` | Trang đăng ký       |
| POST   | `/account/register` | Xử lý đăng ký       |
| POST   | `/account/logout`   | Đăng xuất           |
| GET    | `/account/profile`  | Thông tin tài khoản |
| POST   | `/account/profile`  | Cập nhật thông tin  |
| GET    | `/account/orders`   | Lịch sử đơn hàng    |

### 4.3. Payment APIs

| Method | Endpoint                          | Mô tả                  |
| ------ | --------------------------------- | ---------------------- |
| POST   | `/payment/create-qr`              | Tạo mã QR thanh toán   |
| POST   | `/payment/vnpay-return`           | Callback từ VNPay      |
| POST   | `/payment/momo-ipn`               | IPN từ MoMo            |
| GET    | `/payment/check-status/{orderId}` | Kiểm tra trạng thái TT |

### 4.4. Admin APIs

| Method | Endpoint                      | Mô tả                |
| ------ | ----------------------------- | -------------------- |
| GET    | `/admin`                      | Dashboard            |
| GET    | `/admin/products`             | Danh sách sản phẩm   |
| GET    | `/admin/products/create`      | Form thêm SP         |
| POST   | `/admin/products/create`      | Xử lý thêm SP        |
| GET    | `/admin/products/edit/{id}`   | Form sửa SP          |
| POST   | `/admin/products/edit/{id}`   | Xử lý sửa SP         |
| POST   | `/admin/products/delete/{id}` | Xóa SP               |
| GET    | `/admin/orders`               | Danh sách đơn hàng   |
| GET    | `/admin/orders/{id}`          | Chi tiết đơn         |
| POST   | `/admin/orders/update-status` | Cập nhật trạng thái  |
| GET    | `/admin/customers`            | Danh sách khách hàng |
| GET    | `/admin/reports`              | Báo cáo thống kê     |

---

## 5. Xử lý nghiệp vụ chính

### 5.1. Quy trình đặt hàng

```
1. Khách thêm SP vào giỏ hàng (Session/Cookie)
2. Vào trang Checkout, nhập thông tin giao hàng
3. Chọn phương thức thanh toán
   - COD: Tạo đơn, Status = New, PaymentStatus = Pending
   - QR: Gọi API tạo QR → Hiển thị QR → Chờ callback
4. Sau khi thanh toán thành công → Gửi email xác nhận
5. Admin xử lý đơn: New → Processing → Shipping → Delivered
```

### 5.2. Quy trình thanh toán QR

```
1. Khách chọn thanh toán QR tại Checkout
2. Backend gọi VNPay/MoMo API để tạo QR
3. Frontend hiển thị mã QR
4. Khách quét QR và thanh toán
5. Cổng TT gửi callback (IPN) về Backend
6. Backend cập nhật PaymentStatus = Paid
7. Frontend polling hoặc SignalR để cập nhật UI
```

---

## 6. Bảo mật

- **Mã hóa mật khẩu:** BCrypt hoặc ASP.NET Identity PasswordHasher
- **HTTPS:** Bắt buộc cho production
- **CSRF Protection:** AntiForgery Token
- **Input Validation:** Data Annotations + FluentValidation
- **Authorization:** Role-based (Admin, Staff, Customer)
- **Rate Limiting:** Giới hạn request để chống DDoS
- **SQL Injection:** Sử dụng EF Core parameterized queries

---

## 7. Cấu hình (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoffeeShopDB;User=root;Password=xxx;"
  },
  "PaymentSettings": {
    "VNPay": {
      "TmnCode": "xxx",
      "HashSecret": "xxx",
      "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
      "ReturnUrl": "https://yourdomain.com/payment/vnpay-return"
    },
    "MoMo": {
      "PartnerCode": "xxx",
      "AccessKey": "xxx",
      "SecretKey": "xxx"
    }
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "CoffeeShop",
    "ExpireMinutes": 60
  }
}
```
