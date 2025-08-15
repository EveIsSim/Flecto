using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides basic logical validation methods for <see cref="FlagsEnumFilter{T}"/> instances.
/// </summary>
public static class FlagsEnumValidator
{
    /// <summary>
    /// Performs basic logical validation on the specified <see cref="FlagsEnumFilter{T}"/> and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The enumeration type with flags to validate.</typeparam>
    /// <param name="filter">The <see cref="FlagsEnumFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        FlagsEnumFilter<T> filter,
        bool allowNullable = true)
    where T : struct, Enum
    => Validate(filter, null, allowNullable);

    /// <summary>
    /// Performs basic logical validation on the specified <see cref="FlagsEnumFilter{T}"/> with an optional custom validator,
    /// and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The enumeration type with flags to validate.</typeparam>
    /// <param name="filter">The <see cref="FlagsEnumFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator that allows specifying additional custom validation logic.
    /// </param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
    FlagsEnumFilter<T> filter,
    Func<FlagsEnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator,
    bool allowNullable = true)
    where T : struct, Enum
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInternal(filter, customValidator));

    internal static IEnumerable<(string Field, string Error)> ValidateInternal<T>(
    FlagsEnumFilter<T> filter,
    Func<FlagsEnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    where T : struct, Enum
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(FlagsEnumFilter<T>)))
            yield return error;

        if (filter.HasFlag.HasValue && filter.NotHasFlag.HasValue &&
            EqualityComparer<T>.Default.Equals(filter.HasFlag.Value, filter.NotHasFlag.Value))
        {
            yield return (nameof(FlagsEnumFilter<T>), $"HasFlag and NotHasFlag cannot be equal ({filter.HasFlag.Value})");
        }

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    /// <summary>
    /// Ensures that the specified <see cref="FlagsEnumFilter{T}"/> is valid for binding to the specified table and column.
    /// Throws an exception if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <typeparam name="T">The enumeration type with flags used in the filter.</typeparam>
    /// <param name="filter">The <see cref="FlagsEnumFilter{T}"/> to validate.</param>
    /// <param name="table">The name of the table associated with the filter.</param>
    /// <param name="column">The name of the column associated with the filter.</param>
    internal static void EnsureValid<T>(FlagsEnumFilter<T> filter, string table, string column)
        where T : struct, Enum
    => CommonValidator.EnsureValidBindFilter(filter, table, column, static f => Validate(f, false));
}
