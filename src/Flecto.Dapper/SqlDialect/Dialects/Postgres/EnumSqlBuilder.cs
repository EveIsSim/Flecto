using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.Constants;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Constants;
using Flecto.Dapper.SqlDialect.Enums;
using Flecto.Dapper.SqlDialect.Helpers;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres;

/// <summary>
/// Provides helper methods for building SQL conditions for <c>enum</c> filters in a dialect-agnostic way.
/// Supports building equality and IN/NOT IN conditions for enum columns with flexible filter modes.
/// </summary>
internal static class EnumSqlBuilder
{
    /// <summary>
    /// Builds a SQL condition for comparing an enum column with a single enum value using the specified comparison operator and filter mode.
    /// </summary>
    /// <typeparam name="T">The enum type to compare.</typeparam>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL condition.</param>
    /// <param name="value">The enum value to compare against.</param>
    /// <param name="op">The comparison operator to use (only Eq and NotEq are supported).</param>
    /// <param name="filterMode">The mode to use for representing enum values in SQL (Name, Value, ValueString).</param>
    /// <returns>A tuple containing the SQL condition string and the parameter value.</returns>
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

    /// <summary>
    /// Builds a SQL condition for checking if an enum column's value is in the specified array of enum values using the provided filter mode.
    /// </summary>
    /// <typeparam name="T">The enum type to compare.</typeparam>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL condition.</param>
    /// <param name="rowArr">The array of enum values to check against.</param>
    /// <param name="filterMode">The mode to use for representing enum values in SQL (Name, Value, ValueString).</param>
    /// <returns>A tuple containing the SQL condition string and the array of parameter values.</returns>
    internal static (string SqlCondition, object[] ParamValue) BuildInArray<T>(
        string column,
        string paramName,
        T[] rowArr,
        EnumFilterMode filterMode)
        where T : struct, Enum
    {
        var values = rowArr.Select(x => ConvertValue(x, filterMode)).ToArray();

        return ($"{column} {SqlOps.Eq} {PgSqlOps.ANY}(@{paramName})", values);
    }

    /// <summary>
    /// Builds a SQL condition for checking if an enum column's value is not in the specified array of enum values using the provided filter mode.
    /// </summary>
    /// <typeparam name="T">The enum type to compare.</typeparam>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramName">The parameter name to use in the SQL condition.</param>
    /// <param name="rowArr">The array of enum values to check against.</param>
    /// <param name="filterMode">The mode to use for representing enum values in SQL (Name, Value, ValueString).</param>
    /// <returns>A tuple containing the SQL condition string and the array of parameter values.</returns>
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

    /// <summary>
    /// Converts an enum value to its SQL representation based on the specified <see cref="EnumFilterMode"/>.
    /// Supports:
    /// <list type="bullet">
    /// <item><see cref="EnumFilterMode.Name"/> - converts the enum value to its name as a string (e.g., <c>Enum.A => "A"</c>).</item>
    /// <item><see cref="EnumFilterMode.Value"/> - keeps the enum value as its numeric representation (e.g., <c>Enum.A => 0</c>).</item>
    /// <item><see cref="EnumFilterMode.ValueString"/> - converts the numeric value to its string representation using <c>ToString("D")</c> (e.g., <c>Enum.A => "0"</c>).
    /// <para><c>ToString("D")</c> is a standard .NET format specifier that returns the underlying numeric value of the enum as a string.</para></item>
    /// </list>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the filter mode is unsupported.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to convert.</param>
    /// <param name="filterMode">The filter mode specifying how to represent the enum value.</param>
    /// <returns>The converted value for SQL usage.</returns>
    private static object ConvertValue<T>(T value, EnumFilterMode filterMode)
        where T : struct, Enum
    {
        return filterMode switch
        {
            EnumFilterMode.Name => value.ToString(),
            EnumFilterMode.Value => value,
            EnumFilterMode.ValueString => value.ToString("D"),
            _ => throw new ArgumentOutOfRangeException(nameof(filterMode), filterMode, null)
        };
    }

}
