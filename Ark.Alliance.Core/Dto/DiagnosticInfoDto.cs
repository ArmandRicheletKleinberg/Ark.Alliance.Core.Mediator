using System.Collections.Generic;

namespace Ark.Alliance.Core
{
    /// <summary>
    /// The information about the application diagnostic.
    /// </summary>
    public class DiagnosticInfoDto
    {
        #region Properties (Public)

        /// <summary>
        /// The different logs database tables (prefixed by _logs) with categories.
        /// </summary>
        public LogTableDto[] Tables { get; set; }

        /// <summary>
        /// The reports available in the application diagnostics.
        /// </summary>
        public ReportDto[] Reports { get; set; }

        #endregion Properties (Public)
    }
}