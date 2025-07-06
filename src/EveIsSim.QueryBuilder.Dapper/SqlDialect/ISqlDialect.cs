using EveIsSim.QueryBuilder.Core.Models.Filters;
using EveIsSim.QueryBuilder.Core.Models.Filters.Enums;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect;

internal interface ISqlDialect
{
    #region Search

    (string SqlCondition, string ParamValue) BuildSearch(
        SearchFilter filter,
        string paramName,
        (string Table, string[] Columns)[] tableColumns);

    string BuildTsVectorSearchCondition(
        string paramName,
        TextSearchMode mode,
        string config,
        (string Table, string[] Columns)[] tablesWithColumns);

    #endregion


    #region String 

    (string SqlCondition, string[] paramValues) BuildStringInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive);
    (string SqlCondition, string[] paramValues) BuildStringNotInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive);
    (string SqlCondition, string ParamValue) BuildStringEquals(
        string column,
        string paramName,
        string input,
        bool caseSensitive);
    (string SqlCondition, string ParamValue) BuildStringNotEquals(
        string column,
        string paramName,
        string input,
        bool caseSensitive);
    (string SqlCondition, string ParamValue) BuildStringLike(
        string column,
        string paramName,
        string input,
        StringMatchType matchType,
        bool caseSensitive);

    #endregion

    #region Bool

    string BuildBoolComparison(string column, string paramName, ComparisonOperator op);

    #endregion

    #region Enum

    (string SqlCondition, object ParamValue) BuildEnumComparison<T>(
        string column,
        string paramName,
        T value,
        ComparisonOperator op,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    (string SqlCondition, object[] ParamValue) BuildEnumInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    (string SqlCondition, object[] ParamValue) BuildEnumNotInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    #endregion

    #region FlagsEnum

    string BuildHasFlag(string column, string paramName);
    string BuildNotHasFlag(string column, string paramName);

    #endregion

    #region Common

    string BuildCommonComparison(string column, string param, ComparisonOperator op);
    string BuildCommonInArray(string column, string param);
    string BuildCommonNotInArray(string column, string param);
    string BuildCommonNullCheck(string column, bool isNull);

    #endregion
}
