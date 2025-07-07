namespace EveIsSim.QueryBuilder.Core.Models.Filters;


public readonly struct Sort
{
    public uint Position { get; }
    public bool Descending { get; }

    public Sort(uint position, bool descending)
    {
        Position = position;
        Descending = descending;
    }
}
