using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PostgreSqlAPI.Models;
using PostgreSqlAPI.Services;

namespace PostgreSqlAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class McpController : ControllerBase
    {
        private readonly IDatabaseService _dbService;

        public McpController(IDatabaseService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        [HttpPost("query")]
        public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Sql))
                return BadRequest("SQL query cannot be empty.");

            try
            {
                var results = await _dbService.ExecuteQueryAsync(request.Sql);
                return Ok(results);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest($"Database error: {ex.Message}");
            }
        }

        [HttpGet("schema/{schema}/{table}")]
        public async Task<IActionResult> GetSchema(string schema, string table)
        {
            if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(table))
                return BadRequest("Schema and table names are required.");

            try
            {
                var columns = await _dbService.GetTableSchemaAsync(schema, table);
                if (columns.Count == 0)
                    return NotFound($"Table '{schema}.{table}' not found.");
                return Ok(new { columns });
            }
            catch (NpgsqlException ex)
            {
                return BadRequest($"Database error: {ex.Message}");
            }
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var tables = await _dbService.GetTablesAsync();
                return Ok(tables);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest($"Database error: {ex.Message}");
            }
        }

        [HttpGet("data/{schema}/{table}")]
        public async Task<IActionResult> GetData(string schema, string table)
        {
            if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(table))
                return BadRequest("Schema and table names are required.");
            try
            {
                var data = await _dbService.GetDataFromTableAsync(schema, table);
                return Ok(data);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest($"Database error: {ex.Message}");
            }
        }

        [HttpGet("dbschema")]
        public async Task<IActionResult> GetDbSchema()
        {
            try
            {
                var dbSchemas = await _dbService.GetTableAndValueInforDB();
                return Ok(dbSchemas);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest($"Database error: {ex.Message}");
            }
        }

        [HttpPost("set-connection-string")]
        public IActionResult SetConnectionString([FromBody] DatabaseConnectionModel connectionModel)
        {
            var connectionString = $"Host={connectionModel.Host};Port={connectionModel.Port};Database={connectionModel.Database};Username={connectionModel.Username};Password={connectionModel.Password};";
            DatabaseService.SetConnectionString(connectionString);
            return Ok("Connection string updated successfully.");
        }
    }
}
