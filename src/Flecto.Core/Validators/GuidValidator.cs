using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides basic logical validation methods for <see cref="GuidFilter"/> instances.
/// </summary>
public static class GuidValidator
{
    /// <summary>
    /// Performs basic logical validation on the specified <see cref="GuidFilter"/> and returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="GuidFilter"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        GuidFilter filter,
        bool allowNullable = true)
    => Validate(filter, null, allowNullable);

    /// <summary>
    /// Performs basic logical validation on the specified <see cref="GuidFilter"/> with an optional custom validator,
    /// and returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="GuidFilter"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator that allows specifying additional custom validation logic.
    /// </param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        GuidFilter filter,
        Func<GuidFilter, (bool IsValid, string? ErrorMessage)>? customValidator,
        bool allowNullable = true)
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInternal(filter, customValidator));

    private static IEnumerable<(string Field, string Error)> ValidateInternal(
        GuidFilter filter,
        Func<GuidFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(GuidFilter)))
            yield return error;

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.In, nameof(filter.In)))
            yield return error;

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.NotIn, nameof(filter.NotIn)))
            yield return error;

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    /// <summary>
    /// Ensures that the specified <see cref="GuidFilter"/> is valid for binding to the specified table and column.
    /// Throws an exception if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="GuidFilter"/> to validate.</param>
    /// <param name="table">The name of the table associated with the filter.</param>
    /// <param name="column">The name of the column associated with the filter.</param>
    internal static void EnsureValid(GuidFilter filter, string table, string column)
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}
