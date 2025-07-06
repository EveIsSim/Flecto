using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class DateValidator
{
    public static IEnumerable<(string Field, string Error)> Validate(
        DateFilter filter,
        bool allowNullable = true)
    => Validate(filter, allowNullable, null);

    public static IEnumerable<(string Field, string Error)> Validate(
        DateFilter filter,
        bool allowNullable = true,
        Func<DateFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    => CommonValidator.ValidateNullOr(
        filter,
        allowNullable,
        filter => ValidateInner(filter, customValidator));

    private static IEnumerable<(string Field, string Error)> ValidateInner(
        DateFilter filter,
        Func<DateFilter, (bool IsValid, string? ErrorMessage)>? customValidator = null)
    {
        foreach (var error in CommonValidator.ValidateEqAndNotEq(filter.Eq, filter.NotEq, nameof(DateFilter)))
            yield return error;

        foreach (var error in CommonValidator.ValidateRangeConsistency(
            filter.Gt,
            filter.Gte,
            filter.Lt,
            filter.Lte,
            nameof(DateFilter)))
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

    internal static void EnsureValid(DateFilter filter, string table, string column)
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}


// for docs
// with fluentValidation
//RuleFor(x => x.DateFilter)
//    .Custom((filter, context) =>
//    {
//        var errors = GetDateFilterValidationErrors(filter);
//
//        foreach (var error in errors)
//        {
//            context.AddFailure(error.Field, error.Error);
//        }
//    });
//
// without 
// var errors = GetDateFilterValidationErrors(filter);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "DateFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
