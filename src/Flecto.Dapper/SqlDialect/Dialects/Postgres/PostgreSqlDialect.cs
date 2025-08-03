using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.SqlDialect.Enums;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;

/// <summary>
/// Provides a PostgreSQL-specific implementation of the <see cref="ISqlDialect"/> interface.
/// This dialect is used by the <c>FlectoBuilder</c> to generate SQL conditions compatible with PostgreSQL syntax,
/// including search and full-text search capabilities.
/// </summary>
internal class PostgresSqlDialect : ISqlDialect
{
    #region SearchBuilder

    public (string SqlCondition, string ParamValue) BuildSearch(
        SearchFilter filter,
        string paramName,
        (string Table, string[] Columns)[] tablesWithColumns)
    => SearchBuilder.Build(filter, paramName, tablesWithColumns);

    public string BuildTsVectorSearchCondition(
        string paramName,
        TextSearchMode mode,
        string config,
        (string Table, string[] Columns)[] tablesWithColumns)
    => SearchBuilder.BuildTsVectorSearchCondition(paramName, mode, config, tablesWithColumns);

    #endregion

    #region StringSqlBuilder 

    public (string SqlCondition, string[] paramValues) BuildStringInArray(
        string column, string paramName, string[] input, bool caseSensitive)
    => StringSqlBuilder.BuildInArray(column, paramName, input, caseSensitive);

    public (string SqlCondition, string[] paramValues) BuildStringNotInArray(
        string column, string paramName, string[] input, bool caseSensitive)
    => StringSqlBuilder.BuildNotInArray(column, paramName, input, caseSensitive);

    public (string SqlCondition, string ParamValue) BuildStringEquals(
        string column, string paramName, string input, bool caseSensitive)
    => StringSqlBuilder.BuildEquals(column, paramName, input, caseSensitive);

    public (string SqlCondition, string ParamValue) BuildStringNotEquals(
        string column, string paramName, string input, bool caseSensitive)
    => StringSqlBuilder.BuildNotEquals(column, paramName, input, caseSensitive);

    public (string SqlCondition, string ParamValue) BuildStringLike(
        string column, string paramName, string input, StringMatchType matchType, bool caseSensitive)
    => StringSqlBuilder.BuildLike(column, paramName, input, matchType, caseSensitive);

    #endregion

    #region Bool

    public string BuildBoolComparison(string column, string paramName, ComparisonOperator op)
    => BoolSqlBuilder.BuildComparison(column, paramName, op);

    #endregion

    #region Enum

    public (string SqlCondition, object ParamValue) BuildEnumComparison<T>(
        string column,
        string paramName,
        T value,
        ComparisonOperator op,
        EnumFilterMode filterMode)
        where T : struct, Enum
    => EnumSqlBuilder.BuildComparison(column, paramName, value, op, filterMode);

    public (string SqlCondition, object[] ParamValue) BuildEnumInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum
    => EnumSqlBuilder.BuildInArray(column, paramName, rowArr, filterMode);

    public (string SqlCondition, object[] ParamValue) BuildEnumNotInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum
    => EnumSqlBuilder.BuildNotInArray(column, paramName, rowArr, filterMode);

    #endregion

    #region FlagsEnum

    public string BuildHasFlag(string column, string paramName)
    => FlagsEnumSqlBuilder.BuildHasFlag(column, paramName);

    public string BuildNotHasFlag(string column, string paramName)
    => FlagsEnumSqlBuilder.BuildNotHasFlag(column, paramName);

    #endregion

    #region Common

    public string BuildCommonComparison(string column, string paramName, ComparisonOperator op)
    => CommonSqlBuilder.BuildNumericComparison(column, paramName, op);

    public string BuildCommonInArray(string column, string paramName)
    => CommonSqlBuilder.BuildInArray(column, paramName);

    public string BuildCommonNotInArray(string column, string paramName)
    => CommonSqlBuilder.BuildNotInArray(column, paramName);

    public string BuildCommonNullCheck(string column, bool isNull)
    => CommonSqlBuilder.BuildNullCheck(column, isNull);

    #endregion
}
