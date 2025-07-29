using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This is the base class for the Diagnostics features of an app.
    /// It must be overriden by giving the type of the features group.
    /// The features group are :
    /// - The Logs used to log in the app (logging configuration is set in the app.settings). See .NET Core Logging for more details;
    /// - The Indicators which are data that gives an overall health status of the application.
    /// - The Metrics used to measure values in a recurrent way;
    /// - The Reports used to return raw diagnostics data reports to the application.
    /// </summary>
    public abstract class DiagBase
    {
        #region Properties (Public)

        /// <summary>
        /// This is the list of all the loggers used in the application.
        /// </summary>
        public static Dictionary<string, ILogger> Logs { get; internal set; }

        /// <summary>
        /// This is the list of all the indicators used in the application.
        /// </summary>
        public static Dictionary<string, Indicator> Indicators { get; internal set; }

        /// <summary>
        /// This is the list of all the reports used to diagnose the app.
        /// </summary>
        public static Dictionary<string, Report> Reports { get; internal set; }

        #endregion Properties (Public)
    }

    /// <inheritdoc />
    /// <typeparam name="TLoggers">The type of the class that owns the loggers.</typeparam>
    public abstract class DiagBase<TLoggers> : DiagBase
        where TLoggers : LoggersBase, new()
    {
        #region Methods (Static)

        /// <summary>
        /// Initializes the diagnostics by setting up the loggers given the logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create the loggers.</param>
        public static void Init(ILoggerFactory loggerFactory = null)
        {
            Logs = new TLoggers();
            DiagBase.Logs = Logs.Init(loggerFactory);
        }

        /// <summary>
        /// This is to be set in the unit test projects to mock all the diagnostics features and do nothing instead.
        /// </summary>
        public static void Mock()
        {
            Logs = new TLoggers();
            DiagBase.Logs = Logs.Init();
        }

        #endregion Methods (Static)

        #region Properties (Static)

        /// <summary>
        /// The loggers that can be used in the app.
        /// </summary>
        public new static TLoggers Logs { get; set; }

        #endregion Properties (Static)
    }

    /// <inheritdoc />
    /// <typeparam name="TLoggers">The type of the class that owns the loggers.</typeparam>
    /// <typeparam name="TIndicators">The type of the class that owns the indicators.</typeparam>
    public abstract class DiagBase<TLoggers, TIndicators> : DiagBase<TLoggers>
        where TLoggers : LoggersBase, new()
        where TIndicators : IndicatorsBase, new()
    {
        #region Methods (Static)

        /// <summary>
        /// Initializes the diagnostics by setting up the loggers given the logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create the loggers.</param>
        public new static void Init(ILoggerFactory loggerFactory = null)
        {
            DiagBase<TLoggers>.Init(loggerFactory);
            Indicators = new TIndicators();
            DiagBase.Indicators = Indicators.Init(true);
        }

        /// <summary>
        /// This is to be set in the unit test projects to mock all the diagnostics features and do nothing instead.
        /// </summary>
        public new static void Mock()
        {
            DiagBase<TLoggers>.Mock();
            Indicators = new TIndicators();
            DiagBase.Indicators = Indicators.Init(true);
        }

        #endregion Methods (Static)

        #region Properties (Static)

        /// <summary>
        /// The loggers that can be used in the app.
        /// </summary>
        public new static TIndicators Indicators { get; set; }

        #endregion Properties (Static)
    }

    /// <inheritdoc />
    /// <typeparam name="TLoggers">The type of the class that owns the loggers.</typeparam>
    /// <typeparam name="TIndicators">The type of the class that owns the indicators.</typeparam>
    /// <typeparam name="TReports">The type of the class that owns the reports.</typeparam>
    public abstract class DiagBase<TLoggers, TIndicators, TReports> : DiagBase<TLoggers, TIndicators>
        where TLoggers : LoggersBase, new()
        where TIndicators : IndicatorsBase, new()
        where TReports : ReportsBase, new()
    {
        #region Methods (Static)

        /// <summary>
        /// Initializes the diagnostics by setting up the loggers given the logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create the loggers.</param>
        public new static void Init(ILoggerFactory loggerFactory = null)
        {
            DiagBase<TLoggers, TIndicators>.Init(loggerFactory);
            Reports = new TReports();
            DiagBase.Reports = Reports.Init(true);
        }

        /// <summary>
        /// This is to be set in the unit test projects to mock all the diagnostics features and do nothing instead.
        /// </summary>
        public new static void Mock()
        {
            DiagBase<TLoggers, TIndicators>.Mock();
            Reports = new TReports();
            DiagBase.Reports = Reports.Init(true);
        }

        #endregion Methods (Static)

        #region Properties (Static)

        /// <summary>
        /// The loggers that can be used in the app.
        /// </summary>
        public new static TReports Reports { get; set; }

        #endregion Properties (Static)
    }
}