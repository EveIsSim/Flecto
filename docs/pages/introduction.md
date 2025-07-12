# Introduction

Flecto is a fluent, type-safe **SQL SELECT query generator and filtering layer for .NET 9+**, 
focusing on structured filtering, sorting, and pagination at the moment. 
While designed for seamless use with Dapper, Flecto is **provider-agnostic and extensible**, 
allowing you to integrate it with **other data access providers and SQL dialects** 
as project evolves.

## Why Flecto?
Manual SQL string construction for dynamic reads is error-prone and hard to maintain. 
Flecto enables you to **safely and clearly build dynamic SELECT queries** with structured filters 
and sorting while ensuring safe parameterization and testable filtering logic, 
reducing repetitive boilerplate in your services.

## Who is Flecto for?
- Developers building HTTP APIs and services needing dynamic, structured, type-safe querying.
- Teams seeking to eliminate manual SQL construction for dynamic SELECTs while retaining control over query generation.
- Applications requiring validated, structured filtering and sorting in a clean, maintainable way.
- Systems that may evolve to support multiple database providers and SQL dialects.

## Key Features
- **Fluent API for SELECT Queries**: Compose structured read queries using `Select()`, `BindNumeric()`, `BindString()`, `Search()`, and `ApplyPaging()`.
- **Structured Filtering**: Strongly-typed filters (`NumericFilter`, `StringFilter`, `BoolFilter`, etc.) with validation and extensibility.
- **Safe Parameterization**: Manages SQL parameters automatically to prevent injection vulnerabilities.
- **Sorting and Pagination**: Type-safe, declarative sorting and paging.
- **Provider and Dialect Extensibility**: Built-in support for PostgreSQL with an extensible architecture for SQL Server, MySQL, and other dialects/providers.
- **Cloneable Queries**: Easily adjust and re-use queries with `.Clone()`.
- **Focused on SELECT**: Purpose-built for safe, dynamic read queries in HTTP APIs and services.

## Filters
Flecto uses structured, type-safe filters that support validation and null checks, 
enabling clean, maintainable filtering before executing read queries.

---

## Next Steps
- [Filters](filters.md): Defining and applying structured filters.
- [Validations](validations.md): Integrating and extending validation.
- [Bindings](bindings.md): Using filters with HTTP and JSON binding.
- [Usages](usages.md): Practical examples with Dapper and other providers.
- [Roadmap](roadmap.md): Planned enhancements for Flecto.
