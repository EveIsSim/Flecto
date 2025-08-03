namespace Flecto.Core.Models.Select;

public readonly struct FromTable
{
    public string Table { get; }
    public Field[] Fields { get; }

    public FromTable(string table, params Field[] fields)
    {
        Table = table;
        Fields = fields;
    }
}
