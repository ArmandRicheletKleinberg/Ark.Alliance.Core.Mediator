namespace Ark.Alliance.Core

{
    /// <summary>
    /// The status for an indicator value.
    /// </summary>
    public enum IndicatorStatusEnum
    {
        /// <summary>
        /// No status defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// The indicator value has passed validity check.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The indicator value is not valid but this should not impact so much the app.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// The indicator value is not valid and this impact the application execution.
        /// </summary>
        Error = 3
    }
}