using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Helpers;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;


internal static class BoolSqlBuilder
{
    internal static string BuildComparison(string column, string paramName, ComparisonOperator op)
    => $"{column} {SqlOperatorHelper.GetSqlEqualityOperator(op)} @{paramName}";
}
