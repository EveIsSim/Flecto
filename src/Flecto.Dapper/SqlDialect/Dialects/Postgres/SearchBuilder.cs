using Flecto.Core.Models.Filters;
using Flecto.Dapper.Constants;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;

/// <summary>
/// Provides helper methods for building SQL search conditions, including simple LIKE-based search
/// and full-text search using PostgreSQL <c>tsvector</c> mechanisms.
/// </summary>
internal static class SearchBuilder
{

    /// <summary>
    /// Builds a SQL condition for performing a simple LIKE-based search across the specified tables and columns.
    /// Constructs a condition where each column is compared using the LIKE operator with the provided parameter,
    /// combined with an OR across all columns and tables.
    /// 
    /// Example generated SQL:
    /// <code>
    /// (table1.col1 ILIKE @param OR table1.col2 ILIKE @param OR table2.col1 ILIKE @param)
    /// </code>
    /// 
    /// The parameter value will be wrapped with '%' for partial matching (e.g., <c>%value%</c>).
    /// </summary>
    /// <param name="filter">The <see cref="SearchFilter"/> containing the search value and case sensitivity flag.</param>
    /// <param name="paramName">The parameter name to use in the SQL condition.</param>
    /// <param name="tablesWithColumns">The tables and columns to include in the search.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><c>SqlCondition</c> - The generated SQL condition string.</item>
    /// <item><c>ParamValue</c> - The parameter value wrapped with '%' for partial matching.</item>
    /// </list>
    /// </returns>
    internal static (string SqlCondition, string ParamValue) Build(
        SearchFilter filter,
        string paramName,
        (string Table, string[] Columns)[] tablesWithColumns)
    {
        var conditions = new List<string>();
        var op = CommonSqlBuilder.LikeOperator(filter.CaseSensitive);

        foreach (var (table, columns) in tablesWithColumns)
        {
            foreach (var column in columns)
            {
                var condition = $"{table}.{column} {op} @{paramName}";

                conditions.Add(condition);
            }
        }

        var c = $"({string.Join($" {Sql.OR} ", conditions)})";
        var v = $"%{filter.Value}%";

        return (c, v);
    }

    /// <summary>
    /// Builds a SQL condition for performing full-text search using PostgreSQL's <c>tsvector</c> and <c>tsquery</c> mechanisms.
    /// 
    /// The <paramref name="config"/> parameter specifies the text search configuration (e.g., "simple", "english", "russian"),
    /// which determines the dictionary and parsing rules used during tokenization.
    /// 
    /// The <paramref name="mode"/> parameter specifies the type of full-text search to perform:
    /// <list type="bullet">
    /// <item><see cref="TextSearchMode.Plain"/> - uses <c>plainto_tsquery</c> for parsing plain text.</item>
    /// <item><see cref="TextSearchMode.WebStyle"/> - uses <c>websearch_to_tsquery</c> for search engine-like parsing.</item>
    /// </list>
    /// 
    /// Generates SQL of the form:
    /// <code>
    /// to_tsvector('config', coalesce(table1.col1, '') || ' ' || coalesce(table1.col2, '')) @@ plainto_tsquery('config', @param)
    /// </code>
    /// </summary>
    /// <param name="paramName">The parameter name to use for the search term in the SQL query.</param>
    /// <param name="mode">The type of full-text search to perform.</param>
    /// <param name="config">The PostgreSQL text search configuration (e.g., "simple").</param>
    /// <param name="tablesWithColumns">The tables and columns to include in the full-text search vector.</param>
    /// <returns>The SQL condition string for the full-text search.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported <paramref name="mode"/> is provided.</exception>
    internal static string BuildTsVectorSearchCondition(
        string paramName,
        TextSearchMode mode,
        string config,
        (string Table, string[] Columns)[] tablesWithColumns)
    {
        var columns = tablesWithColumns
            .SelectMany(tc => tc.Columns
                .Select(c => $"{Sql.COALESCE}({tc.Table}.{c}, '')"));

        var concatenatedColumns = string.Join(" || ' ' || ", columns);
        var tsvector = $"to_tsvector('{config}', {concatenatedColumns})";

        var tsquery = mode switch
        {
            TextSearchMode.Plain => $"plainto_tsquery('{config}', @{paramName})",
            TextSearchMode.WebStyle => $"websearch_to_tsquery('{config}', @{paramName})",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported search mode")
        };

        return $"{tsvector} @@ {tsquery}";
    }
}
