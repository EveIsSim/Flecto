namespace Flecto.Core.Models.Filters;

// 999 tests: if someone will change format (check it)

/// <summary>
/// Represents a filter for <see cref="DateTime"/> values in queries
/// </summary>
public class DateFilter : IQueryFilter
{
    /// <summary>
    /// Gets or sets a value indicating that the property should be equal to the specified date value
    /// </summary>
    public DateTime? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified date value
    /// </summary>
    public DateTime? NotEq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the propery should be greater than the specified date
    /// </summary>
    public DateTime? Gt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be greater than or equal to the specified date
    /// </summary>
    public DateTime? Gte { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should less than the specified date
    /// </summary>
    public DateTime? Lt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be less than ot equal to the specified date
    /// </summary>
    public DateTime? Lte { get; set; }
    /// <summary>
    /// Gets or sets an array of dates to filter for inclusion
    /// </summary>
    public DateTime[]? In { get; set; }
    /// <summary>
    /// Gets or sets an array of dates to filter for exclusion
    /// </summary>
    public DateTime[]? NotIn { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? IsNull { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
