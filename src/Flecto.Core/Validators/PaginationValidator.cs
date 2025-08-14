using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

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
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="PaginationFilter"/> to validate.</param>
    internal static void EnsureValid(PaginationFilter filter, bool forbidPagination)
    {
        var prefix = "PaginationFilter: validation failed:";

        if (forbidPagination)
            throw new ArgumentException(
                $"""
                {prefix}
                Pagination (LIMIT/OFFSET) is not allowed when using COUNT(*) query.
                """
            );

        CommonValidator.ThrowIfErrors(Validate(filter).ToArray(), prefix);
    }
}
