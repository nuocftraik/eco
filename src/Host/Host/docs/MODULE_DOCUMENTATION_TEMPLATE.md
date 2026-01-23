# Template cho Module Documentation

> ?? **M?c ?ích:** Template này giúp vi?t documentation nh?t quán cho m?i module/feature trong ECO.WebApi solution.

---

## ?? Nguyên t?c Vi?t Docs

### **1. Self-Contained (T? ??)**
- ? **PH?I** có ??y ?? code trong docs
- ? **KHÔNG** reference ??n code có s?n trong workspace
- ? M?c ?ích: T?o l?i solution t? ??u ch? t? docs
- ? Copy/paste code t? docs ph?i ch?y ???c ngay

### **2. Ti?ng Vi?t & D? Hi?u**
- ? Gi?i thích b?ng ti?ng Vi?t
- ? Thu?t ng? ti?ng Anh có gi?i thích
- ? Code comments b?ng ti?ng Vi?t (ho?c ti?ng Anh rõ ràng)
- ? Ví d? th?c t?, g?n g?i

### **3. Chia Nh? Docs Ph?c T?p**
- ? Main doc: Focus usage & overview
- ? Sub docs: Chi ti?t implementation (BUILD_XX_DetailName.md)
- ? Ví d?: BUILD_11 + BUILD_11_Specification

### **4. Code Quality**
- ? Code ph?i compile ???c
- ? Có comments gi?i thích logic
- ? Namespace ?úng chu?n project
- ? Follow naming conventions

---

## ?? C?u trúc chu?n cho m?i Module Doc

### **Header Section**
```markdown
# [Module Name] - [Short Description]

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** [B??c tr??c ?ó ph?i hoàn thành]

Tài li?u này h??ng d?n xây d?ng [Module Name] - [Purpose].

---
```

**Ví d?:**
```markdown
# Repository Pattern và Specification

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** B??c 10 (Service Registration) ?ã hoàn thành

Tài li?u này h??ng d?n v? Repository Pattern v?i Ardalis.Specification và Domain Events.

---
```

---

### **Section 1: Overview (T?ng quan)**

**Required elements:**
- "Làm gì" (What)
- "T?i sao c?n" (Why)
- "Trong b??c này chúng ta s?" (Checklist)
- Real-world example (n?u ph?c t?p)

```markdown
## 1. Overview

**Làm gì:** [Mô t? ng?n g?n module này làm gì]

**T?i sao c?n:**
- **[Lý do 1]:** [Gi?i thích]
- **[Lý do 2]:** [Gi?i thích]
- **[Lý do 3]:** [Gi?i thích]

**Trong b??c này chúng ta s?:**
- ? [Task 1]
- ? [Task 2]
- ? [Task 3]

**Real-world example:** (n?u module ph?c t?p)
```csharp
// Ví d? usage code ?? ng??i ??c hi?u ???c m?c ?ích
public class ExampleUsage
{
    // ...
}
\```

---
```

**Ví d? th?c t?:**
```markdown
## 1. Overview

**Làm gì:** Setup Repository Pattern v?i Specification ?? query linh ho?t và Domain Events t? ??ng.

**T?i sao c?n:**
- **Abstraction:** Tách Application kh?i Infrastructure (EF Core)
- **Flexible Query:** Specification pattern cho complex queries
- **Domain Events:** T? ??ng phát events khi entity thay ??i
- **Testable:** D? mock repositories cho unit tests

**Trong b??c này chúng ta s?:**
- ? T?o Search/Filter models
- ? T?o Repository interfaces
- ? Implement repositories v?i EF Core
- ? Setup EventAddingRepositoryDecorator
- ? T?o Base Specifications ?? reuse

**Real-world example:**
```csharp
// Controller
public class ProductsController
{
    public async Task<ActionResult> Search([FromBody] SearchProductsRequest request)
    {
        // Specification t? ??ng build query t? request
        var spec = new ProductsBySearchSpec(request);
   
        var products = await _repository.ListAsync(spec);
      var count = await _repository.CountAsync(spec);
        
        return Ok(new PaginatedResult(products, count));
 }
}
\```

---
```

---

### **Section 2: Add Required Packages**

**Required elements:**
- Packages v?i version c? th?
- Gi?i thích "Why" cho m?i package
- File path chính xác

```markdown
## 2. Add Required Packages

### B??c 2.1: [Package Group Name]

**File:** `src/[Project]/[Project].csproj`

```xml
<ItemGroup>
    <!-- [M?c ?ích c?a package group] -->
    <PackageReference Include="PackageName" Version="x.x.x" />
    <PackageReference Include="AnotherPackage" Version="y.y.y" />
</ItemGroup>
\```

**Gi?i thích packages:**
- `PackageName`: [T?i sao c?n package này, nó làm gì]
- `AnotherPackage`: [Gi?i thích]

**?? L?u ý:**
- [L?u ý ??c bi?t n?u có]

---
```

**Ví d? th?c t?:**
```markdown
## 2. Add Required Packages

### B??c 2.1: Add NewId Package

**File:** `src/Core/Domain/Domain.csproj`

```xml
<ItemGroup>
    <!-- For sequential GUID generation -->
    <PackageReference Include="NewId" Version="4.0.1" />
</ItemGroup>
\```

**Why NewId:**
- `NewId.Next().ToGuid()` t?o sequential GUIDs
- Better database performance (less fragmentation)
- Better indexing performance

---
```

---

### **Section 3-N: Implementation Steps**

**Required elements:**
- ?ánh s? B??c X.Y rõ ràng
- File path CHÍNH XÁC
- FULL CODE (không tóm t?t)
- Comments gi?i thích trong code
- Section "Gi?i thích" sau code
- "T?i sao" ho?c "L?i ích"

```markdown
## 3. [Major Component Name]

### B??c 3.1: [Specific Task]

**Làm gì:** [Mô t? ng?n g?n]

**T?i sao:** [Lý do c?n làm b??c này]

**File:** `src/[Project]/[Path]/[FileName].cs`

```csharp
// FULL CODE - Không tóm t?t
namespace ECO.WebApi.[Namespace];

/// <summary>
/// [XML comment mô t? class]
/// </summary>
public class ClassName
{
    /// <summary>
 /// [XML comment mô t? property/method]
    /// </summary>
    public string Property { get; set; } = default!;
 
    // Comment gi?i thích logic
    public void Method()
    {
        // Implementation v?i comments
    }
}
\```

**Gi?i thích:**
- **Property:** [Gi?i thích property]
- **Method():** [Gi?i thích method]
- **Line X-Y:** [Gi?i thích ?o?n code ??c bi?t]

**T?i sao [Design Decision]:**
- [Lý do 1]
- [Lý do 2]

**L?i ích:**
- ? [Benefit 1]
- ? [Benefit 2]

**?? L?u ý:**
- [?i?u quan tr?ng c?n nh?]

---
```

**Ví d? th?c t?:**
```markdown
## 3. T?o Domain Event Contracts

### B??c 3.1: IEvent Interface

**Làm gì:** T?o marker interface cho t?t c? domain events.

**T?i sao:** ?ánh d?u class là domain event, h? tr? generic handlers.

**File:** `src/Core/Domain/Common/Contracts/IEvent.cs`

```csharp
namespace ECO.WebApi.Domain.Common.Contracts;

/// <summary>
/// Marker interface for all domain events
/// </summary>
public interface IEvent
{
}
\```

**Gi?i thích:**
- Marker interface - không có methods
- ?ánh d?u class là m?t domain event
- T?t c? domain events ph?i implement interface này

**Why in Domain layer:**
- Events là domain concept (business logic)
- Không ph?i infrastructure concern
- Follow DDD principles

---
```

---

### **Handling Complex Implementation**

**N?u code QUÁ PH?C T?P (>200 dòng):**

**Main doc - Focus usage:**
```markdown
## 3. [Complex Component]

### ?? **T?ng quan**

[Component Name] là [mô t? ng?n g?n].

**Core methods:**
- `Method1()` - [Mô t?]
- `Method2()` - [Mô t?]

**?? Implementation Chi ti?t:**

Code c?a [Component Name] khá ph?c t?p ([lý do]).  
**FULL CODE implementation** ???c vi?t trong document riêng: **[BUILD_XX_DetailName.md](BUILD_XX_DetailName.md)**

**Trong section này chúng ta ch? h?c CÁCH S? D?NG, không ?i sâu vào implementation.**

---

### B??c 3.1: Cách s? d?ng [Component]

**Usage Example 1 - [Scenario]:**
```csharp
// Simple usage example
\```

**Usage Example 2 - [Scenario]:**
```csharp
// More complex example
\```

**?? ?? hi?u chi ti?t implementation:**
- [Technical detail 1]
- [Technical detail 2]

? Xem [BUILD_XX_DetailName.md](BUILD_XX_DetailName.md)

---
```

**Sub doc (BUILD_XX_DetailName.md) - Full implementation:**
```markdown
# [Component Name] - Chi ti?t Implementation

> ?? [Quay l?i BUILD_XX](BUILD_XX_Main.md)

Document này ch?a FULL CODE implementation c?a [Component Name].  
?ây là ph?n ph?c t?p v?i [technical aspects].

---

## 1. Overview

**File này implement:**
- [Feature 1]
- [Feature 2]

**Dependencies:**
- [Dependency 1]
- [Dependency 2]

---

## 2. Full Implementation

### B??c 2.1: [Part 1]

**File:** `src/[Project]/[Path]/[FileName].cs`

```csharp
// FULL CODE (có th? 500+ dòng)
namespace ECO.WebApi.[Namespace];

public static class ComplexClass
{
    // Full implementation v?i comments chi ti?t
}
\```

**Gi?i thích chi ti?t:**
- [Section 1]: [Detailed explanation]
- [Section 2]: [Detailed explanation]

---

## 3. Flow Diagrams

### 3.1: [Process Name] Flow

\```
[ASCII diagram or detailed explanation]
\```

---

## 4. Usage Examples

[Multiple detailed examples]

---

## 5. Performance Considerations

[Performance tips]

---

## 6. Testing

[Testing examples]

---

**Quay l?i:** [BUILD_XX - Main](BUILD_XX_Main.md)
```

---

### **Section: Examples & Usage**

**Required elements:**
- Complete working examples
- Request DTOs
- Response DTOs
- Handler implementation
- Controller usage

```markdown
## [N]. Usage Examples

### B??c [N].1: Complete Example - [Feature Name]

**Request DTOs:**
```csharp
// Request models v?i full code
public class RequestDto
{
// Properties v?i comments
}
\```

**Response DTOs:**
```csharp
// Response models v?i full code
\```

**Specifications:** (n?u có)
```csharp
// Specification v?i full code
\```

**Handler:**
```csharp
// Handler implementation v?i full code
public class FeatureHandler : IRequestHandler<Request, Response>
{
// Full implementation v?i comments
}
\```

**Controller Usage:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class FeatureController : ControllerBase
{
    // Controller code
}
\```

**API Call Example:**
```bash
curl -X POST https://localhost:7001/api/endpoint \
  -H "Content-Type: application/json" \
  -d '{
    "key": "value"
  }'
\```

**Expected Response:**
```json
{
  "success": true,
  "data": {...}
}
\```

---
```

---

### **Section: Summary**

**Required elements:**
- Checklist nh?ng gì ?ã hoàn thành
- Architecture diagram (n?u ph?c t?p)
- Key concepts
- File structure

```markdown
## [N]. Summary

### ? ?ã hoàn thành trong b??c này:

**[Category 1]:**
- ? [Item 1]
- ? [Item 2]

**[Category 2]:**
- ? [Item 1]
- ? [Item 2]

### ?? Architecture Diagram: (n?u ph?c t?p)

\```
Component A
    ?
Component B
    ?
Component C
\```

### ?? Key Concepts:

**[Concept 1]:**
- [Explanation point 1]
- [Explanation point 2]

**[Concept 2]:**
- [Explanation]

### ?? File Structure:

\```
src/Core/[Project]/
??? Folder1/
?   ??? File1.cs
?   ??? File2.cs
??? Folder2/
    ??? File3.cs
\```

---
```

---

### **Footer Section**

**Required elements:**
- Next steps v?i checklist
- Link quay l?i index

```markdown
## [N+1]. Next Steps

**Ti?p theo:** [BUILD_[X+1] - Next Module](BUILD_[X+1]_Next_Module.md)

Trong b??c ti?p theo, chúng ta s?:
1. ? [Task 1]
2. ? [Task 2]
3. ? [Task 3]

---

**Quay l?i:** [M?c l?c](BUILD_INDEX.md)
```

---

## ?? Anti-Patterns (Không nên làm)

### ? **Không tham chi?u ??n code có s?n**

**Wrong:**
```markdown
**File:** `src/Core/Application/Common/Specification/SpecificationBuilderExtensions.cs`

?? Note: File này ?ã t?n t?i trong workspace. Không c?n t?o m?i, ch? c?n hi?u cách dùng.
```

**Correct:**
```markdown
**File:** `src/Core/Application/Common/Specification/SpecificationBuilderExtensions.cs`

```csharp
// FULL CODE implementation
using System;

namespace ECO.WebApi.Application.Common.Specification;

public static class SpecificationBuilderExtensions
{
    // Full implementation here (500+ lines if needed)
}
\```
```

---

### ? **Không code tóm t?t**

**Wrong:**
```markdown
```csharp
public class Product
{
    // ... existing properties ...
    public decimal Price { get; set; }
    // ... more properties ...
}
\```
```

**Correct:**
```markdown
```csharp
using ECO.WebApi.Domain.Common.Contracts;

namespace ECO.WebApi.Domain.Catalog;

public class Product : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    
    private Product() { }
    
  public static Product Create(string name, string description, decimal price, int stock)
    {
        // Full implementation
    }
}
\```
```

---

### ? **Không gi?i thích thu?t ng? ti?ng Anh**

**Wrong:**
```markdown
Repository Pattern uses Specification Pattern for flexible queries.
```

**Correct:**
```markdown
**Repository Pattern** (M?u Kho l?u tr?) s? d?ng **Specification Pattern** (M?u ??c t?) ?? query linh ho?t.

**Specification Pattern là gì?**
- Build complex queries t? simple objects
- Reusable query logic
- Type-safe
```

---

### ? **Không có examples th?c t?**

**Wrong:**
```markdown
S? d?ng repository ?? query data.
```

**Correct:**
```markdown
**Example - Search Products:**
```csharp
// Request
var request = new SearchProductsRequest
{
    PageNumber = 1,
    PageSize = 10,
    Keyword = "iphone",
    MinPrice = 500,
    MaxPrice = 2000
};

// Specification
var spec = new ProductsBySearchSpec(request);

// Query
var products = await _repository.ListAsync(spec);
var count = await _repository.CountAsync(spec);

// Result
return new PaginatedResult(products, count);
\```
```

---

## ? Checklist khi vi?t Module Doc

### **Content Checklist**
- [ ] Header v?i link quay l?i index và prerequisites
- [ ] Section 1: Overview v?i "Làm gì", "T?i sao", Checklist
- [ ] Section 2: Add Required Packages (n?u có)
- [ ] Implementation steps có th? t? logic
- [ ] **FULL CODE** trong m?i b??c (không tóm t?t)
- [ ] Code có namespace ?úng (`ECO.WebApi.[Layer].[Module]`)
- [ ] Code có comments gi?i thích
- [ ] Gi?i thích sau m?i code block
- [ ] Examples & Usage v?i complete code
- [ ] Summary v?i checklist, diagrams, file structure
- [ ] Next Steps v?i link ??n doc ti?p theo

### **Quality Checklist**
- [ ] Code compile ???c (test tr??c khi commit)
- [ ] Namespace ?úng chu?n project
- [ ] File paths chính xác
- [ ] Commands test thành công
- [ ] JSON examples valid
- [ ] Không có typos
- [ ] Formatting nh?t quán

### **Vietnamese Language Checklist**
- [ ] Gi?i thích b?ng ti?ng Vi?t
- [ ] Thu?t ng? ti?ng Anh có gi?i thích
- [ ] Comments code d? hi?u (Vi?t ho?c Anh rõ ràng)
- [ ] Ví d? th?c t?, g?n g?i

### **Complex Module Checklist**
- [ ] Main doc focus vào usage
- [ ] Sub doc (BUILD_XX_DetailName.md) có full implementation
- [ ] Cross-reference gi?a main và sub docs
- [ ] Sub doc có ??: Overview, Full Code, Flow, Examples, Testing

### **Style Checklist**
- [ ] Emojis phù h?p (?? ?? ? ? ?? ?? ?? ??)
- [ ] Code blocks có syntax highlighting (\```csharp)
- [ ] Sections có separators (`---`)
- [ ] Lists có indentation ?úng
- [ ] Headers có hierarchy rõ (##, ###, ####)

---

## ?? Naming Convention

### **File Names**
```
BUILD_[Number]_[Module_Name].md
BUILD_[Number]_[DetailName].md (for sub docs)

Examples:
- BUILD_11_Repository_Pattern.md (main)
- BUILD_11_Specification.md (sub doc - implementation details)
- BUILD_14_Authentication.md
- BUILD_15_Authorization.md
```

### **Section Numbers**
```
1. Overview
2. Add Required Packages
3. [Major Component 1]
   3.1. [Specific Task]
   3.2. [Specific Task]
4. [Major Component 2]
   4.1. [Specific Task]
5. Usage Examples
6. Summary
7. Next Steps
```

---

## ?? Formatting Guidelines

### **Code Blocks**

**C# Code:**
```markdown
```csharp
// FULL CODE v?i namespace ??y ??
namespace ECO.WebApi.Domain.Catalog;

public class Product
{
    // Full implementation
}
\```
```

**JSON:**
```markdown
```json
{
  "key": "value",
  "nested": {
 "key": "value"
  }
}
\```
```

**Bash/PowerShell:**
```markdown
```bash
# Commands
dotnet build
dotnet run
\```

```powershell
# PowerShell commands
Get-Process | Where-Object Name -like "dotnet*"
\```
```

---

### **Tables**

```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Value 1  | Value 2  | Value 3|
| Value 4  | Value 5  | Value 6  |
```

---

### **Callouts**

```markdown
> ?? **Warning:** Critical information  
> ?? **Note:** Helpful information  
> ?? **Tip:** Pro tip  
> ?? **Prerequisites:** Required steps  
> ?? **Don't:** Anti-pattern
```

---

### **Emojis Usage**

**Standard emojis:**
- ?? Documentation/Back to index
- ?? Prerequisites/Checklist
- ? Completed/Correct/Do this
- ? Wrong/Don't do this
- ?? Warning/Important
- ?? Tip/Idea
- ?? File structure
- ?? Key points/Concepts
- ?? Diagram/Chart
- ?? Anti-pattern/Forbidden
- ?? Flow/Process
- ?? Notes/Documentation
- ?? Formatting/Style

---

### **Cross-Referencing**

**Internal Links:**
```markdown
[Link Text](BUILD_01_Solution_Setup.md)
[Specific Section](BUILD_01_Solution_Setup.md#section-anchor)
[Sub Document](BUILD_11_Specification.md)
```

**External Links:**
```markdown
[External Resource](https://docs.microsoft.com/...)
```

---

## ?? Complete Example Template

**File:** `BUILD_XX_Feature_Name.md`

```markdown
# Feature Name - Short Description

> ?? [Quay l?i M?c l?c](BUILD_INDEX.md)  
> ?? **Prerequisites:** B??c [X-1] ([Previous Module]) ?ã hoàn thành

Tài li?u này h??ng d?n xây d?ng [Feature Name] - [Purpose].

---

## 1. Overview

**Làm gì:** [Description]

**T?i sao c?n:**
- **[Reason 1]:** [Explanation]
- **[Reason 2]:** [Explanation]
- **[Reason 3]:** [Explanation]

**Trong b??c này chúng ta s?:**
- ? [Task 1]
- ? [Task 2]
- ? [Task 3]
- ? [Task 4]

**Real-world example:**
```csharp
// Usage example
\```

---

## 2. Add Required Packages

### B??c 2.1: [Package Group]

**File:** `src/[Project]/[Project].csproj`

```xml
<ItemGroup>
    <PackageReference Include="PackageName" Version="x.x.x" />
</ItemGroup>
\```

**Gi?i thích:**
- `PackageName`: [Why need this]

---

## 3. [Major Component]

### B??c 3.1: [Task Name]

**Làm gì:** [Description]

**T?i sao:** [Reason]

**File:** `src/[Project]/[Path]/[FileName].cs`

```csharp
// FULL CODE
namespace ECO.WebApi.[Namespace];

public class ClassName
{
    // Implementation
}
\```

**Gi?i thích:**
- [Explanation]

**T?i sao [Design Decision]:**
- [Reason]

**L?i ích:**
- ? [Benefit]

---

### B??c 3.2: [Next Task]

[Repeat pattern]

---

## 4. Usage Examples

### B??c 4.1: Complete Example - [Feature]

**Request:**
```csharp
// DTOs
\```

**Handler:**
```csharp
// Implementation
\```

**API Call:**
```bash
curl -X POST https://localhost:7001/api/endpoint
\```

**Response:**
```json
{ "success": true }
\```

---

## 5. Summary

### ? ?ã hoàn thành:

**[Category]:**
- ? [Item]

### ?? Architecture:

\```
[Diagram]
\```

### ?? Key Concepts:

**[Concept]:**
- [Point]

### ?? File Structure:

\```
src/
??? [Structure]
\```

---

## 6. Next Steps

**Ti?p theo:** [BUILD_[X+1] - Next Module](BUILD_[X+1]_Next_Module.md)

Trong b??c ti?p theo:
1. ? [Task 1]
2. ? [Task 2]

---

**Quay l?i:** [M?c l?c](BUILD_INDEX.md)
```

---

## ?? Final Notes

### **Khi vi?t docs, hãy nh?:**

1. **Self-Contained:** Code ??y ??, không reference workspace
2. **Ti?ng Vi?t:** Gi?i thích d? hi?u, thu?t ng? có gi?i thích
3. **Chia Nh?:** Main doc (usage) + Sub doc (implementation) n?u ph?c t?p
4. **Examples:** Ví d? th?c t?, complete code
5. **Quality:** Code compile, paths ?úng, formatting nh?t quán

### **M?c tiêu cu?i cùng:**

> B?t k? developer nào ??c docs này ??u có th? T?O L?I toàn b? solution  
> t? con s? 0, ch? c?n follow t?ng b??c trong docs.

---

**S? d?ng template này ?? vi?t documentation nh?t quán và ch?t l??ng cao cho ECO.WebApi!** ??
