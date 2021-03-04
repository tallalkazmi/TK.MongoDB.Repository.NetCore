namespace TK.MongoDB
{
    /// <summary>
    /// Represents an open connection to a data source.
    /// </summary>
    public class Connection
    {
        readonly string ConnectionString;

        /// <summary>
        /// Represents an open connection to a data source. Instantiate connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets data source's connection string.
        /// </summary>
        /// <returns>Connection string</returns>
        public string GetConnectionString()
        {
            return ConnectionString;
        }
    }
}
