namespace EveIsSim.QueryBuilder.Core.Models.Filters;

public interface IQueryFilter : IFilter
{
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
