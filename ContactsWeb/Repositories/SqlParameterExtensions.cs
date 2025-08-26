using Microsoft.Data.SqlClient;
using System.Data;

namespace ContactsWeb.Repositories;

public static class SqlParameterExtensions
{
    public static void AddParam(this SqlParameterCollection parameters, string name, SqlDbType type, object? value, int size = 0, byte precision = 0, byte scale = 0)
    {
        SqlParameter p = size > 0
            ? parameters.Add(name, type, size)
            : parameters.Add(name, type);

        if (precision > 0) p.Precision = precision;
        if (scale > 0) p.Scale = scale;

        p.Value = value ?? DBNull.Value;
    }
}
