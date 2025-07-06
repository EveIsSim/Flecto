using EveIsSim.QueryBuilder.Dapper.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;


internal static class StringSqlBuilder
{
    internal static (string SqlCondition, string[] paramValues) BuildInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive)
    {
        if (input is null || input.Length == 0)
            throw new ArgumentException("value should not be empty");

        var values = caseSensitive
            ? input
            : input.Select(x => x.ToLowerInvariant()).ToArray();

        var sqlCondition = $"{column} {SqlOps.Eq} {PgSqlOps.ANY}(@{paramName})";

        return (sqlCondition, values);

    }

    internal static (string SqlCondition, string[] paramValues) BuildNotInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive)
    {
        if (input is null || input.Length == 0)
            throw new ArgumentException("value should not be empty");

        var values = caseSensitive
            ? input
            : input.Select(x => x.ToLowerInvariant()).ToArray();

        var sqlCondition = $"{column} {SqlOps.NotEq} {PgSqlOps.ANY}(@{paramName})";

        return (sqlCondition, values);
    }


    internal static (string SqlCondition, string ParamValue) BuildEquals(
        string column,
        string paramName,
        string input,
        bool caseSensitive)
    {
        var val = caseSensitive ? input : input.ToLowerInvariant();
        var sqlCondition = caseSensitive
            ? $"{column} {SqlOps.Eq} @{paramName}"
            : $"{column} {PgSql.ILIKE} @{paramName}";

        return (sqlCondition, val);
    }

    internal static (string SqlCondition, string ParamValue) BuildNotEquals(
        string column,
        string paramName,
        string input,
        bool caseSensitive)
    {
        var val = caseSensitive ? input : input.ToLowerInvariant();
        var sqlCondition = caseSensitive
            ? $"{column} {SqlOps.NotEq} @{paramName}"
            : $"{column} {PgSql.NOT_ILIKE} @{paramName}";

        return (sqlCondition, val);
    }

    internal static (string SqlCondition, string ParamValue) BuildLike(
        string column,
        string paramName,
        string input,
        StringMatchType matchType,
        bool caseSensitive)
    {
        var op = CommonSqlBuilder.LikeOperator(caseSensitive);
        var adoptInput = caseSensitive ? input : input.ToLowerInvariant();

        var val = matchType switch
        {
            StringMatchType.Contains => $"%{adoptInput}%",
            StringMatchType.StartsWith => $"{adoptInput}%",
            StringMatchType.EndsWith => $"%{adoptInput}",
            _ => throw new ArgumentOutOfRangeException(nameof(matchType), matchType, null)
        };

        return ($"{column} {op} @{paramName}", val);
    }
}
