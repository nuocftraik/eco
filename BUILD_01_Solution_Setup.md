# Solution và Build Configuration

> 📖 [Quay lại Mục lục](BUILD_INDEX.md)

Tài liệu này hướng dẫn tạo Solution, Project Structure và Setup Build Configuration.

---

## 1. Tạo Solution và Project Structure

### Bước 1.1: Tạo Solution File

**Làm gì:** Tạo solution file để quản lý tất cả projects.

**Tại sao:** 
- Quản lý tập trung các projects
- Dễ build và restore packages
- IDE hỗ trợ tốt hơn

**Cách làm:**
```bash
dotnet new sln -n ECO.WebApi
```

**Kết quả:** File `ECO.WebApi.sln` được tạo.

---

### Bước 1.2: Tạo Project Structure

**Làm gì:** Tạo các projects theo Clean Architecture.

**Thứ tự tạo:**

```bash
# 1. Shared Layer (không phụ thuộc gì)
dotnet new classlib -n ECO.WebApi.Shared -o src/Core/Shared

# 2. Domain Layer (phụ thuộc Shared)
dotnet new classlib -n ECO.WebApi.Domain -o src/Core/Domain

# 3. Application Layer (phụ thuộc Domain, Shared)
dotnet new classlib -n ECO.WebApi.Application -o src/Core/Application

# 4. Infrastructure Layer (phụ thuộc Application, Domain)
dotnet new classlib -n ECO.WebApi.Infrastructure -o src/Infrastructure/Infrastructure

# 5. Host Layer (phụ thuộc Infrastructure, Application)
dotnet new webapi -n ECO.WebApi.Host -o src/Host/Host

# 6. Migrators (phụ thuộc Infrastructure, Domain)
dotnet new classlib -n Migrators.MSSQL -o src/Migrators/Migrators.MSSQL
```

**Thêm vào Solution:**
```bash
dotnet sln add src/Core/Shared/Shared.csproj
dotnet sln add src/Core/Domain/Domain.csproj
dotnet sln add src/Core/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure/Infrastructure.csproj
dotnet sln add src/Host/Host/Host.csproj
dotnet sln add src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj
```

**Lưu ý:** Thứ tự tạo quan trọng vì phải setup dependencies đúng.

---

## 2. Setup Build Configuration

### Bước 2.1: Tạo Directory.Build.props

**Làm gì:** Tạo file chứa cấu hình build chung cho tất cả projects.

**Tại sao:**
- Tránh lặp lại cấu hình
- Đảm bảo consistency
- Dễ maintain

**File:** `Directory.Build.props` (root directory)

```xml
<Project>
	<PropertyGroup>
		<AnalysisLevel>latest</AnalysisLevel>
		<AnalysisMode>All</AnalysisMode>
		<TreatWarningsAsErrors>false</TreatWarningsAsErrors>
		<CodeAnalysisTreatWarningsAsErrors>false</CodeAnalysisTreatWarningsAsErrors>
		<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference
			Include="StyleCop.Analyzers"
			Version="1.1.118"
			PrivateAssets="all"
			Condition="$(MSBuildProjectExtension) == '.csproj'"
		/>
		<PackageReference
			Include="SonarAnalyzer.CSharp"
			Version="9.7.0.75501"
			PrivateAssets="all"
			Condition="$(MSBuildProjectExtension) == '.csproj'"
		/>
	</ItemGroup>
</Project>
```

**Tác dụng:**
- Tự động áp dụng StyleCop và SonarAnalyzer cho mọi `.csproj`
- Không cần thêm vào từng project riêng

---

### Bước 2.2: Tạo Directory.Build.targets

**Làm gì:** Tạo file định nghĩa build targets chung.

**File:** `Directory.Build.targets`

```xml
<Project>
    <PropertyGroup>
        <DocumentationFile>$(OutputPath)$(AssemblyName).xml</DocumentationFile>
    </PropertyGroup>
</Project>
```

**Tác dụng:** Tự động generate XML documentation cho IntelliSense.

---

### Bước 2.3: Tạo stylecop.json

**Làm gì:** Cấu hình code style rules.

**File:** `stylecop.json` (root directory)

```json
{
  "$schema": "https://raw.githubusercontent.com/DotNetAnalyzers/StyleCopAnalyzers/master/StyleCop.Analyzers/StyleCop.Analyzers/Settings/stylecop.schema.json",
  "settings": {
    "orderingRules": {
      "systemUsingDirectivesFirst": true,
      "usingDirectivesPlacement": "outsideNamespace"
    },
    "layoutRules": {
      "newlineAtEndOfFile": "omit"
    }
  }
}
```

**Tác dụng:** Đảm bảo code style nhất quán trong toàn bộ solution.

---

### Bước 2.4: Tạo .editorconfig

**Làm gì:** Cấu hình editor formatting rules.

**File:** `.editorconfig` (root directory)

**Tác dụng:** Các IDE tự động áp dụng formatting rules khi edit code.

---

**Tiếp theo:** [Xây dựng Shared Layer](BUILD_02_Shared_Layer.md)
