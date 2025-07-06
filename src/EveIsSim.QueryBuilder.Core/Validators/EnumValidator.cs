using EveIsSim.QueryBuilder.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class EnumValidator
{
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        EnumFilter<T> filter,
        bool allowNullable = true)
    where T : struct, Enum
    => Validate(filter, allowNullable, null);

    public static IEnumerable<(string Field, string Error)> Validate<T>(
        EnumFilter<T> filter,
        bool allowNullable = true,
        Func<EnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
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

    internal static void EnsureValid<T>(EnumFilter<T> filter, string table, string column)
        where T : struct, Enum
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}

// For docs
// without FluentValidation
// var errors = GetEnumFilterValidationErrors(filter);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "EnumFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
// with FluentValidation
// RuleFor(x => x.MyEnumFilter)
//    .Custom((filter, context) =>
//    {
//    var errors = GetEnumFilterValidationErrors(filter);
//
//    foreach (var error in errors)
//    {
//        context.AddFailure(error.Field, error.Error);
//    }
// });
//
