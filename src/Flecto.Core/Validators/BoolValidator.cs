using Flecto.Core.Models.Filters;
using Flecto.Core.Validators.Enums;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides basic logical validation methods for <see cref="BoolFilter"/> instances
/// </summary>
public static class BoolValidator
{
    /// <summary>
    /// Performs basic logical validation on the specified <see cref="BoolFilter"/> and returns a collection of validation errors, if any
    /// </summary>
    /// <param name="filter">The <see cref="BoolFilter"/> to validate</param>
    /// <param name="options">The validation options to apply during validation</param>
    /// <returns>
    /// A collection of tuples containing the field name and the error message for each validation error
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        BoolFilter? filter,
        BoolFilterValidationOptions options = BoolFilterValidationOptions.None)
    => Validate(filter, options, null);

    /// <summary>
    /// Performs basic logical validation on the specified <see cref="BoolFilter"/> with optional custom validation logic,
    /// and returns a collection of validation errors, if any
    /// </summary>
    /// <param name="filter">The <see cref="BoolFilter"/> to validate</param>
    /// <param name="options">The validation options to apply during validation</param>
    /// <param name="customValidator">
    /// An optional custom validator that allows specifying additional user-defined validation logic
    /// The function should return a validation result and an error message if validation fails
    /// </param>
    /// <returns>
    /// A collection of tuples containing the field name and the error message for each validation error.
    /// </returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        BoolFilter? filter,
        BoolFilterValidationOptions options = BoolFilterValidationOptions.None,
        Func<BoolFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => CommonValidator.ValidateNullOr(
        filter,
        options.HasFlag(BoolFilterValidationOptions.AllowNullable),
        filter => ValidateInner(filter, options, customValidator));

    /// <summary>
    /// Performs basic logical validation on the specified non-null <see cref="BoolFilter"/> with optional custom validation logic,
    /// and returns a collection of validation errors, if any
    /// </summary>
    /// <param name="filter">The non-null <see cref="BoolFilter"/> to validate</param>
    /// <param name="options">The validation options to apply during validation</param>
    /// <param name="customValidator">
    /// An optional custom validator that allows specifying additional user-defined validation logic
    /// The function should return a validation result and an error message if validation fails
    /// </param>
    /// <returns>
    /// A collection of tuples containing the field name and the error message for each validation error
    /// </returns>
    private static IEnumerable<(string Field, string Error)> ValidateInner(
        BoolFilter filter,
        BoolFilterValidationOptions options = BoolFilterValidationOptions.None,
        Func<BoolFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(BoolFilter)))
            yield return error;

        if (options.HasFlag(BoolFilterValidationOptions.RequireAtLeastOne) &&
            !filter.Eq.HasValue && !filter.NotEq.HasValue && !filter.Null.HasValue)
        {
            yield return (nameof(BoolFilter), "At least one of Eq, NotEq, Null must be specified");
        }

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BoolFilter"/> is valid for binding to the specified table and column.
    /// Throws an exception if the filter is invalid.
    /// This method is used in internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="BoolFilter"/> to validate</param>
    /// <param name="table">The name of the table associated with the filter</param>
    /// <param name="column">The name of the column associated with the filter</param>
    internal static void EnsureValid(BoolFilter filter, string table, string column)
    => CommonValidator.EnsureValidBindFilter(
        filter,
        table,
        column,
        f => Validate(f, BoolFilterValidationOptions.None));
}

// For documentation
// with FluentValidation
//RuleFor(x => x.BoolFilter)
//    .Custom((filter, context) =>
//    {
//        var errors = GetBoolFilterValidationErrors(filter, requireAtLeastOne: false);
//
//        foreach (var error in errors)
//        {
//            context.AddFailure(error.Field, error.Error);
//        }
//    });
//
// Without 
// var errors = GetBoolFilterValidationErrors(
//     filter,
//     requireAtLeastOne: true,
//     disallowEqAndNotEqSimultaneously: true);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "BoolFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
