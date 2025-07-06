namespace EveIsSim.QueryBuilder.Core.Models.Filters;


public class NumericFilter<T> : IQueryFilter where T : struct, IComparable
{
    public T? Eq { get; set; }
    public T? NotEq { get; set; }
    public T? Gt { get; set; }
    public T? Gte { get; set; }
    public T? Lt { get; set; }
    public T? Lte { get; set; }
    public T[]? In { get; set; }
    public T[]? NotIn { get; set; }
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
