namespace Flecto.Core.Models.Select;

public readonly struct Field
{
    public string Column { get; }
    public string? Alias { get; }

    public Field(string column, string? alias = null)
    {
        Column = column;
        Alias = alias;
    }
}
