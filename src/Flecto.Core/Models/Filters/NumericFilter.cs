namespace Flecto.Core.Models.Filters;

/// <summary>
/// Represents a filter for numeric values in queries
/// </summary>
/// <typeparam name="T">
/// The numeric type to filter, which must be a struct and implement <see cref="IComparable"/>
/// </typeparam>
public class NumericFilter<T> : IQueryFilter where T : struct, IComparable
{
    /// <summary>
    /// Gets or sets a value indicating that the property should be equal to the specified value
    /// </summary>
    public T? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified value
    /// </summary>
    public T? NotEq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be greater than the specified value
    /// </summary>
    public T? Gt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be greater than or equal to the specified value
    /// </summary>
    public T? Gte { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be less than the specified value
    /// </summary>
    public T? Lt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should be less than or equal to the specified value
    /// </summary>
    public T? Lte { get; set; }
    /// <summary>
    /// Gets or sets an array of values to filter for inclusion
    /// </summary>
    public T[]? In { get; set; }
    /// <summary>
    /// Gets or sets an array of values to filter for exclusion
    /// </summary>
    public T[]? NotIn { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? IsNull { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
