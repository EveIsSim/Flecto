# Filters

Flecto uses structured, type-safe filters to build clean, safe, and dynamic SQL SELECT queries 
across providers. Filters ensure your read operations are clear and maintainable while preventing 
SQL injection and supporting validation before executing queries.

---

## Why use filters?
- Eliminate manual SQL WHERE clause construction.
- Apply dynamic filters safely without boilerplate.
- Enable validated, composable filtering for APIs and services.
- Support clear API contracts and frontend filter builders.

---

## Available Filters

### `BoolFilter`
Used for filtering boolean columns.

**Key properties:**

- `Eq`: filter by true/false.
- `Null`: filter for null values.
- `Sort`: sorting direction.

**Example:**
```csharp
var filter = new BoolFilter { Eq = true };
```

---

### `DateFilter`
Used for filtering date columns with exact match or ranges.

**Key properties:**

- `Eq` / `NotEq`: exact date match or exclusion.
- `Gt` / `Gte`, `Lt` / `Lte`: range filtering.
- `In` / `NotIn`: inclusion/exclusion by list.
- `Null`: filter for null values.
- `Sort`: sorting direction.

**Example:**
```csharp
var filter = new DateFilter 
{ 
    Gt = new DateTime(1970, 01, 01), 
    Lt = new DateTime(2000, 01, 01) 
};
```

---

### `EnumFilter<T>`
Used for filtering enum columns by name or numeric value.

**Key properties:**

- `Eq` / `NotEq`: exact enum match or exclusion.
- `In` / `NotIn`: inclusion/exclusion by list.
- `Null`: filter for null values.
- `EnumFilterMode`: mode used for filtering enumeration values (Name, Value, ValueString)
- `Sort`: sorting direction.

**Example:**
```csharp
var filter = new EnumFilter<UserStatus> { Eq = UserStatus.Active };
```

---

### `FlagsEnumFilter<T>`

Used for filtering columns that store flags-style enums using bitwise operations, 
enabling type-safe filtering for flags enums.

**Key properties:**

- `Eq`: filter for exact enum value.
- `NotEq`: filter for inequality with the specified enum value.
- `HasFlag`: filter where the specified flag is set.
- `NotHasFlag`: filter where the specified flag is not set.
- `Null`: filter for null values.
- `Sort`: sorting direction.

**Example:**

```csharp
var filter = new FlagsEnumFilter<UserPermissions> { HasFlag = UserPermissions.Admin };
```

---

### `GuidFilter`

Used for filtering `Guid` columns with support for exact matching, inclusion/exclusion, null checks, and sorting.

**Key properties:**

- `Eq`: filter for exact `Guid` value.
- `NotEq`: filter for inequality with the specified `Guid`.
- `In`: filter for inclusion within a list of `Guid` values.
- `NotIn`: filter for exclusion from a list of `Guid` values.
- `Null`: filter for null values.
- `Sort`: sorting direction.

**Example:**

```csharp
var filter = new GuidFilter
{
    NotIn = new Guid[]
    {
        Guid.Parse("5a1d2b3c-6f4e-4d7b-9c2f-8b5c4e7d9f10"),
        Guid.Parse("3f4b5a6c-2e1d-4c3b-8f7e-5d6c7b8a9e01")
    },
    Sort = Sort.Desc
};
```

---

### `NumericFilter<T>`
Used for filtering numeric columns with full support for comparison operations, 
inclusion/exclusion, null checking, and sorting.

**Key properties:**

- `Eq` / `NotEq`: exact equality or inequality.
- `Gt` / `Gte`: greater than, greater than or equal.
- `Lt` / `Lte`: less than, less than or equal.
- `In` / `NotIn`: inclusion or exclusion by list.
- `Null`: filter for null values.
- `Sort`: sorting direction.

**Support types**

- `short, int, long, decimal, double, float`

**Example:**
```csharp
var filter = new NumericFilter<int> 
{ 
    Gte = 18, 
    Lt = 65, 
    Sort = { Position = 1, Descending = false} 
};
```

---

### `PaginationFilter`
Used user-friendly filter for controlling pagination in your queries, 
allowing clients to request data in manageable chunks.

**Key properties:**

- `Limit`: maximum number of items to return.
- `Page`: page number to retrieve.

**Example:**
```csharp
var filter = new PaginationFilter
{
    Limit = 50,
    Page = 2
};

```

---

### `SearchFilter`

Used for performing free-text search across specific columns in your queries, 
supporting OR-based matching or integration with `tsvector` for PostgreSQL full-text search.

**Key properties:**

- `Value`: the search string to match.
- `CaseSensitive`: determines if the search should be case-sensitive.

**Example:**

```csharp
var filter = new SearchFilter
{
    Value = "project manager",
    CaseSensitive = false
};
```

---

### `Sort`

`Sort` is used across all filters in Flecto to define sorting behavior on query properties, 
supporting multi-column and user-defined sorting orders.

**Key properties:**

- `Position`: defines the sort priority among multiple sorted columns.
- `Descending`: determines if the sort direction is descending (`true`) or ascending (`false`).

**Example:**

```csharp
var sort = new Sort(position: 1, descending: true);
```

---

### `StringFilter`
Used for filtering string columns with pattern matching.

**Key properties:**

- `Eq` / `NotEq`: exact match or exclusion.
- `Contains`, `StartsWith`, `EndsWith`: pattern searches.
- `In` / `NotIn`: inclusion/exclusion by list.
- `Null`: filter for null values.
- `Sort`: sorting direction.
- `CaseSensitive`: case-insensitive comparison.

**Example:**
```csharp
var filter = new StringFilter { Contains = "admin", CaseSensitive = true };
```

---

## Usage Example
Filters in Flecto are typically used together with FlectoBuilder to dynamically construct SQL SELECT
Details about FlectoBuilder will be covered in the next sections.

```csharp
var (sqlCount, parametersCount) = new FlectoBuilder("table_name", DialectType.Postgres)
    .BindNumeric("age", filter.Age)
    .BindString("name", filter.Name)
    .ApplyPaging(request.Page, request.PageSize)
    .Build();
```

Filters in Flecto help keep your query layer clean, safe, and adaptable while supporting structured dynamic query building.

