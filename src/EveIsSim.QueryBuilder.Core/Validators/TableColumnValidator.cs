using System.Text.RegularExpressions;

namespace EveIsSim.QueryBuilder.Core.Validators;


internal static class TableColumnValidator
{
    const string TableNameLabel = "Table name";
    const string ColumnNameLabel = "Column name";

    // right docs what we expect what no and validate it.
    private static readonly Regex ColumnRegex = new Regex(
        @"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    internal static void EnsureValidTableWithColumns(
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (tablesWithColumns is null || tablesWithColumns.Length == 0)
            throw new ArgumentException("At least one table with columns must be specified");

        foreach (var (table, columns) in tablesWithColumns)
            EnsureValid(table, columns);
    }

    private static void EnsureValid(string table, string[] columns)
    {
        EnsureValidSqlName(table, TableNameLabel);

        if (columns is null || columns.Length == 0)
            throw new ArgumentException($"Table '{table}' must have at least one column specified");

        foreach (var c in columns)
            EnsureValidSqlName(c, ColumnNameLabel);
    }

    private static void EnsureValidSqlName(string name, string sqlNameType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{sqlNameType} cannot be null or whitespace");

        if (!ColumnRegex.IsMatch(name))
            throw new ArgumentException($"Invalid {sqlNameType} syntax: '{name}'");
    }
}
