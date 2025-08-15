using System.Text.RegularExpressions;
using Flecto.Core.Models.Select;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides validation methods for ensuring that table and column names are valid for SQL query construction.
/// </summary>
internal static class TableColumnValidator
{
    private const string TableNameLabel = "Table name";
    private const string ColumnNameLabel = "Column name";
    private const string AliasNameLabel = "Alias name";

    // Matches valid SQL column names and optional JSONB path expressions.
    // Supports:
    // - Simple column names: e.g., "price", "user_data"
    // - JSONB path with -> or ->>: e.g., "data->'info'->>'name'"
    // Constraints:
    // - Base column must start with a letter or underscore, followed by letters, digits, or underscores
    // - JSON path keys must be alphanumeric or underscore, enclosed in single quotes
    private static readonly Regex ColumnOrJsonPathRegex = new(
        @"^[a-zA-Z_][a-zA-Z0-9_]*(->>'[a-zA-Z0-9_]+'|->'[a-zA-Z0-9_]+')*$",
        RegexOptions.Compiled);

    // Valid SQL identifiers: start with a letter or underscore, followed by letters, numbers, or underscores.
    private static readonly Regex CommonNameRegex = new(
        @"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Ensures that the provided list of tables and their fields (columns with optional aliases)
    /// are valid for use in a SQL SELECT clause.
    /// 
    /// Validation includes:
    /// - At least one table with columns must be provided.
    /// - Each table must have a valid name (non-null, non-empty, valid syntax).
    /// - Each table must have at least one field (column).
    /// - Column names must not be null, empty, or contain invalid SQL characters.
    /// - If provided, aliases must not be null, empty, or contain invalid SQL characters.
    /// - Alias names are validated against the same rules as table/column names.
    /// 
    /// Throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="tablesWithColumns">
    /// An array of <see cref="FromTable"/> objects, where each represents a SQL table and its fields.
    /// Each field includes a column name and an optional alias used for SQL SELECT clause generation.
    /// </param>
    internal static void EnsureValidSelectTableWithColumns(FromTable[] tablesWithColumns)
    {
        if (tablesWithColumns == null || tablesWithColumns.Length == 0)
            throw new ArgumentException("At least one table with columns must be specified");

        var aliases = tablesWithColumns
            .SelectMany(t => t.Fields)
            .Select(x => x.Alias)
            .Where(x => x != null);

        foreach (var alias in aliases)
            EnsureValidSqlName(alias!, AliasNameLabel);

        var tableWithColumns = tablesWithColumns
            .Select(t => (t.Table, t.Fields.Select(f => f.Column).ToArray()))
            .ToArray();

        EnsureValidTableWithColumns(tableWithColumns);
    }

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
            TableNameLabel => CommonNameRegex,
            AliasNameLabel => CommonNameRegex,
            ColumnNameLabel => ColumnOrJsonPathRegex,
            _ => throw new ArgumentException($"Unknown SQL name type: '{label}'")
        };

        if (!regex.IsMatch(name))
            throw new ArgumentException($"Invalid {label} syntax: '{name}'");
    }
}
