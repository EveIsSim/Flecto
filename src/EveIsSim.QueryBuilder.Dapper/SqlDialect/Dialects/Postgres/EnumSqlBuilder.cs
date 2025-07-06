using EveIsSim.QueryBuilder.Core.Models.Filters.Enums;
using EveIsSim.QueryBuilder.Dapper.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Helpers;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;


internal static class EnumSqlBuilder
{
    // write a documentation ant tests that there are only two types of numeric or string Runtime can easily recognize and write tests for all this
    internal static (string SqlCondition, object ParamValue) BuildComparison<T>(
        string column,
        string paramName,
        T value,
        ComparisonOperator op,
        EnumFilterMode filterMode)
        where T : struct, Enum
    {
        var opStr = SqlOperatorHelper.GetSqlEqualityOperator(op);
        var val = ConvertValue(value, filterMode);

        return ($"{column} {opStr} @{paramName}", val);
    }

    internal static (string SqlCondition, object[] ParamValue) BuildInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum
    {
        var values = rowArr.Select(x => ConvertValue(x, filterMode)).ToArray();

        return ($"{column} = {PgSqlOps.ANY}(@{paramName})", values);
    }

    internal static (string SqlCondition, object[] ParamValue) BuildNotInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum
    {
        var values = rowArr.Select(x => ConvertValue(x, filterMode)).ToArray();

        return ($"{column} {SqlOps.NotEq} {PgSqlOps.ANY}(@{paramName})", values);
    }

    private static object ConvertValue<T>(T value, EnumFilterMode filterMode)
        where T : struct, Enum
    {
        return filterMode switch
        {
            EnumFilterMode.Name => value.ToString(), // 999 test : Enum (A : 0) => "A"
            EnumFilterMode.Value => value, // 999 test : Enum (A : 0) => 0
            EnumFilterMode.ValueString => value.ToString("D"), // 999 test : Enum (A : 0) => "0" // descride, what is D
            _ => throw new ArgumentOutOfRangeException(nameof(filterMode), filterMode, null)
        };
    }

}
