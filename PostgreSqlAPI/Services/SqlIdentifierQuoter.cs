using System.Text.RegularExpressions;
using Npgsql;

namespace PostgreSqlAPI.Services
{
    public class SqlIdentifierQuoter
    {
        // Regex đơn giản cho SELECT, FROM, JOIN, WHERE
        private static readonly Regex FromJoinRegex = new(@"(?<=\bFROM\s|\bJOIN\s)([A-Za-z_][A-Za-z0-9_]*)(?:\s+(?:AS\s+)?([A-Za-z_][A-Za-z0-9_]*))?", RegexOptions.IgnoreCase);
        private static readonly Regex SelectColumnRegex = new(@"\bSELECT\s+(.*?)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ColumnRefRegex = new(@"([A-Za-z_][A-Za-z0-9_]*)\.([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase);

        private readonly NpgsqlCommandBuilder _commandBuilder = new();

        public string QuoteSqlIdentifiers(string rawSql)
        {
            var sql = rawSql;

            // Step 1: Quote table names in FROM and JOIN
            sql = FromJoinRegex.Replace(sql, match =>
            {
                var tableName = match.Groups[1].Value;
                var alias = match.Groups[2].Success ? match.Groups[2].Value : null;

                var quotedTable = $"public.{_commandBuilder.QuoteIdentifier(tableName)}";
                return alias != null
                    ? $"{quotedTable} {alias}"
                    : $"{quotedTable}";
            });

            // Step 2: Quote SELECT columns (skip *, functions, and already quoted)
            sql = SelectColumnRegex.Replace(sql, match =>
            {
                var columnsPart = match.Groups[1].Value;
                var columns = columnsPart.Split(',');

                var quotedColumns = columns.Select(c =>
                {
                    var col = c.Trim();

                    if (col == "*" || col.Contains("(") || col.Contains("\""))
                        return col;

                    if (col.Contains("."))
                    {
                        var parts = col.Split('.');
                        return $"{parts[0]}.{_commandBuilder.QuoteIdentifier(parts[1])}";
                    }

                    return _commandBuilder.QuoteIdentifier(col);
                });

                return $"SELECT {string.Join(", ", quotedColumns)} FROM";
            });

            // Step 3: Quote alias.column format in WHERE, ON, GROUP BY, etc.
            sql = ColumnRefRegex.Replace(sql, match =>
            {
                var alias = match.Groups[1].Value;
                var col = match.Groups[2].Value;

                // Bỏ qua nếu đã quoted
                if (col.StartsWith("\"") && col.EndsWith("\""))
                    return match.Value;

                return $"{alias}.{_commandBuilder.QuoteIdentifier(col)}";
            });

            return sql;
        }
    }

}
