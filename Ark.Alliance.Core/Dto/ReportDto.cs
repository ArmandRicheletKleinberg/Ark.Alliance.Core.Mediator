namespace Ark.Alliance.Core
{
    /// <summary>
    /// The information about the report.
    /// </summary>
    public class ReportDto
    {
        #region Properties (Public)

        /// <summary>
        /// The unique key of the report.
        /// By default the method name.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The description of the method.
        /// </summary>
        public string Description { get; set; }
        
        #endregion Properties (Public)
    }
}