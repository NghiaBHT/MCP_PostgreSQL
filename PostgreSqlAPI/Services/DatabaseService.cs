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
            // Preprocess SQL to handle unquoted table names
            sql = PreprocessSqlQuery(sql);

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

        private string PreprocessSqlQuery(string sql)
        {
            // Step 1: Process table names in FROM and JOIN clauses and capture aliases
            // Match pattern: FROM/JOIN TableName [AS] alias
            var tableAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var tableRegex = new Regex(@"(FROM|JOIN)\s+(?!public\.)(?!""[^""]+"")\b([A-Za-z_][A-Za-z0-9_]*)\b(?:\s+(?:AS\s+)?([A-Za-z_][A-Za-z0-9_]*))?", RegexOptions.IgnoreCase);
            
            sql = tableRegex.Replace(sql, match =>
            {
                var clause = match.Groups[1].Value; // FROM or JOIN
                var tableName = match.Groups[2].Value; // The table name
                
                // If there's an alias, capture it
                if (match.Groups[3].Success)
                {
                    var alias = match.Groups[3].Value;
                    tableAliases[alias] = alias; // Store the alias to prevent quoting it later
                    return $"{clause} public.\"{tableName}\" {alias}";
                }
                
                return $"{clause} public.\"{tableName}\"";
            });

            // Step 2: Handle column references in the format alias.column
            // but avoid double-quoting columns that are already quoted
            var columnRefRegex = new Regex(@"([A-Za-z_][A-Za-z0-9_]*)\.((?!""[^""]+"")\b[A-Za-z_][A-Za-z0-9_]*\b)", RegexOptions.IgnoreCase);
            sql = columnRefRegex.Replace(sql, match =>
            {
                var alias = match.Groups[1].Value;
                var column = match.Groups[2].Value;
                
                // Don't quote the alias if it's in our table aliases dictionary
                var quotedAlias = tableAliases.ContainsKey(alias) ? alias : $"\"{alias}\"";
                return $"{quotedAlias}.\"{column}\"";
            });

            // Step 3: Handle bare column names in WHERE clauses and other conditions
            // but skip column names that are already part of an alias.column reference
            var columnRegex = new Regex(@"(WHERE|AND|OR|ON|GROUP\s+BY|ORDER\s+BY|HAVING)\s+(?!""[^""]+""|\w+\.[""a-zA-Z0-9_]+)\b([A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.IgnoreCase);
            sql = columnRegex.Replace(sql, match =>
            {
                var clause = match.Groups[1].Value; // WHERE, AND, OR, etc.
                var columnName = match.Groups[2].Value; // The column name
                
                // Skip quoting if it's in our table aliases
                if (tableAliases.ContainsKey(columnName))
                {
                    return $"{clause} {columnName}";
                }
                
                return $"{clause} \"{columnName}\"";
            });

            // Step 4: Handle comparison operations, but avoid processing aliases
            var comparisonRegex = new Regex(@"(?<![""\w]|\.\s*)\b([A-Za-z_][A-Za-z0-9_]*)\b\s*(=|<>|<|>|<=|>=|LIKE|NOT\s+LIKE|IN|NOT\s+IN|IS|IS\s+NOT)", RegexOptions.IgnoreCase);
            sql = comparisonRegex.Replace(sql, match =>
            {
                var columnName = match.Groups[1].Value;
                var operatorStr = match.Groups[2].Value;
                
                // Skip quoting if it's in our table aliases
                if (tableAliases.ContainsKey(columnName))
                {
                    return $"{columnName} {operatorStr}";
                }
                
                return $"\"{columnName}\" {operatorStr}";
            });

            return sql;
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
