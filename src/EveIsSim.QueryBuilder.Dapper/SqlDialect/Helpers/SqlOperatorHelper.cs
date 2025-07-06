using EveIsSim.QueryBuilder.Dapper.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Helpers;


internal static class SqlOperatorHelper
{
    internal static void EnsureEqualityOperator(ComparisonOperator op)
    {
        if (op is not (ComparisonOperator.Eq or ComparisonOperator.NotEq))
            throw new ArgumentOutOfRangeException(nameof(op), op, "Only Eq and NotEq are supported for this filter.");
    }

    internal static string GetSqlEqualityOperator(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Eq => SqlOps.Eq,
        ComparisonOperator.NotEq => SqlOps.NotEq,
        _ => throw new ArgumentOutOfRangeException(
                nameof(op), op, "Only Eq and NotEq are supported for this filter.")
    };
}
