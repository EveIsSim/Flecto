using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.SqlDialect;

namespace Flecto.Dapper.Models;

internal readonly struct ColumnRef
{
    internal string Table { get; }
    internal string Column { get; }
    private readonly int _index;
    private readonly ICastTypeMapper _castTypeMapper;

    public ColumnRef(string table, string column, int index, ICastTypeMapper castTypeMapper)
    {
        Table = table;
        Column = column;
        _index = index;
        _castTypeMapper = castTypeMapper;
    }

    internal string SqlName(Type clrType) => _castTypeMapper.GetSqlName(this, clrType);
    internal string SqlNameForEnum(EnumFilterMode mode) => _castTypeMapper.GetSqlNameForEnum(this, mode);

    internal string GetParamName()
    => $"{Table}_{Sanitize(Column)}_{_index}";

    internal string GetParamName<T>(T enumOp) where T : struct, Enum
    => $"{GetParamNameWithoutIndex()}_{enumOp.ToString()}_{_index}";

    private string GetParamNameWithoutIndex()
    => $"{Table}_{Sanitize(Column)}";

    private static string Sanitize(string input)
    {
        return input
            .Replace("'", "")
            .Replace("->>", "_")
            .Replace("->", "_")
            .Replace("::", "_");
    }

    internal bool IsJsonPath => Column.Contains("->") || Column.Contains("->>");
}
