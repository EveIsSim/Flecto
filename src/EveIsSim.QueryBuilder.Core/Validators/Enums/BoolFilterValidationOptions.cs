namespace EveIsSim.QueryBuilder.Core.Validators.Enums;

[Flags]
public enum BoolFilterValidationOptions
{
    None = 0,
    AllowNullable = 1 << 0,
    RequireAtLeastOne = 1 << 1,

    // 999 add tests
    All = AllowNullable | RequireAtLeastOne
}
