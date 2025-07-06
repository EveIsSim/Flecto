using EveIsSim.QueryBuilder.Core.Enums;
using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class SearchValidator
{
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

    internal static void EnsureValidTsVector(
        SearchFilter filter,
        (string Table, string[] Columns)[] tablesWithColumns,
        DialectType dialectType)
    {
        if (dialectType != DialectType.Postgres)
            throw new ArgumentException("SearchTsvector is supported only for Postgres dialect.");

        EnsureValid(filter, tablesWithColumns);
    }

    internal static void EnsureValid(
        SearchFilter filter,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        TableColumnValidator.EnsureValidTableWithColumns(tablesWithColumns);

        CommonValidator.ThrowIfErrors(
            Validate(filter, allowNullable: false).ToArray(),
            "SearchFilter validation failed:");
    }
}

// for doc
// with FluentValidation
// RuleFor(x => x.SearchFilter)
//     .Custom((filter, context) =>
//     {
//         foreach (var (field, error) in SearchValidator.Validate(filter))
//         {
//             context.AddFailure(field, error);
//         }
//     });
//
//
// Without FluentValidation
//var errors = SearchValidator.Validate(filter).ToArray();
// 
// if (errors.Length > 0)
// {
//     foreach (var (field, error) in errors)
//     {
//         Console.WriteLine($"{field}: {error}");
//     }
// 
//     throw new ValidationException(
//         "SearchFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
