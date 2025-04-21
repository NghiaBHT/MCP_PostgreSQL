using Npgsql;
using PostgreSqlAPI.Models;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;
using System.Text.RegularExpressions;

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
            var quoter = new SqlIdentifierQuoter();
            // Preprocess SQL to handle unquoted table names
            sql = quoter.QuoteSqlIdentifiers(sql);

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

        public async Task<List<DbSchema>> GetTableAndValueInforDB()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var tables = await GetTablesAsync();
            var dbSchemas = new List<DbSchema>();

            foreach (var table in tables)
            {
                // Lấy danh sách cột cho từng bảng
                var columns = await GetTableSchemaAsync(table.Schema!, table.Table!);

                // Tạo đối tượng DbSchema và thêm vào danh sách
                dbSchemas.Add(new DbSchema
                {
                    TableName = table.Table,
                    Columns = columns
                });
            }

            return dbSchemas;

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

        public async Task<List<Dictionary<string, object>>> GetDataFromTableAsync(string schema, string table)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Corrected SQL query string to avoid syntax errors
            var sql = $"SELECT * FROM {schema}.\"{table}\";";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

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

    }
}
