using System.Collections.Generic;
using System.Linq;

namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This class is the base class used to define the indicators used in diagnostics.
    /// The loggers will be created during the Diagnostics services setup which used the LoggerFactory to create the loggers by reflection.
    /// </summary>
    /// <example>
    /// public Indicator<int> ErrorCount { get; } = new Indicator<int>(value => value > 5 ? IndicatorStatsEnum.Error : IndicatorStatsEnum.Success);
    /// </example>
    public abstract class IndicatorsBase
    {
        #region Methods (Internal)

        /// <summary>
        /// Initializes the indicators (mocked or not).
        /// </summary>
        /// <param name="mock">Whether the indicators call should me mocked.</param>
        internal Dictionary<string, Indicator> Init(bool mock)
            => Indicators = GetType().GetProperties()
                    .Where(p => typeof(Indicator).IsAssignableFrom(p.PropertyType))
                    .Select(p => (Indicator)p.GetValue(this))
                    .ToDictionary(i => i.Key);

        #endregion Methods (Internal)

        #region Properties (Public)

        /// <summary>
        /// This is the list of all the indicators used in the application.
        /// </summary>
        public Dictionary<string, Indicator> Indicators { get; private set; }

        #endregion Properties (Public)
    }
}