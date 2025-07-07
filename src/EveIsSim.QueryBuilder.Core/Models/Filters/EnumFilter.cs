using EveIsSim.QueryBuilder.Core.Models.Filters.Enums;

namespace EveIsSim.QueryBuilder.Core.Models.Filters;

// 999 tests: check enum<T> byte, long, int, short. Default(int)

/// <summary>
/// Represents a filter for enumeration values in queries
/// </summary>
/// <typeparam name="T">
/// The enumeration type of filter, which must be a struct and an <see cref="Enum"/>
/// </typeparam>
public class EnumFilter<T> : IQueryFilter where T : struct, Enum
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
    /// Gets or sets an array of enumeration values to filter for inclusion
    /// </summary>
    public T[]? In { get; set; }
    /// <summary>
    /// Gets or sets an array of enumeration values to filter for exclusion 
    /// </summary>
    public T[]? NotIn { get; set; }
    /// <summary>
    /// Gets or sets a value idicating whether to filter for null values 
    /// </summary>
    public bool? Null { get; set; }
    /// <summary>
    /// Gets or sets the mode used for filtering enumeration values
    /// </summary>
    public EnumFilterMode FilterMode { get; set; }
    /// <summary>
    /// Gets or sets the sort direction for the filtered property
    /// </summary>
    public Sort? Sort { get; set; }
}
