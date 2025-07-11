namespace Flecto.Core.Models.Filters;

/// <summary>
/// Represents sorting information for a property in a query
/// </summary>
public readonly struct Sort
{
    /// <summary>
    /// Gets the sort position for the property in the query
    /// </summary>
    public uint Position { get; }
    /// <summary>
    /// Gets a value indicating whether the sort direction is descending
    /// </summary>
    public bool Descending { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sort"/> struct with the specified position and direction
    /// </summary>
    /// <param name="position">The sort position for the property in the query</param>
    /// <param name="descending">A value indicating whether the sort direction is descending</param>
    public Sort(uint position, bool descending)
    {
        Position = position;
        Descending = descending;
    }
}
