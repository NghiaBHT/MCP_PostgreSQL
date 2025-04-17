namespace PostgreSqlAPI.Models
{
    /// <summary>
    /// Represents the schema of a column in a database table.
    /// </summary>
    public class ColumnSchema
    {
        /// <summary>
        /// The name of the column in the database table.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The data type of the column in the database table.
        /// </summary>
        public string? Type { get; set; }
    }
}
