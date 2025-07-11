using Flecto.Dapper.Constants;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Constants;
using Flecto.Dapper.SqlDialect.Enums;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;


internal static class CommonSqlBuilder
{
    internal static string BuildNumericComparison(string column, string paramName, ComparisonOperator op)
    {
        var opStr = op switch
        {
            ComparisonOperator.Eq => SqlOps.Eq,
            ComparisonOperator.NotEq => SqlOps.NotEq,
            ComparisonOperator.Gt => SqlOps.Gt,
            ComparisonOperator.Gte => SqlOps.Gte,
            ComparisonOperator.Lt => SqlOps.Lt,
            ComparisonOperator.Lte => SqlOps.Lte,
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{column} {opStr} @{paramName}";
    }

    internal static string BuildInArray(string column, string paramName)
    => $"{column} {SqlOps.Eq} {PgSqlOps.ANY}(@{paramName})";

    internal static string BuildNotInArray(string column, string paramName)
    => $"{column} {SqlOps.NotEq} {PgSqlOps.ANY}(@{paramName})";

    internal static string BuildNullCheck(string column, bool isNull)
    => isNull ? $"{column} {SqlOps.IS_NULL}" : $"{column} {SqlOps.IS_NOT_NULL}";

    internal static string LikeOperator(bool caseSensitive)
    => caseSensitive ? Sql.LIKE : PgSql.ILIKE;
}

