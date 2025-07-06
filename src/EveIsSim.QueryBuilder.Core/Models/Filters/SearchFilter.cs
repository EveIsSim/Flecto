namespace EveIsSim.QueryBuilder.Core.Models.Filters;


public class SearchFilter : IFilter
{
    public required string Value { get; set; }
    public bool CaseSensitive { get; set; }
}
