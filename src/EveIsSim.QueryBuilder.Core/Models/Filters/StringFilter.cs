namespace EveIsSim.QueryBuilder.Core.Models.Filters;

/// <summary>
/// Represents a filter for string values in queries
/// </summary>
public class StringFilter : IQueryFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether string comparisons should be case-sensitive
    /// </summary>
    public bool CaseSensitive { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be equal to the specified string
    /// </summary>
    public string? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified string
    /// </summary>
    public string? NotEq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should contain the specified substring
    /// </summary>
    public string? Contains { get; set; }
    /// <summary>
    /// Gets or sets an array of strings to filter for inclusion
    /// </summary>
    public string[]? In { get; set; }
    /// <summary>
    /// Gets or sets an array of strings to filter for exclusion
    /// </summary>
    public string[]? NotIn { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? Null { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should start with the specified substring
    /// </summary>
    public string? StartsWith { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should end with the specified substring
    /// </summary>
    public string? EndsWith { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
