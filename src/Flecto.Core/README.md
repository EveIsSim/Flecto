# Flecto.Core

**Flecto.Core** provides core abstractions and utilities for building composable and validated query logic in .NET applications.  
It includes filter objects, sorting models, pagination, and input validation. This library is designed to be adapter-agnostic and does not generate SQL directly.

> SQL generation is handled by adapter packages like [Flecto.Dapper](https://www.nuget.org/packages/Flecto.Dapper).

---

## Features

- Strongly typed filter models (`StringFilter`, `DateFilter`, `NumericFilter`, etc.)
- Composable pagination and sorting primitives
- Input validation for filters, table and column names
- JSONB field support for column-level expressions
- Designed to be extended via adapter libraries (e.g., Dapper, EF Core)

---

## Usage

This package is intended to be consumed by libraries that handle SQL or other query generation.  
See [Flecto.Dapper](https://www.nuget.org/packages/Flecto.Dapper) for a full example of how to integrate Flecto.Core with Dapper-based data access.

🔗 You can explore usage examples and implementation details on [GitHub Pages](https://flecto-labs.github.io/Flecto/).

---

## License

This project is licensed under the MIT License.
