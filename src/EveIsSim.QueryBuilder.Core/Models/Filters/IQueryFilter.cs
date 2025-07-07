namespace EveIsSim.QueryBuilder.Core.Models.Filters;

/// <summary>
/// Represents a filter that can be applied in queries with null checking and sorting support
/// </summary>
public interface IQueryFilter : IFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? Null { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
