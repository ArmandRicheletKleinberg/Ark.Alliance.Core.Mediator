using Microsoft.Extensions.Configuration;

namespace Ark.Alliance.Core
{
    
    /// <summary>
    /// This is the base class for all app settings sections class.
    /// It adds some useful helpers for the app settings like cloning.
    /// </summary>
    public abstract class AppSettingsSectionBase
    {
        #region Methods (Abstract)

        /// <summary>
        /// The section path must be defined in sub classes.
        /// </summary>
        public abstract string SectionPath { get; }

        #endregion Methods (Abstract)

        #region Methods (Public)

        /// <summary>
        /// Creates a shallow copy of the current System.Object.
        /// </summary>
        /// <returns>A shallow copy of the current System.Object.</returns>
        public new object MemberwiseClone()
            => base.MemberwiseClone();

        #endregion Methods (Public)

        #region Methods (Virtual)

        /// <summary>
        /// Deserializes the section configuration and returns an instance of this object.
        /// By default, simply deserializes the section as a JSON object.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The deserialized settings section object.</returns>
        public virtual object Deserialize(IConfiguration configuration)
            => configuration.GetSection(SectionPath).Get(GetType());

        #endregion Methods (Virtual)
    }
}