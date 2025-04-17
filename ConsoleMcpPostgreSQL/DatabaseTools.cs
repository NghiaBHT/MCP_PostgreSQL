using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleMcpPostgreSQL
{
    [McpServerToolType]
    public class DatabaseTools
    {
        private readonly DatabaseService _databaseService;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public DatabaseTools(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        [McpServerTool, Description("Execute a custom SQL query against the PostgreSQL database")]
        public async Task<string> ExecuteQuery(string sql)
        {
            try
            {
                var results = await _databaseService.ExecuteQueryAsync(sql);
                return JsonSerializer.Serialize(results, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to execute query: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get a list of all tables in the PostgreSQL database")]
        public async Task<string> GetTables()
        {
            try
            {
                var tables = await _databaseService.GetTablesAsync();
                return JsonSerializer.Serialize(tables, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to get tables: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Get the schema information for a specific table in the PostgreSQL database")]
        public async Task<string> GetSchema(string schema, string table)
        {
            try
            {
                var columns = await _databaseService.GetTableSchemaAsync(schema, table);
                return JsonSerializer.Serialize(columns, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to get schema: {ex.Message}", ex);
            }
        }

        [McpServerTool, Description("Retrieve data from a specific table in the PostgreSQL database")]
        public async Task<string> GetData(string schema, string table)
        {
            try
            {
                var data = await _databaseService.GetTableDataAsync(schema, table);
                return JsonSerializer.Serialize(data, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to get data: {ex.Message}", ex);
            }
        }
    }
}
