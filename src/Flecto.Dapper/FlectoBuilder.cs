using System.Text;
using Dapper;
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Core.Models.Select;
using Flecto.Core.Validators;
using Flecto.Dapper.Commons;
using Flecto.Dapper.Constants;
using Flecto.Dapper.Models;
using Flecto.Dapper.SqlDialect;
using Flecto.Dapper.SqlDialect.Dialects.Postgres;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting;
using Flecto.Dapper.SqlDialect.Enums;
using Flecto.Dapper.SqlDialect.Helpers;

namespace Flecto.Dapper;


/// <summary>
/// Provides a flexible and composable builder for dynamically constructing SQL queries in a safe
/// </summary>
public class FlectoBuilder
{
    private readonly ISqlDialect _dialect;
    private readonly ICastTypeMapper _castTypeMapper;
    private readonly DialectType _dialectType;

    private string? _selectColumns;
    private bool _selectWasSet = false;

    private int _searchCounter = 0;
    private int _boolCounter = 0;
    private int _dateCounter = 0;
    private int _enumCounter = 0;
    private int _flagsEnumCounter = 0;
    private int _numericCounter = 0;
    private int _guidCounter = 0;
    private int _stringCounter = 0;

    private readonly string _fromTable;
    private bool _exceptOrderBy;
    private (int Limit, int Offset)? _paging;
    private List<string> _conditions = new();
    private readonly DynamicParameters _parameters = new();
    private readonly Dictionary<string, Sort> _sortFields = new();

    private const string DefaultVectorConfig = "simple";


    /// <summary>
    /// Initializes a new instance of the <see cref="FlectoBuilder"/> class,
    /// building it to the specified table and SQL dialect.
    /// </summary>
    /// <param name="fromTable">
    /// The table name from which to select data.
    /// Must not be null, empty, or whitespace.
    /// </param>
    /// <param name="dialectType">
    /// The SQL dialect to use for query generation (e.g., <see cref="DialectType.Postgres"/>).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="fromTable"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if an unsupported <paramref name="dialectType"/> is provided.
    /// </exception>
    public FlectoBuilder(string fromTable, DialectType dialectType)
    {
        if (string.IsNullOrWhiteSpace(fromTable))
            throw new ArgumentException("FromTable is required and cannot be null or empty");

        _fromTable = fromTable;

        (_dialect, _castTypeMapper) = dialectType switch
        {
            DialectType.Postgres => (new PostgresSqlDialect(), new PgCastTypeMapper()),
            _ => throw new ArgumentOutOfRangeException(nameof(dialectType), dialectType, null)
        };

        _dialectType = dialectType;
    }

    #region Select

    /// <summary>
    /// Selects all columns from the target table in the query.
    /// This method sets the SELECT clause to <c>SELECT *</c>.
    /// </summary>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder SelectAll()
    {
        SelectValidator.EnsureValid(_selectWasSet);

        _selectColumns = $"{Sql.SELECT} *";
        _exceptOrderBy = false;
        _selectWasSet = true;

        return this;
    }

    /// <summary>
    /// Selects a count of all rows from the target table in the query.
    /// This method sets the SELECT clause to <c>SELECT COUNT(*)</c> and disables ORDER BY since ordering is irrelevant for counting.
    /// </summary>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder SelectCount()
    {
        SelectValidator.EnsureValid(_selectWasSet);

        _selectColumns = $"{Sql.SELECT} COUNT(*)";
        _exceptOrderBy = true;
        _selectWasSet = true;

        return this;
    }

    /// <summary>
    /// Selects the specified columns from the target table defined in the current <see cref="FlectoBuilder"/> instance.
    /// Internally attaches the columns to the <c>FROM</c> table associated with this query.
    /// </summary>
    /// <param name="columns">The column names to include in the SELECT clause.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder Select(FromTable tableWithColumns)
    => Select([tableWithColumns]);

    /// <summary>
    /// Selects the specified columns from the specified tables in the query.
    /// This overload is currently private and will be made public when JOIN ON logic is implemented for <see cref="FlectoBuilder"/>.
    /// Validates that the SELECT clause has not already been set and constructs the SELECT clause with fully qualified column references.
    /// </summary>
    /// <param name="tablesWithColumns">
    /// An array of table and column name pairs to include in the SELECT clause.
    /// Each tuple contains the table name and an array of column names associated with that table.
    /// </param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder Select(FromTable[] tablesWithColumns)
    {
        SelectValidator.EnsureValid(_selectWasSet, tablesWithColumns);

        var columns = tablesWithColumns
            .SelectMany(t => t.Fields.Select(f =>
                f.Alias is null
                    ? $"{t.Table}.{f.Column}"
                    : $"{t.Table}.{f.Column} {Sql.AS} {f.Alias}"));

        _selectColumns = $"{Sql.SELECT} {string.Join(", ", columns)}";
        _exceptOrderBy = false;
        _selectWasSet = true;

        return this;
    }

    #endregion Select

    #region Search

    /// <summary>
    /// Adds a LIKE-based search condition to the query using the specified columns of the target table.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The search filter containing the search value and case sensitivity.</param>
    /// <param name="columns">The columns of the target table to include in the search.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder Search(SearchFilter? filter, params string[] columns)
        => Search(filter, _fromTable, columns);

    /// <summary>
    /// Adds a LIKE-based search condition to the query using the specified table and columns.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The search filter containing the search value and case sensitivity.</param>
    /// <param name="table">The table containing the columns to include in the search.</param>
    /// <param name="columns">The columns of the specified table to include in the search.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder Search(SearchFilter? filter, string table, string[] columns)
        => Search(filter, (table, columns));

    /// <summary>
    /// Adds a LIKE-based search condition to the query using the specified tables and columns.
    /// This overload is currently private and will be made public when JOIN support is implemented.
    /// </summary>
    /// <param name="filter">The search filter containing the search value and case sensitivity.</param>
    /// <param name="tablesWithColumns">The tables and their columns to include in the search.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder Search(
        SearchFilter? filter,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (filter is null) return this;
        SearchValidator.EnsureValid(filter, tablesWithColumns);

        const string prefix = "search_param_";
        var paramName = Common.GenSearchParamName(prefix, _searchCounter++);
        var result = _dialect.BuildSearch(filter, paramName, tablesWithColumns);

        AddCondition(result.SqlCondition, paramName, result.ParamValue);

        return this;
    }

    /// <summary>
    /// Adds a full-text search condition using PostgreSQL <c>tsvector</c> to the query,
    /// using the specified columns of the target table.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The search filter containing the search value.</param>
    /// <param name="columns">The columns of the target table to include in the full-text search vector.</param>
    /// <param name="mode">The text search mode (Plain or WebStyle). Defaults to Plain.</param>
    /// <param name="config">The text search configuration (e.g., "simple", "english"). Defaults to "simple".</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder SearchTsVector(
        SearchFilter? filter,
        string[] columns,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig)
    => SearchTsVector(filter, _fromTable, columns, mode, config);

    /// <summary>
    /// Adds a full-text search condition using PostgreSQL <c>tsvector</c> to the query,
    /// using the specified table and columns.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The search filter containing the search value.</param>
    /// <param name="table">The table containing the columns for the full-text search vector.</param>
    /// <param name="columns">The columns of the specified table to include in the full-text search vector.</param>
    /// <param name="mode">The text search mode (Plain or WebStyle). Defaults to Plain.</param>
    /// <param name="config">The text search configuration (e.g., "simple", "english"). Defaults to "simple".</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder SearchTsVector(
        SearchFilter? filter,
        string table,
        string[] columns,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig)
    => SearchTsVector(filter, mode, config, (table, columns));

    /// <summary>
    /// Adds a full-text search condition using PostgreSQL <c>tsvector</c> to the query,
    /// using the specified tables and columns.
    /// Validates the provided <paramref name="filter"/> and ensures compatibility with the configured SQL dialect.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The search filter containing the search value.</param>
    /// <param name="mode">The text search mode (Plain or WebStyle). Defaults to Plain.</param>
    /// <param name="config">The text search configuration (e.g., "simple", "english"). Defaults to "simple".</param>
    /// <param name="tablesWithColumns">The tables and their columns to include in the full-text search vector.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder SearchTsVector(
        SearchFilter? filter,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (filter is null) return this;
        SearchValidator.EnsureValidTsVector(filter, tablesWithColumns, _dialectType);

        const string prefix = "tsvector_query_";
        var paramName = Common.GenSearchParamName(prefix, _searchCounter);
        var condition = _dialect.BuildTsVectorSearchCondition(paramName, mode, config, tablesWithColumns);
        AddCondition(condition, paramName, filter.Value);

        return this;
    }

    #endregion

    #region Bind

    /// <summary>
    /// Binds a <see cref="BoolFilter"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The <see cref="BoolFilter"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindBool(BoolFilter? filter, string column)
    => BindBool(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="BoolFilter"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload is currently private and will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <param name="filter">The <see cref="BoolFilter"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder BindBool(BoolFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        BoolValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _boolCounter++, _castTypeMapper);
        var clrType = typeof(bool);

        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddBoolComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, clrType, _dialect.BuildBoolComparison);
        AddBoolComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, clrType, _dialect.BuildBoolComparison);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    // 999 HERE tests 02.08.2026

    /// <summary>
    /// Binds a <see cref="DateFilter"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, range comparisons, IN/NOT IN array checks,
    /// null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The <see cref="DateFilter"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindDate(DateFilter? filter, string column)
    => BindDate(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="DateFilter"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, range comparisons, IN/NOT IN array checks,
    /// null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <param name="filter">The <see cref="DateFilter"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindDate(DateFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        DateValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _dateCounter++, _castTypeMapper);
        var clrType = typeof(DateTime);

        AddComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Gt, ComparisonOperator.Gt, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Gte, ComparisonOperator.Gte, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Lt, ComparisonOperator.Lt, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Lte, ComparisonOperator.Lte, clrType, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(cr, filter.In, ArrayComparisonOperator.In, clrType, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(cr, filter.NotIn, ArrayComparisonOperator.NotIn, clrType, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    /// <summary>
    /// Binds an <see cref="EnumFilter{T}"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, null checks,
    /// and sorting if specified in the filter.
    /// Supports flexible enum filtering modes (name, numeric value, numeric value as string) via <see cref="EnumFilterMode"/>.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <typeparam name="T">The enum type to bind in the filter.</typeparam>
    /// <param name="filter">The <see cref="EnumFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindEnum<T>(EnumFilter<T>? filter, string column) where T : struct, Enum
    => BindEnum(filter, _fromTable, column);

    /// <summary>
    /// Binds an <see cref="EnumFilter{T}"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, null checks,
    /// and sorting if specified in the filter.
    /// Supports flexible enum filtering modes (name, numeric value, numeric value as string) via <see cref="EnumFilterMode"/>.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <typeparam name="T">The enum type to bind in the filter.</typeparam>
    /// <param name="filter">The <see cref="EnumFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder BindEnum<T>(EnumFilter<T>? filter, string table, string column)
        where T : struct, Enum
    {
        if (filter is null) return this;
        EnumValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _enumCounter++, _castTypeMapper);

        AddEnumComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, filter.FilterMode, _dialect.BuildEnumComparison);
        AddEnumComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, filter.FilterMode, _dialect.BuildEnumComparison);
        AddEnumArrayComparisonIfPresent(cr, filter.In, ArrayComparisonOperator.In, filter.FilterMode, _dialect.BuildEnumInArray);
        AddEnumArrayComparisonIfPresent(cr, filter.NotIn, ArrayComparisonOperator.NotIn, filter.FilterMode, _dialect.BuildEnumInArray);
        AddEnumNullCheckConditionIfPresent(cr, filter, filter.FilterMode);
        AddEnumSortIfPresent(cr, filter, filter.FilterMode);

        return this;
    }

    /// <summary>
    /// Binds a <see cref="FlagsEnumFilter{T}"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, flag checks (HasFlag, NotHasFlag),
    /// null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <typeparam name="T">The enum type with the <c>[Flags]</c> attribute to bind in the filter.</typeparam>
    /// <param name="filter">The <see cref="FlagsEnumFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindFlagsEnum<T>(FlagsEnumFilter<T>? filter, string column)
        where T : struct, Enum
    => BindFlagsEnum(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="FlagsEnumFilter{T}"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, flag checks (HasFlag, NotHasFlag),
    /// null checks, and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <typeparam name="T">The enum type with the <c>[Flags]</c> attribute to bind in the filter.</typeparam>
    /// <param name="filter">The <see cref="FlagsEnumFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder BindFlagsEnum<T>(FlagsEnumFilter<T>? filter, string table, string column)
        where T : struct, Enum
    {
        if (filter is null) return this;
        FlagsEnumValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _flagsEnumCounter++, _castTypeMapper);
        var clrType = typeof(int);

        AddFlagsEnumComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, clrType, _dialect.BuildCommonComparison);
        AddFlagsEnumComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, clrType, _dialect.BuildCommonComparison);
        AddFlagCheckIfPresent(cr, filter.HasFlag, FlagCheckMode.HasFlag, clrType, _dialect.BuildHasFlag);
        AddFlagCheckIfPresent(cr, filter.NotHasFlag, FlagCheckMode.NotHasFlag, clrType, _dialect.BuildNotHasFlag);
        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    /// <summary>
    /// Binds a <see cref="GuidFilter"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, null checks,
    /// and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The <see cref="GuidFilter"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindGuid(GuidFilter? filter, string column)
    => BindGuid(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="GuidFilter"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, null checks,
    /// and sorting if specified in the filter.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <param name="filter">The <see cref="GuidFilter"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder BindGuid(GuidFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        GuidValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _guidCounter++, _castTypeMapper);
        var clrType = typeof(Guid);

        AddComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, clrType, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(cr, filter.In, ArrayComparisonOperator.In, clrType, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(cr, filter.NotIn, ArrayComparisonOperator.NotIn, clrType, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    private static readonly HashSet<Type> SupportedNumericTypes = new()
    {
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(decimal),
        typeof(double),
        typeof(float),
    };

    /// <summary>
    /// Binds a <see cref="NumericFilter{T}"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, range comparisons, IN/NOT IN array checks,
    /// null checks, and sorting if specified in the filter.
    /// Supports only numeric types: <c>short</c>, <c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c>, and <c>float</c>.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric value type to bind in the filter.
    /// Must be one of the supported types: <c>short</c>, <c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c>, <c>float</c>.
    /// </typeparam>
    /// <param name="filter">The <see cref="NumericFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <typeparamref name="T"/> is not one of the supported numeric types.
    /// </exception>
    public FlectoBuilder BindNumeric<T>(NumericFilter<T>? filter, string column) where T : struct, IComparable
    => BindNumeric(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="NumericFilter{T}"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, range comparisons, IN/NOT IN array checks,
    /// null checks, and sorting if specified in the filter.
    /// Supports only numeric types: <c>short</c>, <c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c>, and <c>float</c>.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The numeric value type to bind in the filter.
    /// Must be one of the supported types: <c>short</c>, <c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c>, <c>float</c>.
    /// </typeparam>
    /// <param name="filter">The <see cref="NumericFilter{T}"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <typeparamref name="T"/> is not one of the supported numeric types.
    /// </exception>
    private FlectoBuilder BindNumeric<T>(NumericFilter<T>? filter, string table, string column) where T : struct, IComparable
    {
        if (filter is null) return this;
        NumericValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _numericCounter++, _castTypeMapper);

        if (!SupportedNumericTypes.Contains(typeof(T)))
            throw new ArgumentException(
                $"NumericFilter<{typeof(T).Name}> is not supported for column '{cr.SqlName}'.",
                nameof(filter));

        return BuildNumericFilter(cr, filter);
    }

    private FlectoBuilder BuildNumericFilter<T>(ColumnRef cr, NumericFilter<T> filter) where T : struct, IComparable
    {

        var clrType = typeof(T);

        AddComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Gt, ComparisonOperator.Gt, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Gte, ComparisonOperator.Gte, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Lt, ComparisonOperator.Lt, clrType, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(cr, filter.Lte, ComparisonOperator.Lte, clrType, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(cr, filter.In, ArrayComparisonOperator.In, clrType, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(cr, filter.NotIn, ArrayComparisonOperator.NotIn, clrType, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    /// <summary>
    /// Binds a <see cref="StringFilter"/> to the query for the specified column of the target table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, pattern matching
    /// (contains, starts with, ends with), null checks, and sorting if specified in the filter.
    /// Respects case sensitivity based on the <see cref="StringFilter.CaseSensitive"/> property.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> specifying the conditions to apply.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder BindString(StringFilter? filter, string column)
    => BindString(filter, _fromTable, column);

    /// <summary>
    /// Binds a <see cref="StringFilter"/> to the query for the specified column of the specified table,
    /// adding SQL conditions for equality, inequality, IN/NOT IN array checks, pattern matching
    /// (contains, starts with, ends with), null checks, and sorting if specified in the filter.
    /// Respects case sensitivity based on the <see cref="StringFilter.CaseSensitive"/> property.
    /// If <paramref name="filter"/> is <c>null</c>, this method has no effect.
    /// This overload will be made public when JOIN support is implemented for <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <param name="filter">The <see cref="StringFilter"/> specifying the conditions to apply.</param>
    /// <param name="table">The table containing the column to bind the filter to.</param>
    /// <param name="column">The column name to bind the filter to.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    private FlectoBuilder BindString(StringFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        StringValidator.EnsureValid(filter, table, column);

        var cr = new ColumnRef(table, column, _stringCounter++, _castTypeMapper);
        var clrType = typeof(string);

        AddStringComparisonIfPresent(cr, filter.Eq, ComparisonOperator.Eq, filter.CaseSensitive, clrType, _dialect.BuildStringEquals);
        AddStringComparisonIfPresent(cr, filter.NotEq, ComparisonOperator.NotEq, filter.CaseSensitive, clrType, _dialect.BuildStringNotEquals);
        AddStringArrayComparisonIfPresent(cr, filter.In, ArrayComparisonOperator.In, filter.CaseSensitive, clrType, _dialect.BuildStringInArray);
        AddStringArrayComparisonIfPresent(cr, filter.NotIn, ArrayComparisonOperator.NotIn, filter.CaseSensitive, clrType, _dialect.BuildStringNotInArray);
        AddMatchComparisonIfPresent(cr, filter.Contains, StringMatchType.Contains, filter.CaseSensitive, clrType, _dialect.BuildStringLike);
        AddMatchComparisonIfPresent(cr, filter.StartsWith, StringMatchType.StartsWith, filter.CaseSensitive, clrType, _dialect.BuildStringLike);
        AddMatchComparisonIfPresent(cr, filter.EndsWith, StringMatchType.EndsWith, filter.CaseSensitive, clrType, _dialect.BuildStringLike);
        AddNullCheckConditionIfPresent(cr, filter, clrType);
        AddSortIfPresent(cr, filter, clrType);

        return this;
    }

    #endregion Bind

    /// <summary>
    /// Applies pagination to the query using the specified <see cref="PaginationFilter"/>.
    /// Calculates the <c>LIMIT</c> and <c>OFFSET</c> based on the filter,
    /// where offset is computed as <c>Limit * (Page - 1)</c>.
    /// Validation is performed via <see cref="PaginationValidator"/> to ensure correctness.
    /// </summary>
    /// <param name="filter">The <see cref="PaginationFilter"/> specifying the page number and page size to apply.</param>
    /// <returns>The current <see cref="FlectoBuilder"/> instance for chaining.</returns>
    public FlectoBuilder ApplyPaging(PaginationFilter filter)
    {
        PaginationValidator.EnsureValid(filter);

        _paging = (filter.Limit, filter.Limit * (filter.Page - 1));

        return this;
    }

    // 999 cover tests

    /// <summary>
    /// Creates a deep copy of the current <see cref="FlectoBuilder"/> instance,
    /// duplicating all query state including select columns, conditions, pagination,
    /// parameters, and sorting configurations.
    /// This allows further modifications on the cloned instance without affecting the original instance.
    /// </summary>
    /// <returns>A new <see cref="FlectoBuilder"/> instance with the same configuration as the current instance.</returns>
    public FlectoBuilder Clone()
    {
        var clone = new FlectoBuilder(_fromTable, _dialectType);

        clone._selectColumns = _selectColumns;
        clone._selectWasSet = _selectWasSet;

        clone._searchCounter = _searchCounter;
        clone._boolCounter = _boolCounter;
        clone._dateCounter = _dateCounter;
        clone._enumCounter = _enumCounter;
        clone._flagsEnumCounter = _flagsEnumCounter;
        clone._numericCounter = _numericCounter;
        clone._guidCounter = _guidCounter;
        clone._stringCounter = _stringCounter;


        clone._exceptOrderBy = _exceptOrderBy;

        clone._paging = _paging;

        clone._conditions = new List<string>(_conditions);

        foreach (var paramName in _parameters.ParameterNames)
        {
            var paramValue = _parameters.Get<object?>(paramName);
            clone._parameters.Add(paramName, paramValue);
        }

        foreach (var kvp in _sortFields)
        {
            clone._sortFields.Add(kvp.Key, kvp.Value);
        }

        return clone;
    }

    /// <summary>
    /// Builds the final SQL query and its associated parameters based on the current state of the <see cref="FlectoBuilder"/>.
    /// </summary>
    /// <returns>
    /// A tuple containing the generated SQL query string and the <see cref="DynamicParameters"/> for parameterized execution.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if no <c>SELECT</c> clause was specified before building the query.
    /// Ensure that <see cref="Select()"/>, <see cref="SelectAll()"/>, or <see cref="SelectCount()"/> is called before <see cref="Build()"/>.
    /// </exception>
    public (string Sql, DynamicParameters Parameters) Build()
    {
        if (_selectColumns == null)
            throw new NullReferenceException("Cannot build query: no SELECT clause specified. Call Select(), SelectAll(), or SelectCount() before building the query.");

        var sql = new StringBuilder($"{_selectColumns} {Sql.FROM} {_fromTable}");

        if (_conditions.Any())
        {
            sql.Append($" {Sql.WHERE} ");
            sql.Append(string.Join($" {Sql.AND} ", _conditions));
        }

        AppendOrderByIfPresent(sql);
        AppendPagingIfPresent(sql);

        return (sql.ToString(), _parameters);
    }

    /// <summary>
    /// Appends the <c>ORDER BY</c> clause to the SQL if sorting fields are specified
    /// and ordering is not explicitly suppressed via <c>_exceptOrderBy</c>.
    /// Sorting fields are ordered by their specified <see cref="Sort.Position"/> and direction (ASC/DESC).
    /// </summary>
    /// <param name="sql">The <see cref="StringBuilder"/> containing the SQL to append to.</param>
    private void AppendOrderByIfPresent(StringBuilder sql)
    {
        if (_exceptOrderBy || _sortFields.Count == 0) return;

        sql.Append($" {Sql.ORDER_BY} ");

        var values = _sortFields
            .OrderBy(x => x.Value.Position)
            .Select(x => $"{x.Key} {(x.Value.Descending ? Sql.DESC : Sql.ASC)}");

        sql.Append(string.Join(", ", values));
    }

    /// <summary>
    /// Appends the <c>LIMIT</c> and <c>OFFSET</c> clauses to the SQL if pagination is specified.
    /// Binds the limit and offset values to the <see cref="_parameters"/> dictionary for parameterized execution.
    /// </summary>
    /// <param name="sql">The <see cref="StringBuilder"/> containing the SQL to append to.</param>
    private void AppendPagingIfPresent(StringBuilder sql)
    {
        if (_paging == null) return;

        sql.Append($" {Sql.LIMIT} @_Limit");
        _parameters.Add("_Limit", _paging.Value.Limit);

        sql.Append($" {Sql.OFFSET} @_Offset");
        _parameters.Add("_Offset", _paging.Value.Offset);
    }

    #region AddSmthIfPresent

    private void AddSortIfPresent(ColumnRef cr, IQueryFilter filter, Type clrType)
    {
        if (!filter.Sort.HasValue) return;
        _sortFields.Add(cr.SqlName(clrType), filter.Sort.Value);
    }

    private void AddEnumSortIfPresent(ColumnRef cr, IQueryFilter filter, EnumFilterMode filterMode)
    {
        if (!filter.Sort.HasValue) return;
        _sortFields.Add(cr.SqlNameForEnum(filterMode), filter.Sort.Value);
    }

    private void AddEnumNullCheckConditionIfPresent(
        ColumnRef cr,
        IQueryFilter filter,
        EnumFilterMode filterMode)
    {
        if (!filter.IsNull.HasValue) return;

        var condition = _dialect.BuildCommonNullCheck(cr.SqlNameForEnum(filterMode), filter.IsNull.Value);
        AddCondition(condition);
    }

    private void AddNullCheckConditionIfPresent(ColumnRef cr, IQueryFilter filter, Type clrType)
    {
        if (!filter.IsNull.HasValue) return;

        var condition = _dialect.BuildCommonNullCheck(cr.SqlName(clrType), filter.IsNull.Value);
        AddCondition(condition);
    }

    private void AddComparisonIfPresent<T>(
        ColumnRef cr,
        T? value,
        ComparisonOperator op,
        Type clrType,
        Func<string, string, ComparisonOperator, string> buildConditionSql) where T : struct
    {
        if (!value.HasValue) return;

        var param = cr.GetParamName(op);
        var condition = buildConditionSql(cr.SqlName(clrType), param, op);
        AddCondition(condition, param, value.Value);
    }

    private void AddFlagsEnumComparisonIfPresent<T>(
        ColumnRef cr,
        T? value,
        ComparisonOperator op,
        Type clrType,
        Func<string, string, ComparisonOperator, string> buildConditionSql) where T : struct
    {
        if (!value.HasValue) return;

        var param = cr.GetParamName(op);
        var condition = buildConditionSql(cr.SqlName(clrType), param, op);
        AddCondition(condition, param, Convert.ToInt64(value.Value));
    }

    private void AddFlagCheckIfPresent<T>(
        ColumnRef cr,
        T? value,
        FlagCheckMode mode,
        Type clrType,
        Func<string, string, string> buildConditionSql) where T : struct, Enum
    {
        if (!value.HasValue) return;

        var param = cr.GetParamName(mode);
        var condition = buildConditionSql(cr.SqlName(clrType), param);
        AddCondition(condition, param, Convert.ToInt64(value.Value));
    }

    private void AddArrayComparisonIfPresent<T>(
        ColumnRef cr,
        T[]? value,
        ArrayComparisonOperator op,
        Type clrType,
        Func<string, string, string> buildConditionSql) where T : struct
    {
        if (value is null || !value.Any()) return;

        var param = cr.GetParamName(op);
        var condition = buildConditionSql(cr.SqlName(clrType), param);
        AddCondition(condition, param, value);

    }

    private void AddStringArrayComparisonIfPresent(
        ColumnRef cr,
        string[]? value,
        ArrayComparisonOperator op,
        bool caseSensitive,
        Type clrType,
        Func<string, string, string[], bool, (string, string[])> buildConditionSql)
    {
        if (value is null || !value.Any()) return;

        var param = cr.GetParamName(op);
        var (condition, values) = buildConditionSql(cr.SqlName(clrType), param, value, caseSensitive);
        AddCondition(condition, param, values);

    }

    private void AddMatchComparisonIfPresent(
        ColumnRef cr,
        string? value,
        StringMatchType matchType,
        bool caseSensitive,
        Type clrType,
        Func<string, string, string, StringMatchType, bool, (string, string)> buildConditionSql)
    {
        if (value is null) return;

        var param = cr.GetParamName(matchType);
        var (condition, values) = buildConditionSql(cr.SqlName(clrType), param, value, matchType, caseSensitive);
        AddCondition(condition, param, values);
    }

    private void AddStringComparisonIfPresent(
        ColumnRef cr,
        string? value,
        ComparisonOperator op,
        bool caseSensitive,
        Type clrType,
        Func<string, string, string, bool, (string, string)> buildConditionSql)
    {
        if (value is null) return;

        SqlOperatorHelper.EnsureEqualityOperator(op);

        var param = cr.GetParamName(op);
        var (condition, values) = buildConditionSql(cr.SqlName(clrType), param, value, caseSensitive);
        AddCondition(condition, param, values);

    }

    private void AddBoolComparisonIfPresent(
        ColumnRef cr,
        bool? value,
        ComparisonOperator op,
        Type clrType,
        Func<string, string, ComparisonOperator, string> buildConditionSql)
    {
        if (value is null) return;

        SqlOperatorHelper.EnsureEqualityOperator(op);

        var param = cr.GetParamName(op);
        var condition = buildConditionSql(cr.SqlName(clrType), param, op);
        AddCondition(condition, param, value);

    }

    private void AddEnumComparisonIfPresent<T>(
        ColumnRef cr,
        T? data,
        ComparisonOperator op,
        EnumFilterMode filterMode,
        Func<string, string, T, ComparisonOperator, EnumFilterMode, (string, object)> buildConditionSql)
        where T : struct, Enum
    {
        if (!data.HasValue) return;

        SqlOperatorHelper.EnsureEqualityOperator(op);

        var param = cr.GetParamName(op);
        var (condition, value) = buildConditionSql(cr.SqlNameForEnum(filterMode), param, data.Value, op, filterMode);
        AddCondition(condition, param, value);
    }

    private void AddEnumArrayComparisonIfPresent<T>(
        ColumnRef cr,
        T[]? rowArr,
        ArrayComparisonOperator op,
        EnumFilterMode filterMode,
        Func<string, string, T[], EnumFilterMode, (string, object[])> buildConditionSql)
        where T : struct, Enum
    {
        if (rowArr is null || !rowArr.Any()) return;

        var param = cr.GetParamName(op);
        var (condition, values) = buildConditionSql(cr.SqlNameForEnum(filterMode), param, rowArr, filterMode);
        AddCondition(condition, param, values);

    }

    #endregion

    private void AddCondition(string condition, string? paramName = null, object? value = null)
    {
        // test: validate, that: is it definitely possible to add to conditioned if the parameter is null? 
        _conditions.Add(condition);
        if (paramName != null)
            _parameters.Add(paramName, value);
    }
}
