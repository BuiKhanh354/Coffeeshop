# DATABASE SCHEMA - COFFEE SHOP

**MySQL Workbench 8.0 CE**

## Tạo Database

```sql
CREATE DATABASE IF NOT EXISTS CoffeeShopDB
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE CoffeeShopDB;
```

---

## 1. Bảng `Users` (Người dùng)

```sql
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber VARCHAR(15) NULL,
    Address NVARCHAR(255) NULL,
    Role ENUM('Admin', 'Staff', 'Customer') DEFAULT 'Customer',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);
```

---

## 2. Bảng `Categories` (Danh mục)

```sql
CREATE TABLE Categories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug VARCHAR(100) UNIQUE,
    Description NVARCHAR(500) NULL,
    ImageUrl VARCHAR(255) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    SortOrder INT DEFAULT 0
);
```

---

## 3. Bảng `Products` (Sản phẩm)

```sql
CREATE TABLE Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Slug VARCHAR(200) UNIQUE,
    Description TEXT NULL,
    Price DECIMAL(18,2) NOT NULL,
    OriginalPrice DECIMAL(18,2) NULL,
    StockQuantity INT DEFAULT 0,
    ImageUrl VARCHAR(255) NULL,
    Images JSON NULL,
    IsFeatured BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    ViewCount INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);
```

---

## 4. Bảng `Orders` (Đơn hàng)

```sql
CREATE TABLE Orders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderCode VARCHAR(20) NOT NULL UNIQUE,
    UserId INT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone VARCHAR(15) NOT NULL,
    CustomerEmail VARCHAR(100) NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) DEFAULT 0,
    Discount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod ENUM('COD', 'BankTransfer', 'QRCode') NOT NULL,
    PaymentStatus ENUM('Pending', 'Paid', 'Failed') DEFAULT 'Pending',
    OrderStatus ENUM('New', 'Processing', 'Shipping', 'Delivered', 'Cancelled') DEFAULT 'New',
    Note NVARCHAR(500) NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
```

---

## 5. Bảng `OrderDetails` (Chi tiết đơn hàng)

```sql
CREATE TABLE OrderDetails (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
);
```

---

## 6. Bảng `Carts` (Giỏ hàng)

```sql
CREATE TABLE Carts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    SessionId VARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

---

## 7. Bảng `CartItems` (Chi tiết giỏ hàng)

```sql
CREATE TABLE CartItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

---

## 8. Bảng `Payments` (Thanh toán)

```sql
CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    TransactionId VARCHAR(255) NULL UNIQUE,
    PaidAt DATETIME NULL,
    ResponseData JSON NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
```

---

## 9. Bảng `Reviews` (Đánh giá sản phẩm)

```sql
CREATE TABLE Reviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating TINYINT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000) NULL,
    IsApproved BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

---

## Script tạo tất cả bảng (Full Script)

```sql
-- =============================================
-- COFFEE SHOP DATABASE - MySQL 8.0
-- =============================================

CREATE DATABASE IF NOT EXISTS CoffeeShopDB
CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE CoffeeShopDB;

-- 1. Users
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber VARCHAR(15) NULL,
    Address NVARCHAR(255) NULL,
    Role ENUM('Admin', 'Staff', 'Customer') DEFAULT 'Customer',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

-- 2. Categories
CREATE TABLE Categories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug VARCHAR(100) UNIQUE,
    Description NVARCHAR(500) NULL,
    ImageUrl VARCHAR(255) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    SortOrder INT DEFAULT 0
);

-- 3. Products
CREATE TABLE Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Slug VARCHAR(200) UNIQUE,
    Description TEXT NULL,
    Price DECIMAL(18,2) NOT NULL,
    OriginalPrice DECIMAL(18,2) NULL,
    StockQuantity INT DEFAULT 0,
    ImageUrl VARCHAR(255) NULL,
    Images JSON NULL,
    IsFeatured BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    ViewCount INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);

-- 4. Orders
CREATE TABLE Orders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderCode VARCHAR(20) NOT NULL UNIQUE,
    UserId INT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone VARCHAR(15) NOT NULL,
    CustomerEmail VARCHAR(100) NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) DEFAULT 0,
    Discount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod ENUM('COD', 'BankTransfer', 'QRCode') NOT NULL,
    PaymentStatus ENUM('Pending', 'Paid', 'Failed') DEFAULT 'Pending',
    OrderStatus ENUM('New', 'Processing', 'Shipping', 'Delivered', 'Cancelled') DEFAULT 'New',
    Note NVARCHAR(500) NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- 5. OrderDetails
CREATE TABLE OrderDetails (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
);

-- 6. Carts
CREATE TABLE Carts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    SessionId VARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- 7. CartItems
CREATE TABLE CartItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- 8. Payments
CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    TransactionId VARCHAR(255) NULL UNIQUE,
    PaidAt DATETIME NULL,
    ResponseData JSON NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);

-- 9. Reviews
CREATE TABLE Reviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating TINYINT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000) NULL,
    IsApproved BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =============================================
-- END OF SCRIPT
-- =============================================
```

---

## Sơ đồ quan hệ (ERD)

```
┌──────────────┐       ┌──────────────┐
│   Users      │───────│    Carts     │
└──────────────┘   1:1 └──────────────┘
       │                      │
       │ 1:N                  │ 1:N
       ▼                      ▼
┌──────────────┐       ┌──────────────┐
│   Orders     │       │  CartItems   │──────┐
└──────────────┘       └──────────────┘      │
       │                                      │
       │ 1:N                                  │
       ▼                                      │
┌──────────────┐                              │
│ OrderDetails   │──────┐                       │
└──────────────┘      │                       │
       │              │                       │
       │ 1:N          │ N:1                   │ N:1
       ▼              ▼                       ▼
┌──────────────┐  ┌──────────────┐    ┌──────────────┐
│  Payments    │  │  Products    │────│  Categories  │
└──────────────┘  └──────────────┘ N:1└──────────────┘
                         │
                         │ 1:N
                         ▼
                  ┌──────────────┐
                  │   Reviews    │
                  └──────────────┘
```


USE CoffeeShopDB;

INSERT INTO Users
(Username, PasswordHash, FullName, Email, PhoneNumber, Address, Role, IsActive)
VALUES
('admin',  'hash_admin_123',  'Quản Trị Viên',      'admin@coffee.com',  '0901000001', 'Hà Nội',       'Admin',    TRUE),
('staff1', 'hash_staff1_123', 'Nhân Viên 1',        'staff1@coffee.com', '0901000002', 'Hà Nội',       'Staff',    TRUE),
('staff2', 'hash_staff2_123', 'Nhân Viên 2',        'staff2@coffee.com', '0901000003', 'TP HCM',       'Staff',    TRUE),
('user1',  'hash_user1_123',  'Nguyễn Văn A',       'user1@gmail.com',   '0901111111', 'Hà Nội',       'Customer', TRUE),
('user2',  'hash_user2_123',  'Trần Thị B',         'user2@gmail.com',   '0902222222', 'TP HCM',       'Customer', TRUE),
('user3',  'hash_user3_123',  'Lê Văn C',           'user3@gmail.com',   '0903333333', 'Đà Nẵng',      'Customer', TRUE),
('user4',  'hash_user4_123',  'Phạm Thị D',         'user4@gmail.com',   '0904444444', 'Hải Phòng',    'Customer', TRUE),
('user5',  'hash_user5_123',  'Hoàng Văn E',        'user5@gmail.com',   '0905555555', 'Cần Thơ',      'Customer', TRUE),
('user6',  'hash_user6_123',  'Vũ Thị F',           'user6@gmail.com',   '0906666666', 'Huế',          'Customer', TRUE),
('user7',  'hash_user7_123',  'Đỗ Văn G',           'user7@gmail.com',   '0907777777', 'Quảng Ninh',   'Customer', TRUE);


INSERT INTO Categories (Name, Slug, Description, ImageUrl, SortOrder) VALUES
('Coffee', 'coffee', 'Các loại cà phê', 'coffee.jpg', 1),
('Tea', 'tea', 'Các loại trà', 'tea.jpg', 2),
('Milk Tea', 'milk-tea', 'Trà sữa', 'milktea.jpg', 3),
('Cake', 'cake', 'Bánh ngọt', 'cake.jpg', 4),
('Snack', 'snack', 'Đồ ăn nhẹ', 'snack.jpg', 5),
('Ice Cream', 'ice-cream', 'Kem mát lạnh', 'icecream.jpg', 6),
('Juice', 'juice', 'Nước ép trái cây', 'juice.jpg', 7),
('Smoothie', 'smoothie', 'Sinh tố', 'smoothie.jpg', 8),
('Bakery', 'bakery', 'Bánh mì', 'bakery.jpg', 9),
('Special', 'special', 'Sản phẩm đặc biệt', 'special.jpg', 10);


INSERT INTO Products 
(CategoryId, Name, Slug, Description, Price, OriginalPrice, StockQuantity, ImageUrl, IsFeatured)
VALUES
(1, 'Espresso', 'espresso', 'Cà phê espresso', 30000, 35000, 100, 'espresso.jpg', TRUE),
(1, 'Latte', 'latte', 'Cà phê latte', 40000, 45000, 80, 'latte.jpg', TRUE),
(2, 'Green Tea', 'green-tea', 'Trà xanh', 25000, NULL, 60, 'greentea.jpg', FALSE),
(3, 'Milk Tea Classic', 'milk-tea-classic', 'Trà sữa truyền thống', 35000, 40000, 120, 'milktea.jpg', TRUE),
(4, 'Cheese Cake', 'cheese-cake', 'Bánh phô mai', 45000, 50000, 40, 'cheesecake.jpg', FALSE),
(5, 'French Fries', 'french-fries', 'Khoai tây chiên', 30000, NULL, 70, 'fries.jpg', FALSE),
(6, 'Vanilla Ice Cream', 'vanilla-ice-cream', 'Kem vani', 25000, NULL, 90, 'vanilla.jpg', FALSE),
(7, 'Orange Juice', 'orange-juice', 'Nước cam ép', 28000, NULL, 50, 'orange.jpg', FALSE),
(8, 'Avocado Smoothie', 'avocado-smoothie', 'Sinh tố bơ', 40000, 45000, 60, 'avocado.jpg', TRUE),
(9, 'Baguette', 'baguette', 'Bánh mì Pháp', 20000, NULL, 100, 'baguette.jpg', FALSE);

INSERT INTO Orders
(OrderCode, UserId, CustomerName, CustomerPhone, ShippingAddress,
 SubTotal, ShippingFee, Discount, TotalAmount, PaymentMethod)
VALUES
('ORD001', 1, 'Nguyễn Văn A', '0901111111', 'Hà Nội', 80000, 15000, 0, 95000, 'COD'),
('ORD002', 2, 'Trần Thị B', '0902222222', 'TP HCM', 120000, 20000, 10000, 130000, 'BankTransfer'),
('ORD003', 3, 'Lê Văn C', '0903333333', 'Đà Nẵng', 60000, 10000, 0, 70000, 'QRCode'),
('ORD004', 4, 'Phạm Thị D', '0904444444', 'Hải Phòng', 90000, 15000, 5000, 100000, 'COD'),
('ORD005', 5, 'Hoàng Văn E', '0905555555', 'Cần Thơ', 70000, 10000, 0, 80000, 'COD'),
('ORD006', 6, 'User 6', '0906666666', 'Huế', 50000, 10000, 0, 60000, 'COD'),
('ORD007', 7, 'User 7', '0907777777', 'Quảng Ninh', 110000, 15000, 0, 125000, 'BankTransfer'),
('ORD008', 8, 'User 8', '0908888888', 'Nghệ An', 65000, 10000, 0, 75000, 'QRCode'),
('ORD009', 9, 'User 9', '0909999999', 'Bình Dương', 85000, 15000, 5000, 95000, 'COD'),
('ORD010', 10,'User 10','0910000000','Long An', 100000, 20000, 0, 120000, 'COD');

INSERT INTO OrderDetails
(OrderId, ProductId, ProductName, UnitPrice, Quantity, TotalPrice)
VALUES
(1, 1, 'Espresso', 30000, 2, 60000),
(1, 2, 'Latte', 40000, 1, 40000),
(2, 4, 'Milk Tea Classic', 35000, 3, 105000),
(3, 3, 'Green Tea', 25000, 2, 50000),
(4, 5, 'Cheese Cake', 45000, 2, 90000),
(5, 6, 'French Fries', 30000, 2, 60000),
(6, 7, 'Vanilla Ice Cream', 25000, 2, 50000),
(7, 8, 'Orange Juice', 28000, 3, 84000),
(8, 9, 'Avocado Smoothie', 40000, 1, 40000),
(9,10, 'Baguette', 20000, 4, 80000);


INSERT INTO Carts (UserId, SessionId) VALUES
(1, 'sess1'), (2, 'sess2'), (3, 'sess3'), (4, 'sess4'), (5, 'sess5'),
(6, 'sess6'), (7, 'sess7'), (8, 'sess8'), (9, 'sess9'), (10, 'sess10');


INSERT INTO CartItems (CartId, ProductId, Quantity) VALUES
(1,1,2),(2,2,1),(3,3,3),(4,4,1),(5,5,2),
(6,6,1),(7,7,2),(8,8,1),(9,9,2),(10,10,3);

INSERT INTO Payments
(OrderId, Amount, PaymentMethod, Status, TransactionId, PaidAt)
VALUES
(1, 95000, 'COD', 'Pending', NULL, NULL),
(2,130000,'BankTransfer','Paid','TXN002', NOW()),
(3,70000,'QRCode','Paid','TXN003', NOW()),
(4,100000,'COD','Pending',NULL,NULL),
(5,80000,'COD','Pending',NULL,NULL),
(6,60000,'COD','Pending',NULL,NULL),
(7,125000,'BankTransfer','Paid','TXN007', NOW()),
(8,75000,'QRCode','Paid','TXN008', NOW()),
(9,95000,'COD','Pending',NULL,NULL),
(10,120000,'COD','Pending',NULL,NULL);

INSERT INTO Reviews
(ProductId, UserId, Rating, Comment, IsApproved)
VALUES
(1,1,5,'Rất ngon', TRUE),
(2,2,4,'Khá ổn', TRUE),
(3,3,5,'Trà thơm', TRUE),
(4,4,3,'Hơi ngọt', FALSE),
(5,5,5,'Bánh tuyệt vời', TRUE),
(6,6,4,'Ăn giòn', TRUE),
(7,7,5,'Kem mát', TRUE),
(8,8,4,'Nước ép tươi', TRUE),
(9,9,5,'Sinh tố béo', TRUE),
(10,10,4,'Bánh mì ok', TRUE);

INSERT INTO Reviews
(ProductId, UserId, Rating, Comment, IsApproved)
VALUES
(1,1,5,'Rất ngon', TRUE),
(2,2,4,'Khá ổn', TRUE),
(3,3,5,'Trà thơm', TRUE),
(4,4,3,'Hơi ngọt', FALSE),
(5,5,5,'Bánh tuyệt vời', TRUE),
(6,6,4,'Ăn giòn', TRUE),
(7,7,5,'Kem mát', TRUE),
(8,8,4,'Nước ép tươi', TRUE),
(9,9,5,'Sinh tố béo', TRUE),
(10,10,4,'Bánh mì ok', TRUE);

