using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleMcpPostgreSQL
{
    public class DatabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DatabaseService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public DatabaseService(HttpClient httpClient, ILogger<DatabaseService> logger, string baseUrl = "http://localhost:5000")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Executes a custom SQL query against the database
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <returns>The query results as dynamic objects</returns>
        public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql)
        {
            try
            {
                var request = new
                {
                    Sql = sql
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Mcp/query", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<dynamic>>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", sql);
                throw;
            }
        }

        /// <summary>
        /// Gets the schema for a specific table
        /// </summary>
        /// <param name="schema">The database schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>The table schema information</returns>
        public async Task<IEnumerable<ColumnSchema>> GetTableSchemaAsync(string schema, string table)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Mcp/schema/{schema}/{table}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SchemaResponse>(responseContent, _jsonOptions);
                return result?.Columns ?? new List<ColumnSchema>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting schema for {Schema}.{Table}", schema, table);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of all tables in the database
        /// </summary>
        /// <returns>A list of table information</returns>
        public async Task<IEnumerable<TableSchema>> GetTablesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Mcp/tables");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<TableSchema>>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting tables list");
                throw;
            }
        }

        /// <summary>
        /// Gets data from a specific table
        /// </summary>
        /// <param name="schema">The database schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>The table data as dynamic objects</returns>
        public async Task<IEnumerable<dynamic>> GetTableDataAsync(string schema, string table)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Mcp/data/{schema}/{table}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<dynamic>>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting data for {Schema}.{Table}", schema, table);
                throw;
            }
        }

        public async Task<IEnumerable<DbSchema>> GetTableAndColumnInfoAsync()
        {
            try
            {
                // Fetch all tables
                var tables = await GetTablesAsync();
                var dbSchemas = new List<DbSchema>();

                foreach (var table in tables)
                {
                    // Fetch column schema for each table
                    var columns = await GetTableSchemaAsync(table.Schema!, table.Table!);

                    dbSchemas.Add(new DbSchema
                    {
                        TableName = table.Table,
                        Columns = columns.ToList()
                    });
                }

                return dbSchemas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving table and column information");
                throw;
            }
        }


    }

    // Model classes for API responses
    public class ColumnSchema
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }

    public class TableSchema
    {
        public string? Schema { get; set; }
        public string? Table { get; set; }
    }

    public class SchemaResponse
    {
        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
    }

    public class DbSchema
    {
        public string? TableName { get; set; }
        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
    }

}
