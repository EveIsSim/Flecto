namespace EveIsSim.QueryBuilder.Core.Models.Filters;

// tests: cover enum flags 
public class FlagsEnumFilter<T> : IQueryFilter where T : struct, Enum
{
    public T? Eq { get; set; }
    public T? NotEq { get; set; }
    public T? HasFlag { get; set; }
    public T? NotHasFlag { get; set; }
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
