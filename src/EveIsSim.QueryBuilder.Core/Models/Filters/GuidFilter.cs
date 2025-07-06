namespace EveIsSim.QueryBuilder.Core.Models.Filters;

public class GuidFilter : IQueryFilter
{
    public Guid? Eq { get; set; }
    public Guid? NotEq { get; set; }
    public Guid[]? In { get; set; }
    public Guid[]? NotIn { get; set; }
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
