# Flecto.Dapper

**Flecto.Dapper** is a SQL query builder and adapter for [Dapper](https://github.com/DapperLib/Dapper), built on top of [Flecto.Core](https://www.nuget.org/packages/Flecto.Core).

It takes strongly-typed filter, sorting, and pagination models from `Flecto.Core` and translates them into valid SQL queries with parameters.

🔗 See usage examples and implementation details on [GitHub Pages](https://flecto-labs.github.io/Flecto/).

---

## Features

- Build flexible and composable SQL queries using filter, sort, and paging models
- Support text search and tsvector
- Safe parameter binding for use with Dapper
- Support only Postgres dialect at the moment. Other dialects will add base on priority. 

---

## License

This project is licensed under the MIT License.
