using System.Text.RegularExpressions;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides validation methods for ensuring that table and column names are valid for SQL query construction.
/// </summary>
internal static class TableColumnValidator
{
    const string TableNameLabel = "Table name";
    const string ColumnNameLabel = "Column name";

    // Matches valid SQL column names and optional JSONB path expressions.
    // Supports:
    // - Simple column names: e.g., "price", "user_data"
    // - JSONB path with -> or ->>: e.g., "data->'info'->>'name'"
    // Constraints:
    // - Base column must start with a letter or underscore, followed by letters, digits, or underscores
    // - JSON path keys must be alphanumeric or underscore, enclosed in single quotes
    private static readonly Regex ColumnOrJsonPathRegex = new Regex(
        @"^[a-zA-Z_][a-zA-Z0-9_]*(->>'[a-zA-Z0-9_]+'|->'[a-zA-Z0-9_]+')*$",
        RegexOptions.Compiled);


    // Valid SQL identifiers: start with a letter or underscore, followed by letters, numbers, or underscores.
    private static readonly Regex TableNameRegex = new Regex(
        @"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Ensures that the provided tables and their columns are valid for use in SQL queries.
    /// 
    /// Requirements:
    /// - At least one table with columns must be provided.
    /// - Table names must not be null, empty, or whitespace.
    /// - Column arrays for each table must contain at least one column.
    /// - Column names must not be null, empty, or whitespace.
    /// - Table and column names must match the regex: start with a letter or underscore,
    ///   followed by letters, numbers, or underscores only.
    /// 
    /// Throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="tablesWithColumns">
    /// An array of table and column pairs to validate.
    /// Each tuple must contain a table name and an array of column names associated with that table.
    /// </param>
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

    private static void EnsureValidSqlName(string name, string label)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{label} cannot be null or whitespace");

        var regex = label switch
        {
            TableNameLabel => TableNameRegex,
            ColumnNameLabel => ColumnOrJsonPathRegex,
            _ => throw new ArgumentException($"Unknown SQL name type: '{label}'")
        };

        if (!regex.IsMatch(name))
            throw new ArgumentException($"Invalid {label} syntax: '{name}'");
    }
}
