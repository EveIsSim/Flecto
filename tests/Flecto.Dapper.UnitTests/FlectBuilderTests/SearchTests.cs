using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class SearchTests
{
    private const string Table = "users";

    #region Search

    [Fact]
    public void Search_WithNullFilter_DoesNotAddCondition()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .Search(null, "name", "email")
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void Search_WithEmptyColumns_Throws()
    {
        // Arrange
        var filter = new SearchFilter { Value = "test", CaseSensitive = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder.Search(filter, new string[0]));

        // Assert
        Assert.Equal("Table 'users' must have at least one column specified", ex.Message);
    }

    [Fact]
    public void Search_WithValidFilter_AddsILikeCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "joHn", CaseSensitive = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .Search(filter, "name", "email")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE (" +
                "users.name ILIKE @search_param_0 " +
                "OR users.email ILIKE @search_param_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("search_param_0"));
        Assert.Equal("%joHn%", paramDict["search_param_0"]);
    }

    [Fact]
    public void Search_CaseSensitive_AddsLikeCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "Admin", CaseSensitive = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id"), new("name"), new("role") });

        // Act
        var result = builder
            .Select(tc)
            .Search(filter, "role")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id, users.name, users.role " +
            "FROM users " +
            "WHERE (" +
                "users.role LIKE @search_param_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("search_param_0"));
        Assert.Equal("%Admin%", paramDict["search_param_0"]);
    }

    [Fact]
    public void Search_CalledTwice_AddsTwoConditions()
    {
        // Arrange
        var filter1 = new SearchFilter { Value = "john", CaseSensitive = false };
        var filter2 = new SearchFilter { Value = "Admin", CaseSensitive = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .Search(filter1, "name", "last_name")
            .Search(filter2, "role", "additional_role")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE (" +
                "users.name ILIKE @search_param_0 " +
                "OR users.last_name ILIKE @search_param_0) " +
            "AND (" +
                "users.role LIKE @search_param_1 " +
                "OR users.additional_role LIKE @search_param_1)",
            result.Sql);


        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey("search_param_0"));
        Assert.Equal("%john%", paramDict["search_param_0"]);

        Assert.True(paramDict.ContainsKey("search_param_1"));
        Assert.Equal("%Admin%", paramDict["search_param_1"]);
    }

    [Fact]
    public void Search_JsonbFields_AddConditions()
    {
        // Arrange
        var filter = new SearchFilter { Value = "Admin", CaseSensitive = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id"), new("name"), new("role") });

        // Act
        var result = builder
            .Select(tc)
            .Search(filter, "role", "social->>'platform'", "profile->'personal'->>'full_name'")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id, users.name, users.role " +
            "FROM users " +
            "WHERE (" +
                "users.role LIKE @search_param_0 " +
                "OR users.social->>'platform' LIKE @search_param_0 " +
                "OR users.profile->'personal'->>'full_name' LIKE @search_param_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("search_param_0"));
        Assert.Equal("%Admin%", paramDict["search_param_0"]);
    }

    #endregion

    #region SearchTsVector

    [Fact]
    public void SearchTsVector_WithNullFilter_DoesNothing()
    {
        // Arrange
        var builder = new FlectoBuilder("docs", DialectType.Postgres);

        // Act
        var result = builder
            .SelectAll()
            .SearchTsVector(null, new[] { "text" })
            .Build();

        // Assert
        Assert.Equal("SELECT * FROM docs", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }


    [Fact]
    public void SearchTsVector_WithEmptyColumns_Throws()
    {
        // Arrange
        var filter = new SearchFilter { Value = "test" };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder.SearchTsVector(filter, new string[0]));

        // Assert
        Assert.Equal("Table 'users' must have at least one column specified", ex.Message);
    }

    [Fact]
    public void SearchTsVector_WithValidPlainSearch_AddsCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "developer" };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id"), new("bio") });

        // Act
        var result = builder
            .Select(tc)
            .SearchTsVector(filter, new[] { "bio", "notes", "profile->'personal'->>'full_name'" }) // Default: mode = Plain, config = "simple"
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id, users.bio " +
            "FROM users " +
            "WHERE to_tsvector('simple', " +
                    "COALESCE(users.bio, '') || ' ' || " +
                    "COALESCE(users.notes, '') || ' ' || " +
                    "COALESCE(users.profile->'personal'->>'full_name', '')" +
                ") @@ plainto_tsquery('simple', @tsvector_query_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(name => name, name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("tsvector_query_0"));
        Assert.Equal("developer", paramDict["tsvector_query_0"]);
    }

    [Fact]
    public void SearchTsVector_WithWebStyleSearch_AddsCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "csharp & backend" };
        var builder = new FlectoBuilder("profiles", DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .SearchTsVector(filter, new[] { "summary", "profile->'personal'->>'full_name'" }, TextSearchMode.WebStyle, "english")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id " +
            "FROM profiles " +
            "WHERE to_tsvector('english', " +
                    "COALESCE(profiles.summary, '') || ' ' || " +
                    "COALESCE(profiles.profile->'personal'->>'full_name', '')" +
                ") @@ websearch_to_tsquery('english', @tsvector_query_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(name => name, name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.Equal("csharp & backend", paramDict["tsvector_query_0"]);
    }

    #endregion
}
