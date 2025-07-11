namespace Flecto.Core.Validators;

/// <summary>
/// Provides validation methods for ensuring correct usage of SELECT clauses in queries.
/// </summary>
internal static class SelectValidator
{
    /// <summary>
    /// Ensures that the SELECT clause has not already been set for the query.
    /// Throws an <see cref="InvalidOperationException"/> if SELECT was already called.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="selectWasSet">Indicates whether the SELECT clause has already been set in the query.</param>
    internal static void EnsureValid(bool selectWasSet)
    => EnsureValid(selectWasSet, true, []);

    /// <summary>
    /// Ensures that the SELECT clause has not already been set and that the provided tables and columns are valid.
    /// Throws an <see cref="InvalidOperationException"/> if SELECT was already called.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="selectWasSet">Indicates whether the SELECT clause has already been set in the query.</param>
    /// <param name="tablesWithColumns">The tables and columns to validate for the SELECT clause.</param>
    internal static void EnsureValid(
        bool selectWasSet,
        params (string Table, string[] Columns)[] tablesWithColumns)
    => EnsureValid(selectWasSet, false, tablesWithColumns);

    private static void EnsureValid(
    bool selectWasSet,
    bool skipTableValidation,
    params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (selectWasSet)
            throw new InvalidOperationException("Select can only be called once per query");

        if (skipTableValidation) return;

        TableColumnValidator.EnsureValidTableWithColumns(tablesWithColumns);
    }
}
