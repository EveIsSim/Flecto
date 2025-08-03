namespace Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting.Enums;

internal enum CastType
{
    Boolean,
    SmallInt,   // short → int2
    Integer,    // int → int4
    BigInt,     // long → int8
    Float4,     // float → float4
    Float8,     // double → float8
    Numeric,    // decimal → numeric
    Text,
    Date,
    Timestamp,
    Guid
}
