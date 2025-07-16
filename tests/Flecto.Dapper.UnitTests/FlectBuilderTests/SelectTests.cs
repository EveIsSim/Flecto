using Flecto.Core.Enums;

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
        var ex = Assert.Throws<InvalidOperationException>(() => builder.SelectAll());

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
        var ex = Assert.Throws<InvalidOperationException>(() => builder.SelectAll());

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    #endregion

    #region Select

    [Fact]
    public void Select_Columns_SetsSelectClauseCorrectly()
    {
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select("id", "salary")
            .Build();

        Assert.Equal("SELECT users.id, users.salary FROM users", result.Sql);
    }

    [Fact]
    public void Select_ColumnsWithJson_SetsSelectClauseCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(
                "social->>'platform'",
                "social->'github'",
                "profile->'personal'->>'full_name'"
            )
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.social->>'platform', " +
            "users.social->'github', " +
            "users.profile->'personal'->>'full_name' " +
            "FROM users",
            result.Sql);
    }

    [Fact]
    public void Select_SecondCall_Throws()
    {
        // Arrange
        var column = "id";
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(column);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Select(column));

        // Assert
        Assert.Equal("Select can only be called once per query", ex.Message);
    }

    #endregion
}
