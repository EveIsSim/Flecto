## Bindings

Flecto's binding mechanism allows you to fluently connect filters to specific database columns, 
forming the foundation of a dynamic and type-safe query.

This is done using the `FlectoBuilder` class and its fluent methods such as 
`BindNumeric`, `BindString`, `BindBool`, `Search`, and others. 
Each method binds a provided filter instance to one or more SQL column names, 
safely translating user inputs into parameterized SQL expressions.

### Basic Usage

```csharp
var builder = new FlectoBuilder("employee", DialectType.Postgres)
    .Search(r.Search, "first_name", "last_name", "middle_name", "notes")
    .BindNumeric(r.Id, "id")
    .BindString(r.Name, "first_name")
    .BindNumeric(r.Salary, "salary");

var (sqlCount, parametersCount) = builder
    .Clone()
    .SelectCount()
    .Build();

var totalRecords = await connection.QueryFirstAsync<int>(sqlCount, parametersCount);

var (sql, parameters) = builder
    .SelectAll()
    .ApplyPaging(r.Paging)
    .Build();

var employees = await connection.QueryAsync<Employee>(sql, parameters);
```

### Key Binding Methods

- `BindNumeric(filter, column)` — binds a `NumericFilter<T>` to a column.
- `BindString(filter, column)` — binds a `StringFilter`.
- `BindBool(filter, column)` — binds a `BoolFilter`.
- `BindGuid(filter, column)` — binds a `GuidFilter`.
- `BindDate(filter, column)` — binds a `DateFilter`.
- `BindEnum(filter, column)` — binds an `EnumFilter<T>`.
- `BindFlagsEnum(filter, column)` — binds a `[Flags]` enum filter.
- `Search(searchFilter, columns...)` — applies a search filter using `ILIKE` or full-text search depending on dialect.

All binding methods automatically:

- Perform null-checks and skip filters not provided;
- Build every column with table prefix: `table.column`
- Safely parameterize SQL values to prevent SQL injection.
- Validate filters internally (via `EnsureValid`) and validate table and column(s) naming;

---

### Query Composition Notes

- Currently supports only **single-table** queries;
- All `Bind*` and `Search` methods are translated into the `WHERE` clause using logical **AND**;
- Multiple `Bind*` calls will append conditions conjunctively;
- Selection options include:
  - `SelectAll()` — SELECT \*
  - `SelectCount()` — SELECT COUNT(\*) with ORDER BY skipped
  - `Select(columns)` — SELECT specific columns
  - not allow manual select logic at the moment

---

### Optional Clone and Count

You can safely clone a builder mid-way and use it to perform a `COUNT(*)` query:

```csharp
var (sqlCount, parametersCount) = builder.Clone().SelectCount().Build();
```

This helps compute total pagination values before fetching the actual page.

---

💡 Want to return paginated results with metadata?

→ Check out Usages to learn how to wrap Flecto queries using `SearchResult<T>` and return structured responses.

