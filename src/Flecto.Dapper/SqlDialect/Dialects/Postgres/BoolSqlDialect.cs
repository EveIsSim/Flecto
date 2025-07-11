using Flecto.Dapper.SqlDialect.Enums;
using Flecto.Dapper.SqlDialect.Helpers;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;


internal static class BoolSqlBuilder
{
    internal static string BuildComparison(string column, string paramName, ComparisonOperator op)
    => $"{column} {SqlOperatorHelper.GetSqlEqualityOperator(op)} @{paramName}";
}
