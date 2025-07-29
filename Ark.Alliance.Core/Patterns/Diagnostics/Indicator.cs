using System.Runtime.CompilerServices;


namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This class is the base class for Indicator without generics.
    /// An indicator is a value of the app to monitor to check for overall app health.
    /// </summary>
    public abstract class Indicator
    {
        #region Properties (Public)

        /// <summary>
        /// The unique key of the indicator.
        /// By default the property name.
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// The descriptive label to display for this indicator.
        /// </summary>
        public string Label { get; internal set; }

        /// <summary>
        /// The current value of the indicator.
        /// </summary>
        public virtual object Value { get; internal set; }

        /// <summary>
        /// Whether the value has been set at least once
        /// </summary>
        public bool IsValueSet { get; internal set; }

        /// <summary>
        /// This is the function used to check whether the indicator value is 
        /// </summary>
        /// <returns></returns>
        public virtual Func<object, IndicatorStatusEnum> CheckFunction { get; internal set; }

        #endregion Properties (Public)

        #region Properties (Computed)

        /// <summary>
        /// Checks the indicator status.
        /// </summary>
        public IndicatorStatusEnum Status
            => CheckFunction(Value);

        #endregion Properties (Computed)
    }

    /// <inheritdoc />
    /// <summary>
    /// An indicator is a value of the app to monitor to check for overall app health.
    /// This indicator class owns a strongly typed value/check.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Indicator<TValue> : Indicator
    {
        #region Constructors

        /// <summary>
        /// Creates an indicator to monitor the health of the app.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the Indicator.</typeparam>
        /// <param name="checkFunction">The function that checks the value to give a status.</param>
        /// <param name="initialValue">The initial value of the indicator.</param>
        /// <param name="label">The descriptive label to display for this indicator. If none, this will be the key.</param>
        /// <param name="key">The key of the indicator. Must be unique. By default the property name.</param>
        /// <returns></returns>
        public Indicator(Func<TValue, IndicatorStatusEnum> checkFunction = null, TValue initialValue = default, string label = null, [CallerMemberName] string key = null)
        {
            Key = key;
            Label = label ?? key;
            Value = initialValue;
            base.CheckFunction = checkFunction != null
                ? new Func<object, IndicatorStatusEnum>(value => checkFunction((TValue)value))
                : value => IndicatorStatusEnum.Success;
        }

        #endregion Constructors

        #region Properties (Public)

        /// <summary>
        /// The current value of the indicator.
        /// </summary>
        public new TValue Value
        {
            get => (TValue)base.Value;
            internal set => base.Value = value;
        }

        #endregion Properties (Public)

        #region Methods (Public)

        /// <summary>
        /// Sets a new value for the indicator.
        /// It can change the indicator status.
        /// </summary>
        /// <param name="value">The value </param>
        public void SetValue(TValue value)
        {
            Value = value;
            IsValueSet = true;
        }

        #endregion Methods (Public)
    }
}