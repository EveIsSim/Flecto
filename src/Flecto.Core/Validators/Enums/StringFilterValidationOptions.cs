namespace Flecto.Core.Validators.Enums;

/// <summary>
/// Specifies validation options for a <see cref="Flecto.Core.Models.Filters.StringFilter"/>
/// </summary>
[Flags]
public enum StringFilterValidationOptions
{
    /// <summary>
    /// No validation options are applied
    /// </summary>
    None = 0,
    /// <summary>
    /// Allows the filter to accept empty strings
    /// </summary>
    AllowEmptyStrings = 1 << 0,
    /// <summary>
    /// Allows the filter to accept null values
    /// </summary>
    AllowNullable = 1 << 1,

    // 999 add tests

    /// <summary>
    /// All validation options are applied
    /// </summary>
    All = AllowNullable | AllowNullable
}
