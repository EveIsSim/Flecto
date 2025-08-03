namespace Flecto.Core.Models.Filters;

// 999 tests: cover enum flags 

/// <summary>
/// Represents a filter for enumeration values with flags in queries
/// </summary>
/// <typeparam name="T">
/// The enumeration type to filter, which must be a struct and an <see cref="Enum"/>
/// </typeparam>
public class FlagsEnumFilter<T> : IQueryFilter where T : struct, Enum
{
    /// <summary>
    /// Gets or sets value indicating that the property should be uqual to the specified enumeration value
    /// </summary>
    public T? Eq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the property should not be equal to the specified enumeration value
    /// </summary>
    public T? NotEq { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the specified flag should be set on the property
    /// </summary>
    public T? HasFlag { get; set; }
    /// <summary>
    /// Gets or sets a value indicating that the specified flag should not be set on the property
    /// </summary>
    public T? NotHasFlag { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to filter for null values
    /// </summary>
    public bool? IsNull { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
