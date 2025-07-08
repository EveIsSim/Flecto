using EveIsSim.QueryBuilder.Dapper.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Helpers;

/// <summary>
/// Provides helper methods for working with SQL equality operators within filters.
/// </summary>
internal static class SqlOperatorHelper
{
    /// <summary>
    /// Ensures that the specified <see cref="ComparisonOperator"/> is either <c>Eq</c> or <c>NotEq</c>.
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if any other operator is provided.
    /// This is used to enforce correct operator usage in filters that only support equality checks.
    /// </summary>
    /// <param name="op">The comparison operator to validate.</param>
    internal static void EnsureEqualityOperator(ComparisonOperator op)
    {
        if (op is not (ComparisonOperator.Eq or ComparisonOperator.NotEq))
            throw new ArgumentOutOfRangeException(nameof(op), op, "Only Eq and NotEq are supported for this filter.");
    }

    /// <summary>
    /// Returns the SQL string representation for the specified <see cref="ComparisonOperator"/>,
    /// supporting only <c>Eq</c> and <c>NotEq</c> operators.
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if any other operator is provided.
    /// </summary>
    /// <param name="op">The comparison operator to convert.</param>
    /// <returns>The corresponding SQL operator string (e.g., "=" or "!=").</returns>
    internal static string GetSqlEqualityOperator(ComparisonOperator op) => op switch
    {
        ComparisonOperator.Eq => SqlOps.Eq,
        ComparisonOperator.NotEq => SqlOps.NotEq,
        _ => throw new ArgumentOutOfRangeException(
                nameof(op), op, "Only Eq and NotEq are supported for this filter.")
    };
}
