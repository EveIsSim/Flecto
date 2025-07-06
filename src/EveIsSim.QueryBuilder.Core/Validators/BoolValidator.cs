using EveIsSim.QueryBuilder.Core.Models.Filters;
using EveIsSim.QueryBuilder.Core.Validators.Enums;

namespace EveIsSim.QueryBuilder.Core.Validators;

public static class BoolValidator
{
    public static IEnumerable<(string Field, string Error)> Validate(
        BoolFilter? filter,
        BoolFilterValidationOptions options = BoolFilterValidationOptions.None)
    => Validate(filter, options, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        BoolFilter? filter,
        BoolFilterValidationOptions options = BoolFilterValidationOptions.None,
        Func<BoolFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => CommonValidator.ValidateNullOr(
        filter,
        options.HasFlag(BoolFilterValidationOptions.AllowNullable),
        filter => ValidateInner(filter, options, customValidator));

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
