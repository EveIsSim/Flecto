namespace EveIsSim.QueryBuilder.Core.Validators.Enums;

[Flags]
public enum StringFilterValidationOptions
{
    None = 0,
    AllowEmptyStrings = 1 << 0,
    AllowNullable = 1 << 1,

    // 999 add tests
    All = AllowNullable | AllowNullable
}
