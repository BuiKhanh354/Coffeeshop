---
description: Template yêu cầu sửa lỗi hiệu quả
---

# Sửa Lỗi

Khi yêu cầu sửa lỗi, hãy cung cấp thông tin sau:

## Template yêu cầu

```
Lỗi: [Mô tả ngắn gọn lỗi]
File: [File/Controller/View bị lỗi, nếu biết]
Bước tái hiện:
1. [Bước 1]
2. [Bước 2]
3. [Kết quả lỗi]
Kỳ vọng: [Kết quả mong muốn]
Error message: [Copy paste lỗi nếu có]
```

## Ví dụ yêu cầu tốt

```
Lỗi: Không thể đăng nhập với tài khoản admin
File: AccountController.cs
Bước tái hiện:
1. Vào trang /Account/Login
2. Nhập email: admin@test.com, password: 123456
3. Click "Đăng nhập" → Trang reload nhưng không vào được
Kỳ vọng: Chuyển đến trang Admin Dashboard
Error message: "Invalid login attempt" trong console
```

## Lưu ý

- Cung cấp error message đầy đủ giúp sửa lỗi nhanh hơn
- Screenshot lỗi nếu có thể
