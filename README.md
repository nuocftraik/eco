# ECO.WebApi

ECO.WebApi là một ASP.NET Core Web API project được xây dựng theo Clean Architecture pattern.

## 📚 Tài liệu

Xem [SETUP_GUIDE.md](SETUP_GUIDE.md) để có hướng dẫn setup chi tiết và đầy đủ.

## 🚀 Quick Start

### Migration Commands

Từ thư mục `src/Host/Host/`:

```bash
# Add Migration
dotnet ef migrations add Initial --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext -o Migrations/Application

# Update Database
dotnet ef database update --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext

# Remove Migration
dotnet ef migrations remove --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext
```

Xem [SETUP_GUIDE.md](SETUP_GUIDE.md) section 5.4 để biết thêm các migration commands khác.