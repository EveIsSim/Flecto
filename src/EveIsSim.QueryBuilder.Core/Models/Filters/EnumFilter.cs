using EveIsSim.QueryBuilder.Core.Models.Filters.Enums;

namespace EveIsSim.QueryBuilder.Core.Models.Filters;

// tests: check enum<T> byte, long, int, short. Default(int)
public class EnumFilter<T> : IQueryFilter where T : struct, Enum
{
    public T? Eq { get; set; }
    public T? NotEq { get; set; }
    public T[]? In { get; set; }
    public T[]? NotIn { get; set; }
    public bool? Null { get; set; }
    public EnumFilterMode FilterMode { get; set; }
    public Sort? Sort { get; set; }
}
