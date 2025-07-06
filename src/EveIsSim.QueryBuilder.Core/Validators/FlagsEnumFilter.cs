using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class FlagsEnumFilter
{
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        FlagsEnumFilter<T> filter,
        bool allowNullable = true)
    where T : struct, Enum
    => Validate(filter, allowNullable, null);

    public static IEnumerable<(string Field, string Error)> Validate<T>(
    FlagsEnumFilter<T> filter,
    bool allowNullable = true,
    Func<FlagsEnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    where T : struct, Enum
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInternal(filter, customValidator));

    public static IEnumerable<(string Field, string Error)> ValidateInternal<T>(
    FlagsEnumFilter<T> filter,
    Func<FlagsEnumFilter<T>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    where T : struct, Enum
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(FlagsEnumFilter<T>)))
            yield return error;

        if (filter.HasFlag.HasValue && filter.NotHasFlag.HasValue)
            yield return (nameof(FlagsEnumFilter<T>), "Cannot specify HasFlag and NotHasFlag simultaneously");

        foreach (var error in CommonValidator.ValidateViaCustomValidatorIfNeeded(filter, customValidator))
            yield return error;
    }

    internal static void EnsureValid<T>(FlagsEnumFilter<T> filter, string table, string column)
        where T : struct, Enum
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}

// without Fluent
// var errors = GetFlagsEnumFilterValidationErrors(filter);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "FlagsEnumFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
// with FluentV
// RuleFor(x => x.MyFlagsEnumFilter)
//     .Custom((filter, context) =>
//     {
//     var errors = GetFlagsEnumFilterValidationErrors(filter);
// 
//     foreach (var error in errors)
//     {
//         context.AddFailure(error.Field, error.Error);
//     }
// });
