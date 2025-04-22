using System;

namespace ConsoleMcpPostgreSQL.Models;

// Model classes for API responses
public class ColumnSchema
{
    public string? Name { get; set; }
    public string? Type { get; set; }
}