using System;
using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core
{
    public class LogDto
    {
        #region Properties (Public)

        /// <summary>
        /// The severity level of the log.
        /// </summary>
        public LogSeverityEnum Severity { get; set; }

        /// <summary>
        /// The time when the log was created.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// The category of the log.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The inner details of the logs.
        /// </summary>
        public string Details { get; set; }

        #endregion Properties (Public)
    }
}