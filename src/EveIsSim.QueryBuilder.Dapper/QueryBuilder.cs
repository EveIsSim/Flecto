using System.Text;
using Dapper;
using EveIsSim.QueryBuilder.Core.Enums;
using EveIsSim.QueryBuilder.Core.Models.Filters;
using EveIsSim.QueryBuilder.Core.Models.Filters.Enums;
using EveIsSim.QueryBuilder.Core.Validators;
using EveIsSim.QueryBuilder.Dapper.Commons;
using EveIsSim.QueryBuilder.Dapper.Constants;
using EveIsSim.QueryBuilder.Dapper.SqlDialect;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Enums;
using EveIsSim.QueryBuilder.Dapper.SqlDialect.Helpers;

namespace EveIsSim.QueryBuilder.Dapper;


public class QueryBuilder
{
    private readonly ISqlDialect _dialect;
    private readonly DialectType _dialectType;

    private string? _selectColumns;
    private bool _selectWasSet = false;

    private int _searchConditionCounter = 0;

    private readonly string _fromTable;
    private bool _exceptOrderBy;
    private (int Limit, int Offset)? _paging;
    private readonly List<string> _conditions = new();
    private readonly DynamicParameters _parameters = new();
    private readonly Dictionary<string, Sort> _sortFields = new();

    private const string DefaultVectorConfig = "simple";


    public QueryBuilder(string fromTable, DialectType dialectType)
    {
        if (string.IsNullOrWhiteSpace(fromTable))
            throw new NullReferenceException("FromTable is required and cannot be null or empty");

        _fromTable = fromTable;

        _dialect = dialectType switch
        {
            DialectType.Postgres => new PostgresSqlDialect(),
            _ => throw new ArgumentOutOfRangeException(nameof(dialectType), dialectType, null)
        };

        _dialectType = dialectType;
    }

    #region Select

    public QueryBuilder SelectAll()
    {
        SelectValidator.EnsureValid(_selectWasSet);

        _selectColumns = $"{Sql.SELECT} *";
        _exceptOrderBy = false;
        _selectWasSet = true;

        return this;
    }

    public QueryBuilder SelectCount()
    {
        SelectValidator.EnsureValid(_selectWasSet);

        _selectColumns = $"{Sql.SELECT} COUNT(*)";
        _exceptOrderBy = true;
        _selectWasSet = true;

        return this;
    }

    // doc: will attach to fromTable
    public QueryBuilder Select(params string[] columns)
    => Select((_fromTable, columns));

    // It will public when we implement JOIN ON logic
    private QueryBuilder Select(params (string Table, string[] Columns)[] tablesWithColumns)
    {
        SelectValidator.EnsureValid(_selectWasSet, tablesWithColumns);

        var columns = tablesWithColumns
            .SelectMany(tc => tc.Columns
                .Select(c => tc.Table + "." + c));

        _selectColumns = $"{Sql.SELECT} {string.Join(", ", columns)}";
        _exceptOrderBy = false;
        _selectWasSet = true;

        return this;
    }

    #endregion Select

    #region Search

    public QueryBuilder Search(SearchFilter? filter, params string[] columns)
        => Search(filter, _fromTable, columns);

    public QueryBuilder Search(SearchFilter? filter, string table, params string[] columns)
        => Search(filter, (table, columns));

    // It will public when we implement JOIN ON logic
    // there may be cases where Search may be requested twice. Check this in tests.
    private QueryBuilder Search(
        SearchFilter? filter,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (filter is null) return this;
        SearchValidator.EnsureValid(filter, tablesWithColumns);

        const string prefix = "search_param_";
        var paramName = Common.GenSearchParamName(prefix, _searchConditionCounter++);
        var result = _dialect.BuildSearch(filter, paramName, tablesWithColumns);

        AddCondition(result.SqlCondition, paramName, result.ParamValue);

        return this;
    }

    public QueryBuilder SearchTsVector(
        SearchFilter? filter,
        string[] columns,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig)
    => SearchTsVector(filter, _fromTable, columns, mode, config);

    public QueryBuilder SearchTsVector(
        SearchFilter? filter,
        string table,
        string[] columns,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig)
    => SearchTsVector(filter, mode, config, (table, columns));

    // did not check it, need check and tests 
    public QueryBuilder SearchTsVector(
        SearchFilter? filter,
        TextSearchMode mode = TextSearchMode.Plain,
        string config = DefaultVectorConfig,
        params (string Table, string[] Columns)[] tablesWithColumns)
    {
        if (filter is null) return this;
        SearchValidator.EnsureValidTsVector(filter, tablesWithColumns, _dialectType);

        const string prefix = "tsvector_query_";
        var paramName = Common.GenSearchParamName(prefix, _searchConditionCounter);
        var condition = _dialect.BuildTsVectorSearchCondition(paramName, mode, config, tablesWithColumns);
        AddCondition(condition, paramName, filter.Value);

        return this;
    }

    #endregion

    #region Bind

    public QueryBuilder BindBool(BoolFilter? filter, string column)
    => BindBool(filter, _fromTable, column);

    public QueryBuilder BindBool(BoolFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        BoolValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddNullCheckConditionIfPresent(c, filter);
        AddBoolComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, _dialect.BuildBoolComparison);
        AddBoolComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, _dialect.BuildBoolComparison);
        AddSortIfPresent(c, filter);

        return this;
    }

    public QueryBuilder BindDate(DateFilter? filter, string column)
    => BindDate(filter, _fromTable, column);

    public QueryBuilder BindDate(DateFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        DateValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.Gt, ComparisonOperator.Gt, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.Gte, ComparisonOperator.Gte, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.Lt, ComparisonOperator.Lt, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.Lte, ComparisonOperator.Lte, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(c, filter.In, ArrayComparisonOperator.In, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(c, filter.NotIn, ArrayComparisonOperator.NotIn, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(c, filter);
        AddSortIfPresent(c, filter);

        return this;
    }

    public QueryBuilder BindEnum<T>(EnumFilter<T>? filter, string column) where T : struct, Enum
    => BindEnum(filter, _fromTable, column);

    public QueryBuilder BindEnum<T>(EnumFilter<T>? filter, string table, string column)
        where T : struct, Enum
    {
        if (filter is null) return this;
        EnumValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddEnumComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, filter.FilterMode, _dialect.BuildEnumComparison);
        AddEnumComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, filter.FilterMode, _dialect.BuildEnumComparison);
        AddEnumArrayComparisonIfPresent(c, filter.In, ArrayComparisonOperator.In, filter.FilterMode, _dialect.BuildEnumInArray);
        AddEnumArrayComparisonIfPresent(c, filter.NotIn, ArrayComparisonOperator.NotIn, filter.FilterMode, _dialect.BuildEnumInArray);
        AddNullCheckConditionIfPresent(c, filter);
        AddSortIfPresent(c, filter);

        return this;
    }

    public QueryBuilder BindFlagsEnum<T>(FlagsEnumFilter<T>? filter, string column)
        where T : struct, Enum
    => BindFlagsEnum(filter, _fromTable, column);

    public QueryBuilder BindFlagsEnum<T>(FlagsEnumFilter<T>? filter, string table, string column)
        where T : struct, Enum
    {
        if (filter is null) return this;
        FlagsEnumFilter.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddFlagsEnumComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, _dialect.BuildCommonComparison);
        AddFlagsEnumComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, _dialect.BuildCommonComparison);
        AddFlagCheckIfPresent(c, filter.HasFlag, FlagCheckMode.HasFlag, _dialect.BuildHasFlag);
        AddFlagCheckIfPresent(c, filter.NotHasFlag, FlagCheckMode.NotHasFlag, _dialect.BuildNotHasFlag);
        AddNullCheckConditionIfPresent(c, filter);
        AddSortIfPresent(c, filter);

        return this;
    }

    public QueryBuilder BindGuid(GuidFilter? filter, string column)
    => BindGuid(filter, _fromTable, column);

    public QueryBuilder BindGuid(GuidFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        GuidValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(c, filter.In, ArrayComparisonOperator.In, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(c, filter.NotIn, ArrayComparisonOperator.NotIn, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(c, filter);
        AddSortIfPresent(c, filter);

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

    public QueryBuilder BindNumeric<T>(NumericFilter<T>? filter, string column) where T : struct, IComparable
    => BindNumeric(filter, _fromTable, column);

    public QueryBuilder BindNumeric<T>(NumericFilter<T>? filter, string table, string column) where T : struct, IComparable
    {
        if (filter is null) return this;
        NumericValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        if (!SupportedNumericTypes.Contains(typeof(T)))
            throw new ArgumentException(
                $"NumericFilter<{typeof(T).Name}> is not supported for column '{c}'.",
                nameof(filter));

        return BuildNumericFilter(c, filter);
    }

    private QueryBuilder BuildNumericFilter<T>(string column, NumericFilter<T> filter) where T : struct, IComparable
    {
        AddComparisonIfPresent(column, filter.Eq, ComparisonOperator.Eq, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(column, filter.NotEq, ComparisonOperator.NotEq, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(column, filter.Gt, ComparisonOperator.Gt, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(column, filter.Gte, ComparisonOperator.Gte, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(column, filter.Lt, ComparisonOperator.Lt, _dialect.BuildCommonComparison);
        AddComparisonIfPresent(column, filter.Lte, ComparisonOperator.Lte, _dialect.BuildCommonComparison);
        AddArrayComparisonIfPresent(column, filter.In, ArrayComparisonOperator.In, _dialect.BuildCommonInArray);
        AddArrayComparisonIfPresent(column, filter.NotIn, ArrayComparisonOperator.NotIn, _dialect.BuildCommonNotInArray);
        AddNullCheckConditionIfPresent(column, filter);
        AddSortIfPresent(column, filter);

        return this;
    }

    public QueryBuilder BindString(StringFilter? filter, string column)
    => BindString(filter, _fromTable, column);

    public QueryBuilder BindString(StringFilter? filter, string table, string column)
    {
        if (filter is null) return this;
        StringValidator.EnsureValid(filter, table, column);

        var c = Common.CombineColumn(table, column);

        AddStringComparisonIfPresent(c, filter.Eq, ComparisonOperator.Eq, filter.CaseSensitive, _dialect.BuildStringEquals);
        AddStringComparisonIfPresent(c, filter.NotEq, ComparisonOperator.NotEq, filter.CaseSensitive, _dialect.BuildStringNotEquals);
        AddStringArrayComparisonIfPresent(c, filter.In, ArrayComparisonOperator.In, filter.CaseSensitive, _dialect.BuildStringInArray);
        AddStringArrayComparisonIfPresent(c, filter.NotIn, ArrayComparisonOperator.NotIn, filter.CaseSensitive, _dialect.BuildStringNotInArray);
        AddMatchComparisonIfPresent(c, filter.Contains, StringMatchType.Contains, filter.CaseSensitive, _dialect.BuildStringLike);
        AddMatchComparisonIfPresent(c, filter.StartsWith, StringMatchType.StartsWith, filter.CaseSensitive, _dialect.BuildStringLike);
        AddMatchComparisonIfPresent(c, filter.EndsWith, StringMatchType.EndsWith, filter.CaseSensitive, _dialect.BuildStringLike);
        AddNullCheckConditionIfPresent(c, filter);
        AddSortIfPresent(c, filter);

        return this;
    }

    #endregion Bind

    public QueryBuilder ApplyPaging(PaginationFilter filter)
    {
        PaginationValidator.EnsureValid(filter);

        _paging = (filter.Limit, filter.Limit * (filter.Page - 1));

        return this;
    }

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

    private void AppendOrderByIfPresent(StringBuilder sql)
    {
        if (_exceptOrderBy || _sortFields.Count == 0) return;

        sql.Append($" {Sql.ORDER_BY} ");

        var values = _sortFields
            .OrderBy(x => x.Value.Position)
            .Select(x => $"{x.Key} {(x.Value.Descending ? Sql.DESC : Sql.ASC)}");

        sql.Append(string.Join(", ", values));
    }

    private void AppendPagingIfPresent(StringBuilder sql)
    {
        if (_paging == null) return;

        sql.Append($" {Sql.LIMIT} @_Limit");
        _parameters.Add("_Limit", _paging.Value.Limit);

        sql.Append($" {Sql.OFFSET} @_Offset");
        _parameters.Add("_Offset", _paging.Value.Offset);
    }

    #region AddSmthIfPresent

    private void AddSortIfPresent(string column, IQueryFilter filter)
    {
        if (filter.Sort == null) return;
        _sortFields.Add(column, filter.Sort);
    }

    private void AddNullCheckConditionIfPresent(string column, IQueryFilter filter)
    {
        if (!filter.Null.HasValue) return;

        var condition = _dialect.BuildCommonNullCheck(column, filter.Null.Value);
        AddCondition(condition);
    }

    private void AddComparisonIfPresent<T>(
        string column,
        T? value,
        ComparisonOperator op,
        Func<string, string, ComparisonOperator, string> buildConditionSql) where T : struct
    {
        if (!value.HasValue) return;

        var param = GetParamName(column, op);
        var condition = buildConditionSql(column, param, op);
        AddCondition(condition, param, value.Value);
    }

    private void AddFlagsEnumComparisonIfPresent<T>(
        string column,
        T? value,
        ComparisonOperator op,
        Func<string, string, ComparisonOperator, string> buildConditionSql) where T : struct
    {
        if (!value.HasValue) return;

        var param = GetParamName(column, op);
        var condition = buildConditionSql(column, param, op);
        AddCondition(condition, param, Convert.ToInt64(value.Value));
    }

    private void AddFlagCheckIfPresent<T>(
        string column,
        T? value,
        FlagCheckMode mode,
        Func<string, string, string> buildConditionSql) where T : struct, Enum
    {
        if (!value.HasValue) return;

        var param = GetParamName(column, mode);
        var condition = buildConditionSql(column, param);
        AddCondition(condition, param, Convert.ToInt64(value.Value));
    }

    private void AddArrayComparisonIfPresent<T>(
        string column,
        T[]? value,
        ArrayComparisonOperator op,
        Func<string, string, string> buildConditionSql) where T : struct
    {
        if (value is null || !value.Any()) return;

        var param = GetParamName(column, op);
        var condition = buildConditionSql(column, param);
        AddCondition(condition, param, value);

    }

    private void AddStringArrayComparisonIfPresent(
        string column,
        string[]? value,
        ArrayComparisonOperator op,
        bool caseSensitive,
        Func<string, string, string[], bool, (string, string[])> buildConditionSql)
    {
        if (value is null || !value.Any()) return;

        var param = GetParamName(column, op);
        var (condition, values) = buildConditionSql(column, param, value, caseSensitive);
        AddCondition(condition, param, values);

    }

    private void AddMatchComparisonIfPresent(
        string column,
        string? value,
        StringMatchType matchType,
        bool caseSensitive,
        Func<string, string, string, StringMatchType, bool, (string, string)> buildConditionSql)
    {
        if (value is null) return;

        var param = GetParamName(column, matchType);
        var (condition, values) = buildConditionSql(column, param, value, matchType, caseSensitive);
        AddCondition(condition, param, values);
    }

    private void AddStringComparisonIfPresent(
        string column,
        string? value,
        ComparisonOperator op,
        bool caseSensitive,
        Func<string, string, string, bool, (string, string)> buildConditionSql)
    {
        SqlOperatorHelper.EnsureEqualityOperator(op);

        if (value is null) return;

        var param = GetParamName(column, op);
        var (condition, values) = buildConditionSql(column, param, value, caseSensitive);
        AddCondition(condition, param, values);

    }

    private void AddBoolComparisonIfPresent(
        string column,
        bool? value,
        ComparisonOperator op,
        Func<string, string, ComparisonOperator, string> buildConditionSql)
    {
        SqlOperatorHelper.EnsureEqualityOperator(op);

        if (value is null) return;

        var param = GetParamName(column, op);
        var condition = buildConditionSql(column, param, op);
        AddCondition(condition, param, value);

    }

    private void AddEnumComparisonIfPresent<T>(
        string column,
        T? data,
        ComparisonOperator op,
        EnumFilterMode filterMode,
        Func<string, string, T, ComparisonOperator, EnumFilterMode, (string, object)> buildConditionSql)
        where T : struct, Enum
    {
        SqlOperatorHelper.EnsureEqualityOperator(op);

        if (!data.HasValue) return;

        var param = GetParamName(column, op);
        var (condition, value) = buildConditionSql(column, param, data.Value, op, filterMode);
        AddCondition(condition, param, value);
    }

    private void AddEnumArrayComparisonIfPresent<T>(
        string column,
        T[]? rowArr,
        ArrayComparisonOperator op,
        EnumFilterMode filterMode,
        Func<string, string, T[], EnumFilterMode, (string, object[])> buildConditionSql)
        where T : struct, Enum
    {
        if (rowArr is null || !rowArr.Any()) return;

        var param = GetParamName(column, op);
        var (condition, values) = buildConditionSql(column, param, rowArr, filterMode);
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

    #region BuildParamName

    private string GetParamName<T>(string column, T enumOp) where T : struct, Enum
    => BuildParamName(_fromTable, column, enumOp);

    private static string BuildParamName<T>(string table, string column, T enumOp) where T : struct, Enum
    => $"{table}_{column}_{enumOp.ToString()}";

    #endregion
}
