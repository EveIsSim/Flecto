namespace EveIsSim.QueryBuilder.Core.Models.Filters;

/// <summary>
/// Represents a filter for <see cref="Guid"/> values in queries
/// </summary>
public class GuidFilter : IQueryFilter
{
    /// <summary>
    /// Gets or sets a value indicating that the property should be equal to the specified GUID
    /// </summary>
    public Guid? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified GUID
    /// </summary>
    public Guid? NotEq { get; set; }
    /// <summary>
    /// Gets or sets an array of GUIDs to filter for inclusion
    /// </summary>
    public Guid[]? In { get; set; }
    /// <summary>
    /// Gets or sets an array of GUIDs to filter for exclusion
    /// </summary>
    public Guid[]? NotIn { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? Null { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
