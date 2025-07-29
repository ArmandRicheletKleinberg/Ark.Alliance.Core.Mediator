namespace Ark.Alliance.Core
{
    /// <summary>
    /// The information about the indicator.
    /// </summary>
    public class IndicatorDto
    {
        #region Properties (Public)

        /// <summary>
        /// The unique key of the indicator.
        /// By default the property name.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The descriptive label to display for this indicator.
        /// </summary>
        public string Label { get; set; }


        /// <summary>
        /// The name of the group of the indicator.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The current value of the indicator.
        /// </summary>
        public object Value { get; set; }


        /// <summary>
        /// Checks the indicator status.
        /// </summary>
        public IndicatorStatusEnum Status { get; set; }

        #endregion Properties (Public)
    }
}