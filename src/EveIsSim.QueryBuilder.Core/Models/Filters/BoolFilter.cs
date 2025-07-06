namespace EveIsSim.QueryBuilder.Core.Models.Filters;

public class BoolFilter : IQueryFilter
{
    public bool? Eq { get; set; }
    public bool? NotEq { get; set; }
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
