namespace Flecto.Core.Models.Filters;

/// <summary>
/// Represents a filter for boolean values in queries
/// </summary>
public class BoolFilter : IQueryFilter
{
    /// <summary>
    /// Gets or sets a value indicating that the property should be equal to the specified boolean value
    /// </summary>
    public bool? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified boolean value
    /// </summary>
    public bool? NotEq { get; set; }
    /// <summary>
    /// Gets ot sets a value indicating whether to filter for null values
    /// </summary>
    public bool? Null { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
