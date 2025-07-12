using Flecto.Core.Models.Filters;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides basic logical validation methods for <see cref="NumericFilter{T}"/> instances.
/// </summary>
public static class NumericValidator
{
    /// <summary>
    /// Performs basic logical validation on the specified <see cref="NumericFilter{T}"/> and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The numeric type to validate, which must be a struct implementing <see cref="IComparable"/>.</typeparam>
    /// <param name="filter">The <see cref="NumericFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        NumericFilter<T> filter,
        bool allowNullable = true)
        where T : struct, IComparable
    => Validate<T>(filter, allowNullable, null);

    /// <summary>
    /// Performs basic logical validation on the specified <see cref="NumericFilter{T}"/> with an optional custom validator,
    /// and returns any validation errors found.
    /// </summary>
    /// <typeparam name="T">The numeric type to validate, which must be a struct implementing <see cref="IComparable"/>.</typeparam>
    /// <param name="filter">The <see cref="NumericFilter{T}"/> to validate.</param>
    /// <param name="allowNullable">Indicates whether the filter can be null during validation.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator that allows specifying additional custom validation logic.
    /// </param>
    /// <returns>
    /// A collection of field-error pairs indicating validation errors, if any.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        NumericFilter<T> filter,
        bool allowNullable = true,
        Func<NumericFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
        where T : struct, IComparable
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInternal(filter, customValidator));

    private static IEnumerable<(string Field, string Error)> ValidateInternal<T>(
        NumericFilter<T> filter,
        Func<NumericFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
        where T : struct, IComparable
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(NumericFilter<T>)))
            yield return error;

        foreach (var error in CommonValidator.ValidateRangeConsistency(
            filter.Gt,
            filter.Gte,
            filter.Lt,
            filter.Lte,
            nameof(NumericFilter<T>)))
        {
            yield return error;
        }

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.In, nameof(filter.In)))
            yield return error;

        foreach (var error in CommonValidator.ValidateArrayIfNeeded(filter.NotIn, nameof(filter.NotIn)))
            yield return error;

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    /// <summary>
    /// Ensures that the specified <see cref="NumericFilter{T}"/> is valid for binding to the specified table and column.
    /// Throws an exception if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <typeparam name="T">The numeric type used in the filter, which must be a struct implementing <see cref="IComparable"/>.</typeparam>
    /// <param name="filter">The <see cref="NumericFilter{T}"/> to validate.</param>
    /// <param name="table">The name of the table associated with the filter.</param>
    /// <param name="column">The name of the column associated with the filter.</param>
    internal static void EnsureValid<T>(NumericFilter<T> filter, string table, string column)
        where T : struct, IComparable
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}
