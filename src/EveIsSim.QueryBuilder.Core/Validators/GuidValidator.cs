using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class GuidValidator
{
    public static IEnumerable<(string Field, string Error)> Validate(
        GuidFilter filter,
        bool allowNullable = true)
    => Validate(filter, allowNullable, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        GuidFilter filter,
        bool allowNullable = true,
        Func<GuidFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
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

    internal static void EnsureValid(GuidFilter filter, string table, string column)
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}

// For Doc
// without FluentValidation
// var errors = GetGuidFilterValidationErrors(filter);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "GuidFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
// with FluentValidation
// RuleFor(x => x.GuidFilter)
//    .Custom((filter, context) =>
//    {
//    var errors = GetGuidFilterValidationErrors(filter);
//
//    foreach (var error in errors)
//    {
//        context.AddFailure(error.Field, error.Error);
//    }
// });
//
//
//
