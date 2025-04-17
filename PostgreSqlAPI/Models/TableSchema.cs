namespace PostgreSqlAPI.Models
{
    public class TableSchema
    {
        /// <summary>
        /// The name of the schema in the database.
        /// </summary>
        public string? Schema { get; set; }
        /// <summary>
        /// The name of the table in the database.
        /// </summary>
        public string? Table { get; set; }
    }
}
