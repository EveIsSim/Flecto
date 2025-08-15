using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class SearchTests
{
    private const string Table = "users";
    private readonly FromTable _tc = new(Table, [new("id")]);

    #region Search

    [Fact]
    public void Search_WithNullFilter_DoesNotAddCondition()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
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
            builder.Search(filter, []));

        // Assert
        Assert.Equal("Table 'users' must have at least one column specified", ex.Message);
    }

    [Fact]
    public void Search_WithValidFilter_AddsILikeCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "joHn", CaseSensitive = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
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
                static name => name,
                result.Parameters.Get<string>);

        _ = Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("search_param_0"));
        Assert.Equal("%joHn%", paramDict["search_param_0"]);
    }

    [Fact]
    public void Search_CaseSensitive_AddsLikeCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "Admin", CaseSensitive = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, [new("id"), new("name"), new("role")]);

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
                static name => name,
                result.Parameters.Get<string>);

        _ = Assert.Single(paramDict);
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

        // Act
        var result = builder
            .Select(_tc)
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
                static name => name,
                result.Parameters.Get<string>);

        Assert.Equal(2, paramDict.Count);
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

        var tc = new FromTable(Table, [new("id"), new("name"), new("role")]);

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
                static name => name,
                result.Parameters.Get<string>);

        _ = Assert.Single(paramDict);
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
            .SearchTsVector(null, ["text"])
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
            builder.SearchTsVector(filter, []));

        // Assert
        Assert.Equal("Table 'users' must have at least one column specified", ex.Message);
    }

    [Fact]
    public void SearchTsVector_WithValidPlainSearch_AddsCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "developer" };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, [new("id"), new("bio")]);

        // Act
        var result = builder
            .Select(tc)
            .SearchTsVector(filter, ["bio", "notes", "profile->'personal'->>'full_name'"]) // Default: mode = Plain, config = "simple"
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id, users.bio " +
            "FROM users " +
            "WHERE to_tsvector('simple', " +
                    "COALESCE(users.bio, '') || ' ' || " +
                    "COALESCE(users.notes, '') || ' ' || " +
                    "COALESCE(users.profile->'personal'->>'full_name', '')" +
                ") @@ plainto_tsquery('simple', @search_tsvector_param_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(static name => name, result.Parameters.Get<string>);

        _ = Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey("search_tsvector_param_0"));
        Assert.Equal("developer", paramDict["search_tsvector_param_0"]);
    }

    [Fact]
    public void SearchTsVector_WithWebStyleSearch_AddsCondition()
    {
        // Arrange
        var filter = new SearchFilter { Value = "csharp & backend" };
        var builder = new FlectoBuilder("profiles", DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .SearchTsVector(filter, ["summary", "profile->'personal'->>'full_name'"], TextSearchMode.WebStyle, "english")
            .Build();

        // Assert SQL
        Assert.Equal(
            "SELECT users.id " +
            "FROM profiles " +
            "WHERE to_tsvector('english', " +
                    "COALESCE(profiles.summary, '') || ' ' || " +
                    "COALESCE(profiles.profile->'personal'->>'full_name', '')" +
                ") @@ websearch_to_tsquery('english', @search_tsvector_param_0)",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(static name => name, result.Parameters.Get<string>);

        _ = Assert.Single(paramDict);
        Assert.Equal("csharp & backend", paramDict["search_tsvector_param_0"]);
    }

    #endregion

    #region Multi

    [Fact]
    public void SearchMulti_AddsCondition()
    {
        // Arrange
        var searchfilter0 = new SearchFilter { Value = "joHn", CaseSensitive = false };
        var searchfilter1 = new SearchFilter { Value = "Alice", CaseSensitive = true };

        var plainFilter0 = new SearchFilter { Value = "developer" };
        var plainFilter1 = new SearchFilter { Value = "tech_guy" };

        var webStyleFilter2 = new SearchFilter { Value = "csharp & backend" };
        var webStyleFilter3 = new SearchFilter { Value = "golang" };

        var builder = new FlectoBuilder("profiles", DialectType.Postgres);

        var tc = new FromTable(Table, [new("id")]);

        // Act
        var result = builder
            .Select(tc)
            .Search(searchfilter0, ["a", "b"])
            .Search(searchfilter1, ["c", "d"])
            .SearchTsVector(plainFilter0, ["e", "ee->'eee'->>'eeee'"], TextSearchMode.Plain)
            .SearchTsVector(plainFilter1, ["f", "ff->'fff'->>'ffff'"], TextSearchMode.Plain, "english")
            .SearchTsVector(webStyleFilter2, ["g", "gg->'ggg'->>'gggg'"], TextSearchMode.WebStyle, "english")
            .SearchTsVector(webStyleFilter3, ["k", "kk->'kkk'->>'kkkk'"], TextSearchMode.WebStyle, "english")
            .Build();

        // Assert SQL
        var searchParam0 = "search_param_0";
        var searchParam1 = "search_param_1";

        var plainParam0 = "search_tsvector_param_0";
        var plainParam1 = "search_tsvector_param_1";
        var webStyleParam2 = "search_tsvector_param_2";
        var webStyleParam3 = "search_tsvector_param_3";

        Assert.Equal(
            "SELECT users.id " +
            "FROM profiles " +
            $"WHERE (profiles.a ILIKE @search_param_0 OR profiles.b ILIKE @{searchParam0}) " +
                $"AND (profiles.c LIKE @search_param_1 OR profiles.d LIKE @{searchParam1}) " +
                "AND to_tsvector('simple', " +
                        "COALESCE(profiles.e, '') || ' ' || " +
                        "COALESCE(profiles.ee->'eee'->>'eeee', '')" +
                    $") @@ plainto_tsquery('simple', @{plainParam0}) " +
                "AND to_tsvector('english', " +
                        "COALESCE(profiles.f, '') || ' ' || " +
                        "COALESCE(profiles.ff->'fff'->>'ffff', '')" +
                    $") @@ plainto_tsquery('english', @{plainParam1}) " +
                "AND to_tsvector('english', " +
                        "COALESCE(profiles.g, '') || ' ' || " +
                        "COALESCE(profiles.gg->'ggg'->>'gggg', '')" +
                    $") @@ websearch_to_tsquery('english', @{webStyleParam2}) " +
                "AND to_tsvector('english', " +
                        "COALESCE(profiles.k, '') || ' ' || " +
                        "COALESCE(profiles.kk->'kkk'->>'kkkk', '')" +
                    $") @@ websearch_to_tsquery('english', @{webStyleParam3})",
            result.Sql);

        // Assert params
        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                static name => name,
                result.Parameters.Get<string>);

        Assert.Equal(6, paramDict.Count);

        Assert.True(paramDict.ContainsKey(searchParam0));
        Assert.Equal("%joHn%", paramDict[searchParam0]);

        Assert.True(paramDict.ContainsKey(searchParam1));
        Assert.Equal("%Alice%", paramDict[searchParam1]);

        Assert.True(paramDict.ContainsKey(searchParam1));
        Assert.Equal(plainFilter0.Value, paramDict[plainParam0]);

        Assert.True(paramDict.ContainsKey(searchParam1));
        Assert.Equal(plainFilter1.Value, paramDict[plainParam1]);

        Assert.True(paramDict.ContainsKey(searchParam1));
        Assert.Equal(webStyleFilter2.Value, paramDict[webStyleParam2]);

        Assert.True(paramDict.ContainsKey(searchParam1));
        Assert.Equal(webStyleFilter3.Value, paramDict[webStyleParam3]);
    }

    #endregion
}
