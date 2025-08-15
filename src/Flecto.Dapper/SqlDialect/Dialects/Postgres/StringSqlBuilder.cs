using Flecto.Dapper.Constants;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Constants;
using Flecto.Dapper.SqlDialect.Enums;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;

/// <summary>
/// Provides helper methods for building SQL conditions for string-based filters,
/// including equality, inequality, pattern matching, and IN/NOT IN array checks,
/// supporting case sensitivity as needed.
/// </summary>
internal static class StringSqlBuilder
{
    /// <summary>
    /// Builds a SQL condition for checking if a string column's value is in the specified input array,
    /// using PostgreSQL's <c>ANY</c> syntax for efficient matching.
    /// Converts all input strings to lowercase if <paramref name="caseSensitive"/> is false.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL query.</param>
    /// <param name="input">The array of string values to check against.</param>
    /// <param name="caseSensitive">Whether the comparison should be case-sensitive.</param>
    /// <returns>A tuple containing the SQL condition and the processed array of parameter values.</returns>
    internal static (string SqlCondition, string[] paramValues) BuildInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive)
    {
        if (caseSensitive)
        {
            return ($"{column} {SqlOps.Eq} {PgSqlOps.ANY}(@{paramName})", input);
        }

        var lowered = input.Select(static x => x.ToLowerInvariant()).ToArray();
        return ($"{Sql.LOWER}({column}) {SqlOps.Eq} {PgSqlOps.ANY}(@{paramName})", lowered);
    }

    /// <summary>
    /// Builds a SQL condition for checking if a string column's value is not in the specified input array,
    /// using PostgreSQL's <c>ANY</c> syntax for efficient matching.
    /// Converts all input strings to lowercase if <paramref name="caseSensitive"/> is false.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL query.</param>
    /// <param name="input">The array of string values to check against.</param>
    /// <param name="caseSensitive">Whether the comparison should be case-sensitive.</param>
    /// <returns>A tuple containing the SQL condition and the processed array of parameter values.</returns>
    internal static (string SqlCondition, string[] paramValues) BuildNotInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive)
    {
        if (caseSensitive)
        {
            return ($"{column} {SqlOps.NotEq} {PgSqlOps.ALL}(@{paramName})", input);
        }

        var lowered = input.Select(static x => x.ToLowerInvariant()).ToArray();
        return ($"{Sql.LOWER}({column}) {SqlOps.NotEq} {PgSqlOps.ALL}(@{paramName})", lowered);
    }

    /// <summary>
    /// Builds a SQL condition for checking string equality on a column, using either <c>=</c> 
    /// or <c>ILIKE</c> depending on case sensitivity.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL query.</param>
    /// <param name="caseSensitive">Whether the comparison should be case-sensitive.</param>
    /// <returns>SQL condition</returns>
    internal static string BuildEquals(
        string column,
        string paramName,
        bool caseSensitive)
    {
        return caseSensitive
            ? $"{column} {SqlOps.Eq} @{paramName}"
            : $"{column} {PgSql.ILIKE} @{paramName}";
    }

    /// <summary>
    /// Builds a SQL condition for checking string inequality on a column, using either <c>!=</c> 
    /// or <c>NOT ILIKE</c> depending on case sensitivity.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL query.</param>
    /// <param name="caseSensitive">Whether the comparison should be case-sensitive.</param>
    /// <returns>SQL condition</returns>
    internal static string BuildNotEquals(string column, string paramName, bool caseSensitive)
    => caseSensitive
        ? $"{column} {SqlOps.NotEq} @{paramName}"
        : $"{column} {PgSql.NOT_ILIKE} @{paramName}";

    /// <summary>
    /// Builds a SQL LIKE-based condition for matching a string column using the specified 
    /// <see cref="StringMatchType"/> and case sensitivity.
    /// Supports:
    /// <list type="bullet">
    /// <item><see cref="StringMatchType.Contains"/> - generates a pattern of <c>%value%</c>.</item>
    /// <item><see cref="StringMatchType.StartsWith"/> - generates a pattern of <c>value%</c>.</item>
    /// <item><see cref="StringMatchType.EndsWith"/> - generates a pattern of <c>%value</c>.</item>
    /// </list>
    /// Uses <c>LIKE</c> or <c>ILIKE</c> depending on <paramref name="caseSensitive"/>.
    /// </summary>
    /// <param name="column">The column name to apply the pattern match against.</param>
    /// <param name="paramName">The parameter name to use in the SQL query.</param>
    /// <param name="input">The string value to match.</param>
    /// <param name="matchType">The pattern matching type to apply.</param>
    /// <param name="caseSensitive">Whether the pattern matching should be case-sensitive.</param>
    /// <returns>A tuple containing the SQL condition and the processed pattern value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported <paramref name="matchType"/> is provided.</exception>
    internal static (string SqlCondition, string ParamValue) BuildLike(
        string column,
        string paramName,
        string input,
        StringMatchType matchType,
        bool caseSensitive)
    {
        var op = CommonSqlBuilder.LikeOperator(caseSensitive);

        var val = matchType switch
        {
            StringMatchType.Contains => $"%{input}%",
            StringMatchType.StartsWith => $"{input}%",
            StringMatchType.EndsWith => $"%{input}",
            _ => throw new ArgumentOutOfRangeException(nameof(matchType), matchType, null)
        };

        return ($"{column} {op} @{paramName}", val);
    }
}
