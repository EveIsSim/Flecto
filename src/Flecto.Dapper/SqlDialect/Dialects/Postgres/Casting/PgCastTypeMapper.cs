using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.Models;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting.Enums;

namespace Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting;

/// <summary>
/// Provides PostgreSQL-specific SQL casting logic for mapping CLR types to PostgreSQL SQL types
/// and generating SQL-compatible column references with explicit type casts.
/// </summary>
internal class PgCastTypeMapper : ICastTypeMapper
{
    /// <summary>
    /// Returns the SQL-compatible column reference with type casting based on the provided CLR type.
    /// </summary>
    /// <param name="columnRef">The column reference to be cast.</param>
    /// <param name="clrType">The CLR type used to determine the SQL cast type.</param>
    /// <returns>SQL string with appropriate PostgreSQL cast, e.g., <c>(table.column->>jsonb_column)::boolean</c>.</returns>
    public string GetSqlName(ColumnRef columnRef, Type clrType)
    {
        var castType = FromClrType(clrType);
        return GetSqlName(columnRef, castType);
    }

    /// <summary>
    /// Returns the SQL-compatible column reference with casting based on the enum filter mode.
    /// </summary>
    /// <param name="columnRef">The column reference to be cast.</param>
    /// <param name="mode">The enum filter mode determining how the enum should be interpreted in SQL.</param>
    /// <returns>SQL string with appropriate cast for enum filtering.</returns>
    public string GetSqlNameForEnum(ColumnRef columnRef, EnumFilterMode mode)
    {
        var castType = mode switch
        {
            EnumFilterMode.Value => CastType.Integer,
            EnumFilterMode.ValueString => CastType.Text,
            EnumFilterMode.Name => CastType.Text,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        return GetSqlName(columnRef, castType);
    }

    /// <summary>
    /// Builds the SQL-compatible column reference with cast.
    /// </summary>
    /// <param name="cr">The column reference.</param>
    /// <param name="castType">The desired PostgreSQL cast type.</param>
    /// <returns>SQL string with cast applied. We will cast if column has json path</returns>
    private static string GetSqlName(ColumnRef cr, CastType castType)
    {
        var pgType = ToPostgresCastName(castType);
        return cr.IsJsonPath
            ? $"({cr.Table}.{cr.Column})::{pgType}"
            : $"{cr.Table}.{cr.Column}";
    }

    /// <summary>
    /// Maps a CLR type to a PostgreSQL-specific cast type.
    /// </summary>
    /// <param name="type">CLR type to map.</param>
    /// <returns>Equivalent <see cref="CastType"/> used for PostgreSQL casting.</returns>
    /// <exception cref="NotSupportedException">Thrown if type is not supported.</exception>
    private static CastType FromClrType(Type type)
    {
        if (type == typeof(bool)) return CastType.Boolean;
        if (type == typeof(string)) return CastType.Text;
        if (type == typeof(DateTime)) return CastType.Timestamp;
        if (type == typeof(DateOnly)) return CastType.Date;
        if (type == typeof(Guid)) return CastType.Guid;

        if (type == typeof(short)) return CastType.SmallInt;
        if (type == typeof(int)) return CastType.Integer;
        if (type == typeof(long)) return CastType.BigInt;
        if (type == typeof(float)) return CastType.Float4;
        if (type == typeof(double)) return CastType.Float8;
        if (type == typeof(decimal)) return CastType.Numeric;

        throw new NotSupportedException($"Type {type.Name} is not supported for SQL casting.");
    }

    /// <summary>
    /// Maps a <see cref="CastType"/> to its corresponding PostgreSQL type name.
    /// </summary>
    /// <param name="type">The logical cast type.</param>
    /// <returns>PostgreSQL type name string, e.g., <c>int4</c>, <c>uuid</c>, <c>boolean</c>.</returns>
    /// <exception cref="NotSupportedException">Thrown if cast type is unknown.</exception>
    private static string ToPostgresCastName(CastType type) => type switch
    {
        CastType.Boolean => "boolean",
        CastType.SmallInt => "int2",
        CastType.Integer => "int4",
        CastType.BigInt => "int8",
        CastType.Float4 => "float4",
        CastType.Float8 => "float8",
        CastType.Numeric => "numeric",
        CastType.Text => "text",
        CastType.Date => "date",
        CastType.Timestamp => "timestamptz",
        CastType.Guid => "uuid",
        _ => throw new NotSupportedException($"Unsupported PostgreSQL cast type: {type}")
    };
}
