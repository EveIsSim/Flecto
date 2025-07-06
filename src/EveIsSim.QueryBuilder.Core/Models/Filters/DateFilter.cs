namespace EveIsSim.QueryBuilder.Core.Models.Filters;

// tests: if someone will change format (check it)
public class DateFilter : IQueryFilter
{
    public DateTime? Eq { get; set; }
    public DateTime? NotEq { get; set; }
    public DateTime? Gt { get; set; }
    public DateTime? Gte { get; set; }
    public DateTime? Lt { get; set; }
    public DateTime? Lte { get; set; }
    public DateTime[]? In { get; set; }
    public DateTime[]? NotIn { get; set; }
    public bool? Null { get; set; }
    public Sort? Sort { get; set; }
}
