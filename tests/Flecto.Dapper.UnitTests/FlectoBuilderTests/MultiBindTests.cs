using System.Globalization;
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class MultiBandTests
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
    private readonly SearchFilter searchTsVectorPlainFilter = new()
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
    private readonly EnumFilter<UserStatus> enumNameFilter = new()
    {
        NotIn = [UserStatus.Unknown, UserStatus.Inactive]
    };
    private readonly EnumFilter<UserStatus> enumValueFilter = new()
    {
        NotIn = [UserStatus.Unknown, UserStatus.Inactive]
    };
    private readonly EnumFilter<UserStatus> enumValueStringFilter = new()
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
    public void MultiBind_AddingTwice_AllConditionsDisplayCorrectly()
    {
        // Arrange
        var builder = new FlectoBuilder(Table, DialectType.Postgres)
            .Select(_tc)

            .Search(searchFilter, "name", "email")
            .Search(searchFilter, "name", "email")

            .SearchTsVector(
                searchTsVectorPlainFilter,
                ["bio", "notes", "profile->'personal'->>'full_name'"],
                TextSearchMode.Plain)
            .SearchTsVector(
                searchTsVectorPlainFilter,
                ["bio", "notes", "profile->'personal'->>'full_name'"],
                TextSearchMode.Plain)

            .SearchTsVector(
                searchTsVectorWebStyleFilter,
                ["summary", "profile->'personal'->>'full_name'"],
                TextSearchMode.WebStyle,
                "english")
            .SearchTsVector(
                searchTsVectorWebStyleFilter,
                ["summary", "profile->'personal'->>'full_name'"],
                TextSearchMode.WebStyle,
                "english")

            .BindBool(boolFilter, "profile->'is_active'")
            .BindBool(boolFilter, "profile->'is_active'")

            .BindDate(dateFilter, "created_at")
            .BindDate(dateFilter, "created_at")

            .BindEnum(enumNameFilter, "status", EnumFilterMode.Name)
            .BindEnum(enumNameFilter, "status", EnumFilterMode.Name)

            .BindEnum(enumValueFilter, "status", EnumFilterMode.Value)
            .BindEnum(enumValueFilter, "status", EnumFilterMode.Value)

            .BindEnum(enumValueStringFilter, "status", EnumFilterMode.ValueString)
            .BindEnum(enumValueStringFilter, "status", EnumFilterMode.ValueString)

            .BindFlagsEnum(flagsEnumFilter, "access")
            .BindFlagsEnum(flagsEnumFilter, "access")

            .BindGuid(guidFilter, "department_id")
            .BindGuid(guidFilter, "department_id")

            .BindNumeric(shortNumericFilter, "profile->>'short_value'")
            .BindNumeric(shortNumericFilter, "profile->>'short_value'")

            .BindNumeric(intNumericFilter, "profile->>'int_value'")
            .BindNumeric(intNumericFilter, "profile->>'int_value'")

            .BindNumeric(longNumericFilter, "profile->>'long_value'")
            .BindNumeric(longNumericFilter, "profile->>'long_value'")

            .BindNumeric(decimalNumericFilter, "profile->>'decimal_value'")
            .BindNumeric(decimalNumericFilter, "profile->>'decimal_value'")

            .BindNumeric(doubleNumericFilter, "profile->>'double_value'")
            .BindNumeric(doubleNumericFilter, "profile->>'double_value'")

            .BindNumeric(floatNumericFilter, "profile->>'float_value'")
            .BindNumeric(floatNumericFilter, "profile->>'float_value'")

            .BindString(stringFilter, "first_name")
            .BindString(stringFilter, "first_name")

            .ApplyPaging(pagingFilter);

        var search0 = (Param: "search_param_0", Val: searchFilter);
        var search1 = (Param: "search_param_1", Val: searchFilter);

        var tsVectorPlain0 = (Param: "search_tsvector_param_0", Val: searchTsVectorPlainFilter);
        var tsVectorPlain1 = (Param: "search_tsvector_param_1", Val: searchTsVectorPlainFilter);

        var tsVectorWebStyle2 = (Param: "search_tsvector_param_2", Val: searchTsVectorWebStyleFilter);
        var tsVectorWebStyle3 = (Param: "search_tsvector_param_3", Val: searchTsVectorWebStyleFilter);

        var bool0 = (Param: "users_profile_is_active_Eq_0", Val: boolFilter);
        var bool1 = (Param: "users_profile_is_active_Eq_1", Val: boolFilter);

        var date0 = (Param: "users_created_at_In_0", Val: dateFilter);
        var date1 = (Param: "users_created_at_In_1", Val: dateFilter);

        var enumName0 = (Param: "users_status_NotIn_0", Val: enumNameFilter);
        var enumName1 = (Param: "users_status_NotIn_1", Val: enumNameFilter);
        var enumValue2 = (Param: "users_status_NotIn_2", Val: enumValueFilter);
        var enumValue3 = (Param: "users_status_NotIn_3", Val: enumValueFilter);
        var enumValueString4 = (Param: "users_status_NotIn_4", Val: enumValueStringFilter);
        var enumValueString5 = (Param: "users_status_NotIn_5", Val: enumValueStringFilter);

        var flagsEnum0 = (Param: "users_access_HasFlag_0", Val: flagsEnumFilter);
        var flagsEnum1 = (Param: "users_access_HasFlag_1", Val: flagsEnumFilter);

        var short0 = (Param: "users_profile_short_value_Gt_0", Val: shortNumericFilter);
        var short1 = (Param: "users_profile_short_value_Gt_1", Val: shortNumericFilter);

        var int2 = (Param: "users_profile_int_value_Lt_2", Val: intNumericFilter);
        var int3 = (Param: "users_profile_int_value_Lt_3", Val: intNumericFilter);

        var long4 = (Param: "users_profile_long_value_Lte_4", Val: longNumericFilter);
        var long5 = (Param: "users_profile_long_value_Lte_5", Val: longNumericFilter);

        var decimal6 = (Param: "users_profile_decimal_value_Gt_6", Val: decimalNumericFilter);
        var decimal7 = (Param: "users_profile_decimal_value_Gt_7", Val: decimalNumericFilter);

        var double8 = (Param: "users_profile_double_value_Gte_8", Val: doubleNumericFilter);
        var double9 = (Param: "users_profile_double_value_Gte_9", Val: doubleNumericFilter);

        var float10 = (Param: "users_profile_float_value_Gt_10", Val: floatNumericFilter);
        var float11 = (Param: "users_profile_float_value_Gt_11", Val: floatNumericFilter);

        var string0 = (Param: "users_first_name_In_0", Val: stringFilter);
        var string1 = (Param: "users_first_name_In_1", Val: stringFilter);

        var paging = (OffsetParam: "_Offset", LimitParam: "_Limit", Val: pagingFilter);

        // Act
        var result = builder.Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.name ILIKE @{search0.Param} OR users.email ILIKE @{search0.Param}) " +
                $"AND (users.name ILIKE @{search1.Param} OR users.email ILIKE @{search1.Param}) " +
                "AND to_tsvector('simple', " +
                        "COALESCE(users.bio, '') || ' ' || " +
                        "COALESCE(users.notes, '') || ' ' || " +
                        "COALESCE(users.profile->'personal'->>'full_name', '')" +
                    $") @@ plainto_tsquery('simple', @{tsVectorPlain0.Param}) " +
                "AND to_tsvector('simple', " +
                        "COALESCE(users.bio, '') || ' ' || " +
                        "COALESCE(users.notes, '') || ' ' || " +
                        "COALESCE(users.profile->'personal'->>'full_name', '')" +
                    $") @@ plainto_tsquery('simple', @{tsVectorPlain1.Param}) " +
                "AND to_tsvector('english', " +
                        "COALESCE(users.summary, '') || ' ' || " +
                        "COALESCE(users.profile->'personal'->>'full_name', '')" +
                    $") @@ websearch_to_tsquery('english', @{tsVectorWebStyle2.Param}) " +
                "AND to_tsvector('english', " +
                        "COALESCE(users.summary, '') || ' ' || " +
                        "COALESCE(users.profile->'personal'->>'full_name', '')" +
                    $") @@ websearch_to_tsquery('english', @{tsVectorWebStyle3.Param}) " +
                $"AND (users.profile->'is_active')::boolean = @{bool0.Param} " +
                $"AND (users.profile->'is_active')::boolean = @{bool1.Param} " +
                $"AND users.created_at = ANY(@{date0.Param}) " +
                $"AND users.created_at = ANY(@{date1.Param}) " +
                $"AND users.status <> ANY(@{enumName0.Param}) " +
                $"AND users.status <> ANY(@{enumName1.Param}) " +
                $"AND users.status <> ANY(@{enumValue2.Param}) " +
                $"AND users.status <> ANY(@{enumValue3.Param}) " +
                $"AND users.status <> ANY(@{enumValueString4.Param}) " +
                $"AND users.status <> ANY(@{enumValueString5.Param}) " +
                $"AND users.access & @{flagsEnum0.Param} <> 0 " +
                $"AND users.access & @{flagsEnum1.Param} <> 0 " +
                $"AND users.department_id IS NOT NULL " +
                $"AND users.department_id IS NOT NULL " +
                $"AND (users.profile->>'short_value')::int2 > @{short0.Param} " +
                $"AND (users.profile->>'short_value')::int2 > @{short1.Param} " +
                $"AND (users.profile->>'int_value')::int4 < @{int2.Param} " +
                $"AND (users.profile->>'int_value')::int4 < @{int3.Param} " +
                $"AND (users.profile->>'long_value')::int8 <= @{long4.Param} " +
                $"AND (users.profile->>'long_value')::int8 <= @{long5.Param} " +
                $"AND (users.profile->>'decimal_value')::numeric > @{decimal6.Param} " +
                $"AND (users.profile->>'decimal_value')::numeric > @{decimal7.Param} " +
                $"AND (users.profile->>'double_value')::float8 >= @{double8.Param} " +
                $"AND (users.profile->>'double_value')::float8 >= @{double9.Param} " +
                $"AND (users.profile->>'float_value')::float4 > @{float10.Param} " +
                $"AND (users.profile->>'float_value')::float4 > @{float11.Param} " +
                $"AND LOWER(users.first_name) = ANY(@{string0.Param}) " +
                $"AND LOWER(users.first_name) = ANY(@{string1.Param}) " +
            "ORDER BY (users.profile->'is_active')::boolean ASC, users.created_at DESC, users.department_id DESC " +
            $"LIMIT @{paging.LimitParam} OFFSET @{paging.OffsetParam}",
            result.Sql);


        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                result.Parameters.Get<object?>);

        Assert.Equal(34, paramDict.Count);

        Assert.Equal($"%{search0.Val.Value}%", paramDict[search0.Param]);
        Assert.Equal($"%{search1.Val.Value}%", paramDict[search0.Param]);

        Assert.Equal(tsVectorPlain0.Val.Value, paramDict[tsVectorPlain0.Param]);
        Assert.Equal(tsVectorPlain1.Val.Value, paramDict[tsVectorPlain1.Param]);

        Assert.Equal(tsVectorWebStyle2.Val.Value, paramDict[tsVectorWebStyle2.Param]);
        Assert.Equal(tsVectorWebStyle3.Val.Value, paramDict[tsVectorWebStyle3.Param]);

        Assert.Equal(bool0.Val.Eq, paramDict[bool0.Param]);
        Assert.Equal(bool1.Val.Eq, paramDict[bool1.Param]);

        Assert.Equal(date0.Val.In, paramDict[date0.Param]);
        Assert.Equal(date1.Val.In, paramDict[date1.Param]);


        Assert.Equal(new object[] { "Unknown", "Inactive" }, paramDict[enumName0.Param]!);
        Assert.Equal(new object[] { "Unknown", "Inactive" }, paramDict[enumName1.Param]!);
        Assert.Equal(new object[] { 0, 2 }, paramDict[enumValue2.Param]!);
        Assert.Equal(new object[] { 0, 2 }, paramDict[enumValue3.Param]!);
        Assert.Equal(new object[] { "0", "2" }, paramDict[enumValueString4.Param]!);
        Assert.Equal(new object[] { "0", "2" }, paramDict[enumValueString5.Param]!);

        Assert.Equal(
            Convert.ToInt64(flagsEnum0.Val.HasFlag, CultureInfo.InvariantCulture),
            paramDict[flagsEnum0.Param]);
        Assert.Equal(
            Convert.ToInt64(flagsEnum1.Val.HasFlag, CultureInfo.InvariantCulture),
            paramDict[flagsEnum1.Param]);

        Assert.Equal(short0.Val.Gt, paramDict[short0.Param]);
        Assert.Equal(short1.Val.Gt, paramDict[short1.Param]);

        Assert.Equal(int2.Val.Lt, paramDict[int2.Param]);
        Assert.Equal(int3.Val.Lt, paramDict[int3.Param]);

        Assert.Equal(long4.Val.Lte, paramDict[long4.Param]);
        Assert.Equal(long5.Val.Lte, paramDict[long5.Param]);

        Assert.Equal(decimal6.Val.Gt, paramDict[decimal6.Param]);
        Assert.Equal(decimal7.Val.Gt, paramDict[decimal7.Param]);

        Assert.Equal(double8.Val.Gte, paramDict[double8.Param]);
        Assert.Equal(double9.Val.Gte, paramDict[double9.Param]);

        Assert.Equal(float10.Val.Gt, paramDict[float10.Param]);
        Assert.Equal(float11.Val.Gt, paramDict[float11.Param]);

        var expectedStringIn = new string[] { "alice", "bob" };
        Assert.Equal(expectedStringIn, paramDict[string0.Param]);
        Assert.Equal(expectedStringIn, paramDict[string1.Param]);

        Assert.Equal(paging.Val.Limit, paramDict[paging.LimitParam]);
        Assert.Equal(40, paramDict[paging.OffsetParam]);
    }
}
