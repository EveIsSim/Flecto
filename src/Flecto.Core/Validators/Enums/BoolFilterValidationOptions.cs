namespace Flecto.Core.Validators.Enums;

/// <summary>
/// Specifies validation options for a <see cref="Flecto.Core.Models.Filters.BoolFilter"/>
/// </summary>
[Flags]
public enum BoolFilterValidationOptions
{
    /// <summary>
    /// No validation options are applied
    /// </summary>
    None = 0,
    /// <summary>
    /// Allows the filter to accept nullable values
    /// </summary>
    AllowNullable = 1 << 0,
    /// <summary>
    /// Requires that at least one filter condition is specified
    /// </summary>
    RequireAtLeastOne = 1 << 1,

    // 999 add tests
    /// <summary>
    /// All validation options are applied
    /// </summary>
    All = AllowNullable | RequireAtLeastOne
}
