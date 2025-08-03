using Flecto.Core.Models.Filters.Enums;
using Flecto.Dapper.Models;

namespace Flecto.Dapper.SqlDialect;


internal interface ICastTypeMapper
{
    string GetSqlName(ColumnRef columnRef, Type clrType);
    string GetSqlNameForEnum(ColumnRef columnRef, EnumFilterMode mode);
}
