using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Validators;


public static class NumericValidator
{
    public static IEnumerable<(string Field, string Error)> Validate<T>(
        NumericFilter<T> filter,
        bool allowNullable = true)
        where T : struct, IComparable
    => Validate<T>(filter, allowNullable, null);

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

    internal static void EnsureValid<T>(NumericFilter<T> filter, string table, string column)
        where T : struct, IComparable
    => CommonValidator.EnsureValidBindFilter(filter, table, column, f => Validate(f, false));
}


// for doc
// without FluentV
// var errors = GetNumericFilterValidationErrors(filter);
// 
// if (errors.Count > 0)
// {
//     foreach (var error in errors)
//     {
//         Console.WriteLine($"{error.Field}: {error.Error}");
//     }
// 
//     throw new ValidationException(
//         "NumericFilter validation failed",
//         errors.Select(e => new ValidationFailure(e.Field, e.Error)));
// }
//
// with FluentV
//RuleFor(x => x.MyNumericFilter)
//    .Custom((filter, context) =>
//    {
//        var errors = GetNumericFilterValidationErrors(filter);
//
//        foreach (var error in errors)
//        {
//            context.AddFailure(error.Field, error.Error);
//        }
//    });
//
