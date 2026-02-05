---
description: Các lệnh database thường dùng
---

# Database Commands

// turbo-all

## Entity Framework Migrations

1. Tạo migration mới:

```bash
cd d:\githubclone\Coffeeshop\src\CoffeeShop.Web
dotnet ef migrations add [TenMigration]
```

2. Áp dụng migration vào database:

```bash
dotnet ef database update
```

3. Xem danh sách migrations:

```bash
dotnet ef migrations list
```

4. Rollback migration cuối:

```bash
dotnet ef migrations remove
```

## Lưu ý

- Thay [TenMigration] bằng tên mô tả, ví dụ: AddProductReviewTable
- Backup database trước khi chạy migration trên production
