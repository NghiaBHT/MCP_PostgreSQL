using System;

namespace ConsoleMcpPostgreSQL.Models;

public class DbSchema
{
    public string? TableName { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
}
