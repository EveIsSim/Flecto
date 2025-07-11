using Flecto.Core.Models.Filters;
using Flecto.Core.Validators.Enums;

namespace Flecto.Core.Validators;

/// <summary>
/// Provides validation methods for <see cref="StringFilter"/> instances.
/// </summary>
public static class StringValidator
{
    /// <summary>
    /// Validates the specified <see cref="StringFilter"/> with an optional maximum length and validation options.
    /// Returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="maxLength">An optional maximum allowed length for string values.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All)
    => Validate(filter, maxLength, options, null, null);

    /// <summary>
    /// Validates the specified <see cref="StringFilter"/> with an optional single-value custom validator.
    /// Returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator for validating individual string values within the filter.
    /// </param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => Validate(filter, null, options, customValidator, null);

    /// <summary>
    /// Validates the specified <see cref="StringFilter"/> with an optional array-value custom validator.
    /// Returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <param name="customArrayValidator">
    /// An optional user-defined validator for validating entire string arrays within the filter.
    /// </param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    => Validate(filter, null, options, null, customArrayValidator);

    /// <summary>
    /// Validates the specified <see cref="StringFilter"/> with an optional maximum length and single-value custom validator.
    /// Returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="maxLength">An optional maximum allowed length for string values.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator for validating individual string values within the filter.
    /// </param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => Validate(filter, maxLength, options, customValidator, null);

    /// <summary>
    /// Validates the specified <see cref="StringFilter"/> with an optional maximum length and array-value custom validator.
    /// Returns any validation errors found.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="maxLength">An optional maximum allowed length for string values.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <param name="customArrayValidator">
    /// An optional user-defined validator for validating entire string arrays within the filter.
    /// </param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    => Validate(filter, maxLength, options, null, customArrayValidator);

    /// <summary>
    /// Core validation method for <see cref="StringFilter"/> with support for maximum length,
    /// single-value and array-value custom validators.
    /// 
    /// Note: Both single-value (<paramref name="customValidator"/>) and array-value (<paramref name="customArrayValidator"/>) validators
    /// can be supplied. The single-value validator will be applied to each string and each element within arrays, while the array-value validator
    /// will be applied to the entire array.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="maxLength">An optional maximum allowed length for string values.</param>
    /// <param name="options">Validation options controlling nullability and empty string allowance.</param>
    /// <param name="customValidator">
    /// An optional user-defined validator for validating individual string values within the filter and within arrays.
    /// </param>
    /// <param name="customArrayValidator">
    /// An optional user-defined validator for validating entire string arrays within the filter.
    /// </param>
    /// <returns>A collection of field-error pairs indicating validation errors, if any.</returns>
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    => CommonValidator.ValidateNullOr(
        filter,
        options.HasFlag(StringFilterValidationOptions.AllowNullable),
        filter => ValidateInner(
            filter,
            maxLength,
            options.HasFlag(StringFilterValidationOptions.AllowEmptyStrings),
            customValidator,
            customArrayValidator));

    private static IEnumerable<(string Field, string Error)> ValidateInner(
        StringFilter filter,
        int? maxLength = null,
        bool allowEmptyStrings = true,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    {
        var stringFields = new (string? Value, string Name)[]
            {
                (filter.Eq, nameof(filter.Eq)),
                (filter.NotEq, nameof(filter.NotEq)),
                (filter.Contains, nameof(filter.Contains)),
                (filter.StartsWith, nameof(filter.StartsWith)),
                (filter.EndsWith, nameof(filter.EndsWith)),
            };

        foreach (var (value, name) in stringFields)
        {
            foreach (var error in CheckString(value, name, allowEmptyStrings, customValidator, maxLength))
                yield return error;
        }

        var arrayFields = new (string[]? Value, string Name)[]
        {
            (filter.In, nameof(filter.In)),
            (filter.NotIn, nameof(filter.NotIn)),
        };

        foreach (var (value, name) in arrayFields)
        {
            foreach (var error in CheckStringArray(value, name, allowEmptyStrings, customValidator, customArrayValidator, maxLength))
                yield return error;
        }
    }

    private static IEnumerable<(string Field, string Error)> CheckStringArray(
        string[]? values,
        string fieldName,
        bool allowEmptyStrings,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null,
        int? maxLength = null)
    {
        if (values is null || values.Length == 0) yield break;

        for (int i = 0; i < values.Length; i++)
        {
            foreach (var error in CheckString(
                values[i],
                $"{fieldName}[{i}]",
                allowEmptyStrings,
                customValidator,
                maxLength))
            {
                yield return error;
            }
        }

        foreach (var error in ValidateViaCustomValidatorIfNeeded<string[]>(values, fieldName, customArrayValidator))
            yield return error;
    }

    private static IEnumerable<(string Field, string Error)> CheckString(
        string? value,
        string fieldName,
        bool allowEmptyStrings,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null,
        int? maxLength = null)
    {
        if (value is null) yield break;

        if (!allowEmptyStrings && value.Trim().Length == 0)
            yield return (fieldName, "Value cannot be empty");

        if (maxLength.HasValue && value.Length > maxLength)
            yield return (fieldName, $"Value exceeds max length of {maxLength.Value}");

        foreach (var error in ValidateViaCustomValidatorIfNeeded(value, fieldName, customValidator))
            yield return error;
    }

    private static IEnumerable<(string Field, string Error)> ValidateViaCustomValidatorIfNeeded<T>(
        T value,
        string fieldName,
        Func<T, (bool IsValid, string? ErrorMessage)>? customValidator)
    {
        if (customValidator is null)
            yield break;

        var result = customValidator(value);
        if (!result.IsValid)
            yield return (fieldName, result.ErrorMessage ?? "Filter failed custom validation");
    }

    /// <summary>
    /// Ensures that the specified <see cref="StringFilter"/> is valid for binding to the specified table and column.
    /// Throws an <see cref="ArgumentException"/> if the filter is invalid.
    /// This method is used for internal validations within the <c>FlectoBuilder</c>.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> to validate.</param>
    /// <param name="table">The table associated with the filter.</param>
    /// <param name="column">The column associated with the filter.</param>
    internal static void EnsureValid(StringFilter filter, string table, string column)
    => CommonValidator.EnsureValidBindFilter(
        filter,
        table,
        column,
        f => Validate(f, null, StringFilterValidationOptions.AllowEmptyStrings));
}

// for docs 
// with FluentValidation
// RuleFor(x => x)
//     .Custom((filter, context) =>
//     {
//         var errors = StringValidator.Validate(
//             filter,
//             maxLength: 100);
// 
//         foreach (var error in errors)
//         {
//             context.AddFailure(error.Field, error.Error);
//         }
//     });
//
//  without
//  var errors = GetStringFilterValidationErrors(
//      filter,
//      maxLength: 100,
//      disallowEmptyStrings: true,
//      customStringValidator: value =>
//      {
//          if (value.Contains("forbidden"))
//              return (false, "Value contains forbidden word.");
//          return (true, null);
//      });
//      
//      if (errors.Count > 0)
//      {
//          foreach (var error in errors)
//          {
//              Console.WriteLine($"{error.Field}: {error.Error}");
//          }
//      
//          // Или выбросить исключение:
//          throw new ValidationException("StringFilter validation failed", errors.Select(e => new ValidationFailure(e.Field, e.Error)));
//      }
