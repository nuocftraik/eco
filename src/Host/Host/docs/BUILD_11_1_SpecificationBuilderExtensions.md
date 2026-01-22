# SpecificationBuilderExtensions — Giải thích chi tiết từng hàm

File này tóm lược ngắn gọn, kèm giải thích comment-in-code (đã có trong source). Dùng khi bạn cần hiểu nhanh logic hoặc debug payload.

---

## 0) Tổng quan
- Đây là bộ extension methods cho Ardalis.Specification:
  - `SearchBy` = Keyword search + AdvancedSearch + AdvancedFilter
  - `PaginateBy` = Skip/Take + OrderBy
  - `OrderBy` = Parse mảng orderBy
  - Helpers: build Expression Tree cho search/filter/order, support nested property.

Payload gợi ý (đầy đủ): xem phần cuối.

---

## 1) SearchBy
**Mục đích:** Gộp tất cả bước search/filter từ `BaseFilter`.
- Gọi tuần tự:
  1) `SearchByKeyword(keyword)`
  2) `AdvancedSearch(advancedSearch)`
  3) `AdvancedFilter(advancedFilter)`
- Trả về builder để tiếp tục chain.

Khi dùng: `Query.SearchBy(request);`

---

## 2) PaginateBy
**Mục đích:** Áp pagination và sắp xếp.
- Chuẩn hóa `PageNumber` (mặc định 1) và `PageSize` (mặc định 10 nếu <=0).
- Tính `Skip` nếu page > 1.
- Gọi `Take(PageSize)` và `OrderBy(orderByFields)`.

Khi dùng: `Query.PaginateBy(request);`

---

## 3) SearchByKeyword
**Mục đích:** Search nhanh bằng keyword trên tất cả primitive fields (first-level).
- Thực chất là wrapper gọi `AdvancedSearch` với `Search { Keyword = keyword }`.

Khi dùng: `Query.SearchByKeyword(request.Keyword);`

---

## 4) AdvancedSearch
**Mục đích:** Search nâng cao, có thể chỉ định fields hoặc search tất cả primitive fields.
- Nếu `search.Fields` có giá trị → chỉ search các field đó (support nested `Category.Name`).
- Nếu không có `Fields` → search tất cả primitive fields (string, number, bool, datetime, …; bỏ enum/object).
- Mỗi field được chuyển thành search criteria (LIKE): mặc định `contains` (`%keyword%`), convert to-lower để không phân biệt hoa/thường.

Khi dùng: `Query.AdvancedSearch(request.AdvancedSearch);`

---

## 5) AddSearchPropertyByKeyword (helper)
**Mục đích:** Build search criteria cho một property.
- Xác định kiểu property:
  - String: dùng trực tiếp.
  - Non-string: ToString() + null-check.
- Tạo pattern: `contains`/`startswith`/`endswith` → `"%keyword%"`, `"keyword%"`, `"%keyword"`.
- Thêm `SearchExpressionInfo` vào `Specification.SearchCriterias`.

---

## 6) AdvancedFilter
**Mục đích:** Filter phức tạp với logic AND/OR/XOR và nhiều operators.
- Nếu `filter.Logic` có (and/or/xor):
  - Bắt buộc có `Filters` con → đệ quy build expression.
- Nếu không có `Logic`:
  - Dùng `Field + Operator + Value` build expression đơn.
- Hỗ trợ nested field `Category.Name`.
- Operators: `eq, neq, lt, lte, gt, gte, contains, startswith, endswith`.
- Enum/Guid/DateTime/String được parse an toàn (sai format → CustomException).

Khi dùng: `Query.AdvancedFilter(request.AdvancedFilter);`

---

## 7) CreateFilterExpression (3 overloads) & CombineFilter
**Mục đích:** Core build binary expressions.
- Overload 1: từ Logic + Filters (đệ quy).
- Overload 2: từ Field + Operator + Value → build MemberExpression + ConstantExpression.
- Overload 3: từ MemberExpression + ConstantExpression → switch theo operator.
- `CombineFilter`: kết hợp 2 expression bằng `And/Or/Xor`.

Khi debug: kiểm tra `field`, `operator`, format `value`.

---

## 8) GetPropertyExpression (helper)
**Mục đích:** Build MemberExpression từ chuỗi field (support nested).
- `"Category.Name"` → `x => x.Category.Name`.
- Dùng trong search/filter/order.

---

## 9) GeValuetExpression & ChangeType (helpers)
**Mục đích:** Chuyển value (thường là JsonElement) thành ConstantExpression đúng type.
- Enum: parse string → enum (ignore case).
- Guid: parse string → Guid.
- DateTime: parse string.
- String: lấy trực tiếp.
- Nullable: xử lý qua `ChangeType`.
- Sai format → CustomException.

---

## 10) OrderBy & ParseOrderBy
**Mục đích:** Apply ordering từ mảng string.
- Format: `"Name"`, `"Price Desc"`, `"Category.Name"`.
- Phần tử đầu → `OrderBy/OrderByDescending`; các phần tử tiếp → `ThenBy/ThenByDescending`.
- Hỗ trợ nested field.

Khi dùng: `Query.OrderBy(request.OrderBy);` (được gọi bên trong `PaginateBy`).

---

## 11) Payload mẫu (đầy đủ)
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "orderBy": ["Name asc", "CreatedOn desc"],
  "keyword": "iphone",
  "advancedSearch": {
    "fields": ["Name", "Code", "Description"],
    "keyword": "pro"
  },
  "advancedFilter": {
    "logic": "and",
    "filters": [
      { "field": "Price", "operator": "gte", "value": 1000 },
      { "field": "Price", "operator": "lte", "value": 3000 },
      { "field": "Status", "operator": "eq", "value": "Active" },
      {
        "logic": "or",
        "filters": [
          { "field": "Category.Name", "operator": "contains", "value": "phone" },
          { "field": "Category.Name", "operator": "contains", "value": "tablet" }
        ]
      }
    ]
  }
}
```
**Luồng:**
1) `SearchBy`: keyword + advancedSearch + advancedFilter → build Where & Search expressions.
2) `PaginateBy`: Skip/Take + OrderBy.
3) Repo `ListAsync/CountAsync` → EF Core sinh SQL đầy đủ.

---

## 12) Khi debug
- Nếu không filter được: kiểm tra `field` có đúng tên (và nested) không.
- Nếu lỗi parse: kiểm tra `value` có đúng format cho Enum/Guid/DateTime không.
- Nếu không order được: kiểm tra chuỗi `orderBy` có “Desc” chuẩn không; nested field có tồn tại không.

---

*File này chỉ để giải thích nhanh; code đã được comment chi tiết trong `SpecificationBuilderExtensions.cs`.*
