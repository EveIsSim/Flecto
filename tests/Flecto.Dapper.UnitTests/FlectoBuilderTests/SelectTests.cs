using Flecto.Core.Enums;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class SelectTests
{
    #region SelectAll

    private const string Table = "users";

    [Fact]
    public void SelectAll_SetsSelectClauseCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .SelectAll()
            .Build();

        // Assert
        Assert.Equal("SELECT * FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void SelectAll_SecondCall_Throws()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .SelectAll();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(builder.SelectAll);

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    #endregion

    #region SelectCount

    [Fact]
    public void SelectCount_SetsSelectClauseCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .SelectCount()
            .Build();

        // Assert
        Assert.Equal("SELECT COUNT(*) FROM users", result.Sql);
    }

    [Fact]
    public void SelectCount_SecondCall_Throws()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .SelectCount();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(builder.SelectAll);

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    #endregion

    #region Select

    [Fact]
    public void Select_Columns_SetsSelectClauseCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, [new("id"), new("salary")]);

        // Act
        var result = builder
            .Select(tc)
            .Build();

        Assert.Equal("SELECT users.id, users.salary FROM users", result.Sql);
    }

    [Fact]
    public void Select_ColumnsWithJson_SetsSelectClauseCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(
            Table,
            [
                new("social->>'platform'", "social_platform"),
                new("social->'github'"),
                new("profile->'personal'->>'full_name'", "profile_personal_full_name")]);

        // Act
        var result = builder
            .Select(tc)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.social->>'platform' AS social_platform, " +
            "users.social->'github', " +
            "users.profile->'personal'->>'full_name' AS profile_personal_full_name " +
            "FROM users",
            result.Sql);
    }

    [Fact]
    public void Select_SecondCall_Throws()
    {
        // Arrange
        var tc = new FromTable(Table, [new("id")]);

        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(tc);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Select(tc));

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    #endregion
}
