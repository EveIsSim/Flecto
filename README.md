# Flecto

Fluent, safe, and flexible SQL SELECT query builder for .NET (starting with .NET 9+) with structured filters, sorting, and pagination, designed for use with Dapper and extensible to other providers and sql dialects.

## Why Flecto?

Stop building SQL manually. Flecto lets you dynamically and safely construct SQL queries in .NET using structured filters, sorting, and pagination. It automatically manages parameters, prevents SQL injection, and keeps your query logic clean and maintainable, whether used in HTTP APIs or internal services.

## Who is this for?
* Building Read APIs with structured, dynamic queries.
* Using without manual SQL and parameter management.
* Services that need clean, type-safe filtering and validation before querying.
* Admin panels and backend endpoints requiring fast, reliable data retrieval.

## Key Features

- **Fluent API**: Build queries clearly using `Select()`, `Search()`, `BindNumeric()`, `BindString()`, and `ApplyPaging()`.
- **Automatic Filtering**: Filters are applied basic logical validations only if provided, and supporting to adding custom validators.
- **Safe Parameterization**: Prevents SQL injection automatically.
- **Validation**: Built-in validation, custom validators, and you can apply it in FluentValidation.
- **Sorting and Pagination**: Integrated and type-safe.
- **Multiple SQL Dialects**: PostgreSQL supported, extensible to SQL Server/MySQL.
- **Cloneable Queries**: Create variations easily with `.Clone()`.
- **HTTP and Internal Services Ready**: Use with APIs or background/CLI services.

## Filters

Use strongly-typed filters (`NumericFilter`, `StringFilter`, `BoolFilter`, etc.) for safe, composable filtering with sorting and null checks.

## Getting Started

### Installation

```bash
dotnet add package Flecto
dotnet add package Flecto.Dapper
```

### Basic HTTP Usage with SearchMetadata

```csharp
public class Request
{
    public SearchFilter? Search { get; set; }
    public NumericFilter<int>? Id { get; set; }
    public StringFilter? Name { get; set; }
    public PaginationFilter? Paging { get; set; }
}

public async Task<SearchResult<Employee[]>> Search(Request request, CancellationToken token)
{
    var builder = new FlectoBuilder("employees", DialectType.Postgres)
        .Search(request.Search, "first_name", "last_name")
        .BindNumeric(request.Id, "id")
        .BindString(request.Name, "first_name");

    var (countSql, countParams) = builder
        .SelectCount()
        .Build();
    var totalRecords = await connection.QueryFirstAsync<int>(countSql, countParams);

    var (sql, parameters) = builder
        .Clone()
        .Select("id", "first_name", "last_name")
        .ApplyPaging(request.Paging)
        .Build();

    var employees = await connection.QueryAsync<Employee>(sql, parameters);

    return new SearchResult<Employee[]>(employees.ToArray(), SearchMetadata.From(totalRecords, request.Paging));
}
```

This pattern provides a **ready-to-use response with pagination on the frontend.**

### Using in Background Services

```csharp
var filter = new NumericFilter<decimal> { Gte = 5000m };
var builder = new FlectoBuilder("employees", DialectType.Postgres)
    .BindNumeric(filter, "salary");

var (sql, parameters) = builder.Build();
var employees = await connection.QueryAsync<Employee>(sql, parameters);
```

### Validation Example

```csharp
var errors = StringValidator.Validate(nameFilter, maxLength: 100, customValidator: value =>
{
    if (value.Contains("badword")) return (false, "Contains forbidden words.");
    return (true, null);
});
```

Or use FluentValidation to validate DTOs before using FlectoBuilder.

## Roadmap

* JOIN and ON support for building related queries.
* GET request support with hybrid sorting (sort=field,-createdAt).
* DISTINCT support.
* support system extended filters:
    var builder = new FlectoBuilder("employees", DialectType.Postgres)
    .BindNumeric(request.Id, "id")
    .BindString(request.Name, "first_name")
    .AndWhere("is_deleted = false") // системный фильтр
    .GroupBy("department_id")
    .Having("SUM(salary) > @minSalary", new { minSalary = 10000 });
* ? GROUP BY and HAVING support.
    * * Aggregate functions support (Sum, Count, Max, Min).
* ? Subquery support.
* ? Advanced usage: OR, AND, NOT grouping in WHERE clauses.
* ? pretty-print for SQL debugging.

## Conclusion

Flecto provides a lightweight, powerful way to build clear, safe SQL queries in .NET. With filters, search, sorting, pagination, and validation, you can generate precise queries easily for APIs and internal services without manual SQL handling.

