using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides basic logical validation methods for <see cref="EnumFilter{T}"/> instances.
/// </summary>
public static class EnumValidator
{
    /// <summary>
    /// Performs basic logical validation on the specified <see cref="EnumFilter{T}"/> and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The enumeration type to validate.</typeparam>
    /// <param name="filter">The <see cref="EnumFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        EnumFilter<T> filter,
        bool allowNullable = true)
    where T : struct, Enum
    => Validate(filter, null, allowNullable);

    /// <summary>
    /// Performs basic logical validation on the specified <see cref="EnumFilter{T}"/> with an optional custom validator,
    /// and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The enumeration type to validate.</typeparam>
    /// <param name="filter">The <see cref="EnumFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator that allows specifying additional custom validation logic.
    /// </param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        EnumFilter<T> filter,
        Func<EnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator,
        bool allowNullable = true)
        where T : struct, Enum
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInternal(filter, customValidator));

    private static IEnumerable<(string Field, string Error)> ValidateInternal<T>(
        EnumFilter<T> filter,
        Func<EnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
        where T : struct, Enum
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(EnumFilter<T>)))
            yield return error;

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.In, nameof(filter.In)))
            yield return error;

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.NotIn, nameof(filter.NotIn)))
            yield return error;

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EnumFilter{T}"/> is valid for binding to the specified table and column.
    /// Throws an exception if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <typeparam name="T">The enumeration type used in the filter.</typeparam>
    /// <param name="filter">The <see cref="EnumFilter{T}"/> to validate.</param>
    /// <param name="table">The name of the table associated with the filter.</param>
    /// <param name="column">The name of the column associated with the filter.</param>
    internal static void EnsureValid<T>(EnumFilter<T> filter, string table, string column)
        where T : struct, Enum
    => CommonValidator.EnsureValidBindFilter(filter, table, column, static f => Validate(f, false));
}
