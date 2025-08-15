using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides validation methods for <see cref="SearchFilter"/> instances.
/// </summary>
public static class SearchValidator
{
    /// <summary>
    /// Performs validation on the specified <see cref="SearchFilter"/> and returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="SearchFilter"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <param name="maxLength">An optional maximum length that the filter's value must not exceed.</param>
    /// <param name="minLength">An optional minimum length that the filter's value must meet.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        SearchFilter filter,
        bool allowNullable = true,
        int? maxLength = null,
        int? minLength = null)
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInner(filter, maxLength, minLength));

    private static IEnumerable<(string Field, string Error)> ValidateInner(
        SearchFilter filter,
        int? maxLength = null,
        int? minLength = null)
    {
        var field = nameof(filter.Value);

        if (string.IsNullOrWhiteSpace(filter.Value))
        {
            yield return (field, "Value is required and cannot be empty or whitespace if filter was specified");
            yield break;
        }

        if (minLength.HasValue && filter.Value.Length < minLength.Value)
            yield return (field, $"Value must be at least {minLength.Value} characters");

        if (maxLength.HasValue && filter.Value.Length > maxLength.Value)
            yield return (field, $"Value cannot exceed {maxLength.Value} characters");
    }

    /// <summary>
    /// Ensures that the specified <see cref="SearchFilter"/> is valid for use with full-text search on PostgreSQL.
    /// Throws an <see cref="ArgumentException"/> if the filter is invalid or if the dialect is not PostgreSQL.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="SearchFilter"/> to validate.</param>
    /// <param name="tablesWithColumns">The tables and columns to validate against.</param>
    /// <param name="dialectType">The SQL dialect type. Must be <see cref="DialectType.Postgres"/> for full-text search validation.</param>
    internal static void EnsureValidTsVector(
        SearchFilter filter,
        (string Table, string[] Columns)[] tablesWithColumns,
        DialectType dialectType)
    {
        if (dialectType != DialectType.Postgres)
            throw new ArgumentException("SearchTsvector is supported only for Postgres dialect.");

        EnsureValid(filter, tablesWithColumns);
    }

    /// <summary>
    /// Ensures that the specified <see cref="SearchFilter"/> is valid and applicable to the provided tables and columns.
    /// Throws an <see cref="ArgumentException"/> if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="SearchFilter"/> to validate.</param>
    /// <param name="tablesWithColumns">The tables and columns to validate against.</param>
    internal static void EnsureValid(
        SearchFilter filter,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        TableColumnValidator.EnsureValidTableWithColumns(tablesWithColumns);

        CommonValidator.ThrowIfErrors(
            [.. Validate(filter, allowNullable: false)],
            "SearchFilter validation failed:");
    }
}
