using System.Runtime.CompilerServices;

/// <summary>
/// Grants the <c>Flecto.Dapper</c> assembly access to the internal members of this assembly.
/// This is used to allow the Dapper integration layer to utilize and test internal types within the FlectoBuilder
/// without making those types public.
/// </summary>
[assembly: InternalsVisibleTo("Flecto.Dapper")]

[assembly: InternalsVisibleTo("Flecto.Core.UnitTests")]

