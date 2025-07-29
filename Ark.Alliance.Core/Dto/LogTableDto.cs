namespace Ark.Alliance.Core
{
    /// <summary>
    /// The information on a _logs table in the database.
    /// </summary>
    public class LogTableDto
    {
        #region Properties (Public)

        /// <summary>
        /// The name of the log table (always prefixed by _logs).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The categories of the logs in this table.
        /// </summary>
        public string[] Categories { get; set; }

        #endregion Properties (Public)
    }
}