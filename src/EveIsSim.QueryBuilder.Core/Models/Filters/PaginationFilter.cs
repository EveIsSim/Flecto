namespace EveIsSim.QueryBuilder.Core.Models.Filters;

/// <summary>
/// Represents pagination settings for a query
/// </summary>
public class PaginationFilter
{
    /// <summary>
    /// Gets or sets the maximum number of items to return in the query results
    /// </summary>
    public int Limit { get; set; }
    /// <summary>
    /// Gets or sets the page number of the query results
    /// </summary>
    public int Page { get; set; }
}
