using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;

/// <summary>
/// Provides validation methods for <see cref="PaginationFilter"/> instances.
/// </summary>
public static class PaginationValidator
{
    /// <summary>
    /// Performs validation on the specified <see cref="PaginationFilter"/> and returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="PaginationFilter"/> to validate. This parameter is required and must not be null.</param>
    /// <param name="maxLimit">An optional maximum limit that the filter's limit value must not exceed.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        PaginationFilter filter,
        int? maxLimit = null)
    {
        if (filter == null)
        {
            yield return (nameof(PaginationFilter), $"PaginationFilter is required but was null");
            yield break;
        }

        foreach (var error in CheckGreaterThanZero(filter.Limit, nameof(filter.Limit)))
            yield return error;
        foreach (var error in CheckGreaterThanZero(filter.Page, nameof(filter.Page)))
            yield return error;

        if (maxLimit.HasValue && filter.Limit > maxLimit.Value)
            yield return (nameof(filter.Limit), $"Limit cannot exceed {maxLimit.Value}");
    }

    private static IEnumerable<(string Field, string Error)> CheckGreaterThanZero(int value, string fieldName)
    {
        if (value > 0) yield break;
        yield return (fieldName, "Value should be greater than 0");
    }

    /// <summary>
    /// Ensures that the specified <see cref="PaginationFilter"/> is valid.
    /// Throws an <see cref="ArgumentException"/> if the filter is invalid.
    /// This method is used for internal validations within the <c>QueryBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="PaginationFilter"/> to validate.</param>
    internal static void EnsureValid(PaginationFilter filter)
    {
        var errors = Validate(filter).ToArray();
        if (errors.Length == 0) return;

        throw new ArgumentException(
            $"Invalid PaginationFilter: {string.Join("; ", errors.Select(e => $"{e.Field}: {e.Error}"))}",
            nameof(filter));
    }
}

// for doc 
// without FluentV
// var errors = GetPaginationFilterValidationErrors(filter, maxLimit: 500);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "PaginationFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
// with FluentV
// RuleFor(x => x.PaginationFilter)
//     .Custom((filter, context) =>
//     {
//         var errors = GetPaginationFilterValidationErrors(filter, maxLimit: 500);
// 
//         foreach (var error in errors)
//         {
//             context.AddFailure(error.Field, error.Error);
//         }
//     });
