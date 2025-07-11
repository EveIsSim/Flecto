namespace Flecto.Core.Models.Filters;

/// <summary>
/// Represents a text search filter for queries
/// </summary>
public class SearchFilter : IFilter
{
    /// <summary>
    /// Gets or sets the search value to match in the query
    /// </summary>
    public required string Value { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the search should be case-sensitive
    /// </summary>
    public bool CaseSensitive { get; set; }
}
