using EveIsSim.QueryBuilder.Core.Models.Filters;
using EveIsSim.QueryBuilder.Core.Validators.Enums;

namespace EveIsSim.QueryBuilder.Core.Validators;

public static class StringValidator
{
    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All)
    => Validate(filter, maxLength, options, null, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => Validate(filter, null, options, customValidator, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    => Validate(filter, null, options, null, customArrayValidator);

    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => Validate(filter, maxLength, options, customValidator, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        StringFilter filter,
        int? maxLength = null,
        StringFilterValidationOptions options = StringFilterValidationOptions.All,
        Func<string[], (bool IsValid, string? ErrorMessage)>? customArrayValidator = null)
    => Validate(filter, maxLength, options, null, customArrayValidator);

    // write why 2 validators, that customValidator will also be called for each element in the array
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
//         var errors = GetStringFilterValidationErrors(
//             filter,
//             maxLength: 100,
//             disallowEmptyStrings: true);
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
