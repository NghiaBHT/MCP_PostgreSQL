using PostgreSqlAPI.Models;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace PostgreSqlAPI.Services
{
    public interface IDatabaseService
    {
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql);
        Task<List<ColumnSchema>> GetTableSchemaAsync(string schema, string table);
        Task<List<TableSchema>> GetTablesAsync();
        Task<List<Dictionary<string, object>>> GetDataFromTableAsync(string schema, string table);
        Task<List<DbSchema>> GetTableAndValueInforDB();
    }
}
