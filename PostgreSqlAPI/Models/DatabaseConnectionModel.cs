using System;

namespace PostgreSqlAPI.Models;

public class DatabaseConnectionModel
{
    public string Host { get; set; } = "localhost";
    public string Port { get; set; } = "5432";
    public string Database { get; set; } = "CompanyEmployee";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "20122001";
}
