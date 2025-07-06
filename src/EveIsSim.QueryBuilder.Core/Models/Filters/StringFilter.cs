namespace EveIsSim.QueryBuilder.Core.Models.Filters;


public class StringFilter : IQueryFilter
{
    public bool CaseSensitive { get; set; }
    public string? Eq { get; set; }
    public string? NotEq { get; set; }
    public string? Contains { get; set; }
    public string[]? In { get; set; }
    public string[]? NotIn { get; set; }
    public bool? Null { get; set; }
    public string? StartsWith { get; set; }
    public string? EndsWith { get; set; }
    public Sort? Sort { get; set; }
}
