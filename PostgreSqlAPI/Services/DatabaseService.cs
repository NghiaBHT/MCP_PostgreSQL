using Npgsql;
using PostgreSqlAPI.Models;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace PostgreSqlAPI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ??
                throw new ArgumentNullException(nameof(configuration), "PostgreSQL connection string is missing.");
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                result.Add(row);
            }
            return result;
        }

        public async Task<List<ColumnSchema>> GetTableSchemaAsync(string schema, string table)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_schema = @schema AND table_name = @table
            ORDER BY ordinal_position  ";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("schema", schema);
            cmd.Parameters.AddWithValue("table", table);
            using var reader = await cmd.ExecuteReaderAsync();
            var columns = new List<ColumnSchema>();
            while (await reader.ReadAsync())
            {
                columns.Add(new ColumnSchema
                {
                    Name = reader["column_name"].ToString(),
                    Type = reader["data_type"].ToString()
                });
            }
            return columns;
        }

        public async Task<List<TableSchema>> GetTablesAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
            SELECT table_schema, table_name
            FROM information_schema.tables
            WHERE table_type = 'BASE TABLE' AND table_schema = 'public'
        ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            var tables = new List<TableSchema>();
            while (await reader.ReadAsync())
            {
                tables.Add(new TableSchema
                {
                    Schema = reader["table_schema"].ToString(),
                    Table = reader["table_name"].ToString()
                });
            }
            return tables;
        }
    }
}
