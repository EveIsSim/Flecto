using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.Models;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting;

namespace Flecto.Dapper.UnitTests.CastingTests;


public class PgCastTypeMapperTests
{
    private readonly PgCastTypeMapper _mapper = new();

    [Theory]
    [InlineData(typeof(bool), "is_active", "users.is_active")]
    [InlineData(typeof(bool), "data->>'is_active'", "(users.data->>'is_active')::boolean")]
    [InlineData(typeof(int), "json->'age'", "(users.json->'age')::int4")]
    [InlineData(typeof(string), "profile->'name'", "(users.profile->'name')::text")]
    public void GetSqlName_CorrectlyAppliesCast_OnlyForJsonPath(Type clrType, string column, string expected)
    {
        // Arrange
        var cr = new ColumnRef("users", column, 0, _mapper);

        // Act
        var result = _mapper.GetSqlName(cr, clrType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(EnumFilterMode.Value, "role", "users.role")]
    [InlineData(EnumFilterMode.ValueString, "role", "users.role")]
    [InlineData(EnumFilterMode.Name, "role", "users.role")]
    [InlineData(EnumFilterMode.Value, "social->>net", "(users.social->>net)::int4")]
    [InlineData(EnumFilterMode.ValueString, "social->>net", "(users.social->>net)::text")]
    [InlineData(EnumFilterMode.Name, "social->>net", "(users.social->>net)::text")]
    public void GetSqlNameForEnum_ReturnsCorrectSql(EnumFilterMode mode, string column, string expected)
    {
        // Arrange
        var cr = new ColumnRef("users", column, 0, _mapper);

        // Act
        var result = _mapper.GetSqlNameForEnum(cr, mode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSqlName_Throws_ForUnsupportedType()
    {
        // Arrange
        var cr = new ColumnRef("users", "unsupported_column", 0, _mapper);

        // Act
        var ex = Assert.Throws<NotSupportedException>(() =>
            _mapper.GetSqlName(cr, typeof(TimeSpan)));

        // Assert
        Assert.Contains("Type TimeSpan is not supported", ex.Message);
    }
}
