namespace EveIsSim.QueryBuilder.Core.Models.Filters.Enums;

/// <summary>
/// Specifies the mode to use when filtering enumeration values
/// </summary>
public enum EnumFilterMode
{
    /// <summary>
    /// Filters using the name of the enumeration member 
    /// </summary>
    Name,
    /// <summary>
    /// Filters using the numeric value of the enumeration member 
    /// </summary>
    Value,
    /// <summary>
    /// Filters using the string representation of the numeric value 
    /// </summary>
    ValueString
}
