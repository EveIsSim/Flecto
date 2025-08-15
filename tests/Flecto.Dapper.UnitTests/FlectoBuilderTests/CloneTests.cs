using System.Globalization;
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class CloneTests
{
    private const string Table = "users";
    private readonly FromTable _tc = new(Table, [new("id")]);

    private enum UserStatus
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2
    }

    [Flags]
    private enum Access
    {
        Nont = 0,
        Read = 1 << 0, // 1
        Write = 1 << 1, // 2
        Admin = 1 << 2, // 4
    }

    private readonly SearchFilter searchFilter = new()
    {
        Value = "query usual search",
        CaseSensitive = false
    };
    private readonly SearchFilter searchTsVectorPlaneFilter = new()
    {
        Value = "query ts vector plain",
        CaseSensitive = false
    };
    private readonly SearchFilter searchTsVectorWebStyleFilter = new()
    {
        Value = "query ts vector webStyle",
        CaseSensitive = false
    };
    private readonly BoolFilter boolFilter = new()
    {
        Eq = true,
        Sort = new Sort(1, descending: false)
    };
    private readonly DateFilter dateFilter = new()
    {
        In = [DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture)],
        Sort = new Sort(2, descending: true)
    };
    private readonly EnumFilter<UserStatus> enumFilter = new()
    {
        NotIn = [UserStatus.Unknown, UserStatus.Inactive]
    };
    private readonly FlagsEnumFilter<Access> flagsEnumFilter = new()
    {
        HasFlag = Access.Admin
    };
    private readonly GuidFilter guidFilter = new()
    {
        IsNull = false,
        Sort = new Sort(3, descending: true)
    };
    private readonly NumericFilter<short> shortNumericFilter = new() { Gt = 10 };
    private readonly NumericFilter<int> intNumericFilter = new() { Lt = 20 };
    private readonly NumericFilter<long> longNumericFilter = new() { Lte = 30L };
    private readonly NumericFilter<decimal> decimalNumericFilter = new() { Gt = 9999.99m };
    private readonly NumericFilter<double> doubleNumericFilter = new() { Gte = 123.456 };
    private readonly NumericFilter<float> floatNumericFilter = new() { Gt = 3.14f };
    private readonly StringFilter stringFilter = new()
    {
        In = ["Alice", "Bob"],
        CaseSensitive = false
    };
    private readonly PaginationFilter pagingFilter = new() { Limit = 10, Page = 5 };

    [Fact]
    public void Clone_ProducesIdenticalQueryAndParams_WithSelectAll()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .SelectAll()
            .Search(searchFilter, "name", "email")
            .SearchTsVector(
                searchTsVectorPlaneFilter,
                ["bio", "notes", "profile->'personal'->>'full_name'"],
                TextSearchMode.Plain)
            .SearchTsVector(
                searchTsVectorWebStyleFilter,
                ["summary", "profile->'personal'->>'full_name'"],
                TextSearchMode.WebStyle,
                "english")
            .BindBool(boolFilter, "profile->'is_active'")
            .BindDate(dateFilter, "created_at")
            .BindEnum(enumFilter, "status", EnumFilterMode.Name)
            .BindFlagsEnum(flagsEnumFilter, "access")
            .BindGuid(guidFilter, "department_id")
            .BindNumeric(shortNumericFilter, "profile->>'short_value'")
            .BindNumeric(intNumericFilter, "profile->>'int_value'")
            .BindNumeric(longNumericFilter, "profile->>'long_value'")
            .BindNumeric(decimalNumericFilter, "profile->>'decimal_value'")
            .BindNumeric(doubleNumericFilter, "profile->>'double_value'")
            .BindNumeric(floatNumericFilter, "profile->>'float_value'")
            .BindString(stringFilter, "first_name")
            .ApplyPaging(pagingFilter);

        var original = builder.Build();

        // Act
        var cloned = builder
            .Clone()
            .Build();

        // Assert
        Assert.Equal(original.Sql, cloned.Sql);

        var oParams = original.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                original.Parameters.Get<object?>);
        var cParams = cloned.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                cloned.Parameters.Get<object?>);

        var expectedParamsNumber = 16;
        Assert.Equal(oParams.Count, expectedParamsNumber);

        Assert.Equal(oParams.Count, cParams.Count);

        foreach (var (k, v) in oParams)
        {
            Assert.True(cParams.ContainsKey(k));
            Assert.Equal(v, cParams[k]);
        }
    }

    [Fact]
    public void Clone_ProducesIdenticalQueryAndParams_WithSelectCount()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .SelectCount()
            .Search(searchFilter, "name", "email")
            .SearchTsVector(
                searchTsVectorPlaneFilter,
                ["bio", "notes", "profile->'personal'->>'full_name'"],
                TextSearchMode.Plain)
            .SearchTsVector(
                searchTsVectorWebStyleFilter,
                ["summary", "profile->'personal'->>'full_name'"],
                TextSearchMode.WebStyle,
                "english")
            .BindBool(boolFilter, "profile->'is_active'")
            .BindDate(dateFilter, "created_at")
            .BindEnum(enumFilter, "status", EnumFilterMode.Name)
            .BindFlagsEnum(flagsEnumFilter, "access")
            .BindGuid(guidFilter, "department_id")
            .BindNumeric(shortNumericFilter, "profile->>'short_value'")
            .BindNumeric(intNumericFilter, "profile->>'int_value'")
            .BindNumeric(longNumericFilter, "profile->>'long_value'")
            .BindNumeric(decimalNumericFilter, "profile->>'decimal_value'")
            .BindNumeric(doubleNumericFilter, "profile->>'double_value'")
            .BindNumeric(floatNumericFilter, "profile->>'float_value'")
            .BindString(stringFilter, "first_name");

        var original = builder.Build();

        // Act
        var cloned = builder
            .Clone()
            .Build();

        // Assert
        Assert.Equal(original.Sql, cloned.Sql);

        var oParams = original.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                original.Parameters.Get<object?>);
        var cParams = cloned.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                cloned.Parameters.Get<object?>);

        var expectedParamsNumber = 14;
        Assert.Equal(oParams.Count, expectedParamsNumber);

        Assert.Equal(oParams.Count, cParams.Count);

        foreach (var (k, v) in oParams)
        {
            Assert.True(cParams.ContainsKey(k));
            Assert.Equal(v, cParams[k]);
        }
    }

    [Fact]
    public void Clone_ProducesIdenticalQueryAndParams_WithSelect()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(_tc)
            .Search(searchFilter, "name", "email")
            .SearchTsVector(
                searchTsVectorPlaneFilter,
                ["bio", "notes", "profile->'personal'->>'full_name'"],
                TextSearchMode.Plain)
            .SearchTsVector(
                searchTsVectorWebStyleFilter,
                ["summary", "profile->'personal'->>'full_name'"],
                TextSearchMode.WebStyle,
                "english")
            .BindBool(boolFilter, "profile->'is_active'")
            .BindDate(dateFilter, "created_at")
            .BindEnum(enumFilter, "status", EnumFilterMode.Name)
            .BindFlagsEnum(flagsEnumFilter, "access")
            .BindGuid(guidFilter, "department_id")
            .BindNumeric(shortNumericFilter, "profile->>'short_value'")
            .BindNumeric(intNumericFilter, "profile->>'int_value'")
            .BindNumeric(longNumericFilter, "profile->>'long_value'")
            .BindNumeric(decimalNumericFilter, "profile->>'decimal_value'")
            .BindNumeric(doubleNumericFilter, "profile->>'double_value'")
            .BindNumeric(floatNumericFilter, "profile->>'float_value'")
            .BindString(stringFilter, "first_name")
            .ApplyPaging(pagingFilter);

        var original = builder.Build();

        // Act
        var cloned = builder
            .Clone()
            .Build();

        // Assert
        Assert.Equal(original.Sql, cloned.Sql);

        var oParams = original.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                original.Parameters.Get<object?>);
        var cParams = cloned.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                cloned.Parameters.Get<object?>);

        var expectedParamsNumber = 16;
        Assert.Equal(oParams.Count, expectedParamsNumber);

        Assert.Equal(oParams.Count, cParams.Count);

        foreach (var (k, v) in oParams)
        {
            Assert.True(cParams.ContainsKey(k));
            Assert.Equal(v, cParams[k]);
        }
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(_tc)
            .BindBool(boolFilter, "is_active");

        var originalBefore = builder.Build();

        // Act
        var cloned = builder
            .Clone()
            .BindString(new StringFilter { Eq = "Alice", CaseSensitive = true }, "name")
            .Build();

        // Assert
        var originalAfter = builder.Build();

        Assert.Equal(originalBefore.Sql, originalAfter.Sql);
        Assert.Equal(
            originalBefore.Parameters.ParameterNames.Count(),
            originalAfter.Parameters.ParameterNames.Count());

        Assert.NotEqual(originalAfter.Sql, cloned.Sql);

        var exp = "users.name = @users_name_Eq_0";
        Assert.Contains(exp, cloned.Sql);
        Assert.DoesNotContain(exp, originalAfter.Sql);
        Assert.DoesNotContain(exp, originalBefore.Sql);
    }

    [Fact]
    public void Clone_ModifyingOriginalDoesNotAffectClone()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(_tc)
            .BindBool(boolFilter, "is_active");

        var cloned = builder
            .Clone()
            .Build();

        // Act
        var original = builder
            .BindString(new StringFilter { Eq = "Alice", CaseSensitive = true }, "name")
            .Build();

        // Assert
        Assert.NotEqual(original.Sql, cloned.Sql);

        var exp = "users.name = @users_name_Eq_0";
        Assert.DoesNotContain(exp, cloned.Sql);
        Assert.Contains(exp, original.Sql);
    }

    [Fact]
    public void Clone_CountersAreCopiedAndAdvanceIndependently_And_NotSharedReference()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(_tc)
            .BindBool(boolFilter, "is_active");

        var clone = builder
            .Clone();

        // Act
        var cloned = clone
            .BindBool(new BoolFilter { Eq = false }, "is_active")
            .BindBool(new BoolFilter { Eq = false }, "is_active")
            .Build();

        // Assert
        Assert.Contains("@users_is_active_Eq_0", cloned.Sql);
        Assert.Contains("@users_is_active_Eq_1", cloned.Sql);

        var original = builder
            .BindBool(new BoolFilter { Eq = false }, "is_active")
            .Build();
        Assert.Contains("@users_is_active_Eq_0", original.Sql);
        Assert.Contains("@users_is_active_Eq_1", original.Sql);
        Assert.DoesNotContain("@users_is_active_Eq_2", original.Sql);
    }
}
