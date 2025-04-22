using System;

namespace ConsoleMcpPostgreSQL.Models;

public class SchemaResponse
{
    public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
}
