using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.SqlDialect.Enums;

namespace Flecto.Dapper.SqlDialect;

/// <summary>
/// Defines SQL dialect operations for building query conditions in a database-agnostic manner.
/// This interface is used by the <c>FlectoBuilder</c> to generate SQL snippets for filters, search, and comparisons,
/// allowing support for different SQL dialects (e.g., Postgres, MSSQL) without exposing implementation details to consumers.
/// </summary>
internal interface ISqlDialect
{
    #region Search

    /// <summary>
    /// Builds a SQL search condition using the provided <see cref="SearchFilter"/> across the specified tables and columns.
    /// </summary>
    /// <param name="filter">The search filter containing the search value and case sensitivity flag.</param>
    /// <param name="paramName">The parameter name to use in the generated SQL.</param>
    /// <param name="tableColumns">The tables and columns to include in the search.</param>
    /// <returns>A tuple containing the SQL condition and the parameter value for the search.</returns>
    (string SqlCondition, string ParamValue) BuildSearch(
        SearchFilter filter,
        string paramName,
        (string Table, string[] Columns)[] tableColumns);

    /// <summary>
    /// Builds a SQL condition for full-text search using the <c>tsvector</c> mechanism in supported dialects (e.g., Postgres).
    /// </summary>
    /// <param name="paramName">The parameter name to use in the SQL.</param>
    /// <param name="mode">
    /// The text search mode, which determines how the search query is processed (e.g., Plain, WebStyle).
    /// </param>
    /// <param name="config">
    /// The text search configuration to use (e.g., "simple", "english").
    /// This determines the dictionary and parsing rules used during the search.
    /// </param>
    /// <param name="tablesWithColumns">The tables and columns to include in the full-text search.</param>
    /// <returns>The SQL condition string for the full-text search.</returns>
    string BuildTsVectorSearchCondition(
        string paramName,
        TextSearchMode mode,
        string config,
        (string Table, string[] Columns)[] tablesWithColumns);

    #endregion

    #region String 

    /// <summary> 
    /// Builds a SQL condition for checking if a string column is in the specified input array. 
    /// </summary>
    (string SqlCondition, string[] paramValues) BuildStringInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive);

    /// <summary> 
    /// Builds a SQL condition for checking if a string column is not in the specified input array. 
    /// </summary>
    (string SqlCondition, string[] paramValues) BuildStringNotInArray(
        string column,
        string paramName,
        string[] input,
        bool caseSensitive);

    /// <summary> 
    /// Builds a SQL condition for checking string equality on a column. 
    /// </summary>
    string BuildStringEquals(string column, string paramName, bool caseSensitive);

    /// <summary> 
    /// Builds a SQL condition for checking string inequality on a column. 
    /// </summary>
    string BuildStringNotEquals(string column, string paramName, bool caseSensitive);

    /// <summary> 
    /// Builds a SQL LIKE condition for a string column using the specified match type and case sensitivity. 
    /// </summary>
    (string SqlCondition, string ParamValue) BuildStringLike(
        string column,
        string paramName,
        string input,
        StringMatchType matchType,
        bool caseSensitive);

    #endregion

    #region Bool

    /// <summary> Builds a SQL condition for boolean comparisons on a column. </summary>
    string BuildBoolComparison(string column, string paramName, ComparisonOperator op);

    #endregion

    #region Enum

    /// <summary> Builds a SQL condition for enum comparisons on a column. </summary>
    (string SqlCondition, object ParamValue) BuildEnumComparison<T>(
        string column,
        string paramName,
        T value,
        ComparisonOperator op,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    /// <summary> 
    /// Builds a SQL condition for checking if an enum column is in the specified array. 
    /// </summary>
    (string SqlCondition, object[] ParamValue) BuildEnumInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    /// <summary> 
    /// Builds a SQL condition for checking if an enum column is not in the specified array. 
    /// </summary>
    (string SqlCondition, object[] ParamValue) BuildEnumNotInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum;

    #endregion

    #region FlagsEnum

    /// <summary> Builds a SQL condition for checking if an enum column has a specific flag set. </summary>
    string BuildHasFlag(string column, string paramName);

    /// <summary> Builds a SQL condition for checking if an enum column does not have a specific flag set. </summary>
    string BuildNotHasFlag(string column, string paramName);

    #endregion

    #region Common

    /// <summary> Builds a SQL condition for common comparison operators on a column. </summary>
    string BuildCommonComparison(string column, string param, ComparisonOperator op);

    /// <summary> Builds a SQL condition for checking if a column value is in a parameterized array. </summary>
    string BuildCommonInArray(string column, string param);

    /// <summary> Builds a SQL condition for checking if a column value is not in a parameterized array. </summary>
    string BuildCommonNotInArray(string column, string param);

    /// <summary> Builds a SQL condition for checking if a column value is null or not null. </summary>
    string BuildCommonNullCheck(string column, bool isNull);

    #endregion
}
