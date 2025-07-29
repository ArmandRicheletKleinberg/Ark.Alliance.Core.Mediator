using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This class is the base class used to define the loggers used.
    /// The loggers will be created during the Diagnostics services setup which used the LoggerFactory to create the loggers by reflection.
    /// </summary>
    /// <example>
    /// public ILogger General { get; set; }
    /// </example>
    public abstract class LoggersBase
    {
        #region Methods (Internal)

        /// <summary>
        /// Initializes the loggers (mocked or not).
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create the loggers.</param>
        internal Dictionary<string, ILogger> Init(ILoggerFactory loggerFactory = null)
        {
            loggerFactory = loggerFactory ?? new NullLoggerFactory();
            return GetType().GetProperties()
                .Where(p => typeof(ILogger).IsAssignableFrom(p.PropertyType))
                .ToDictionary(p => p.Name, p =>
                    {
                        var logger = loggerFactory.CreateLogger(p.Name);
                        p.SetValue(this, logger);
                        return logger;
                    });
        }

        #endregion Methods (Internal)

        #region Properties (Public)

        /// <summary>
        /// This logger is used to log all the general events.
        /// </summary>
        public ILogger General { get; set; }

        /// <summary>
        /// This logger is used to log all the Controllers events.
        /// </summary>
        public ILogger Controllers { get; set; }

        #endregion Properties (Public)
    }
}