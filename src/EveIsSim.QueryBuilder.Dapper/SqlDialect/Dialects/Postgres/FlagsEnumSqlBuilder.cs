using EveIsSim.QueryBuilder.Dapper.Constants;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;

internal static class FlagsEnumSqlBuilder
{
    internal static string BuildHasFlag(string column, string paramName)
    => $"{column} & @{paramName} {SqlOps.NotEq} 0";

    internal static string BuildNotHasFlag(string column, string paramName)
    => $"{column} & @{paramName} {SqlOps.Eq} 0";
}

