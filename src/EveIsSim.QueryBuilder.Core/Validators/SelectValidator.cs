namespace EveIsSim.QueryBuilder.Core.Validators;

internal static class SelectValidator
{
    internal static void EnsureValid(bool selectWasSet)
    => EnsureValid(selectWasSet, true, []);

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
