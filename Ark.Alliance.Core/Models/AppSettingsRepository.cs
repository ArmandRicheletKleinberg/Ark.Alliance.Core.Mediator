using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Specialized;

namespace Ark.Alliance.Core

{
    /// <summary>
    /// This repository is used to get some app settings from the app configuration settings files.
    /// </summary>
    public class AppSettingsRepository
    {
        #region Static

        /// <summary>
        /// The sections are kept in static memory for performance purpose.
        /// </summary>
        private static readonly Dictionary<Type, AppSettingsSectionBase> Sections = new Dictionary<Type, AppSettingsSectionBase>();

        /// <summary>
        /// The app configuration set by the service collection extension.
        /// </summary>
        public static IConfiguration Configuration { get; set; }

        #endregion Static

        #region Methods (Public)

        /// <summary>
        /// Gets an app settings section filled with configuration data.
        /// It creates a new instance every call to avoid global change if properties changed in caller method.
        /// </summary>
        public virtual TSection GetSection<TSection>()
            where TSection : AppSettingsSectionBase
        {
            // First search in the section cache for performance purpose
            var sectionType = typeof(TSection);
            if (Sections.GetValue(sectionType) is TSection section)
                return section;

            if (Configuration == null)
                throw new Exception("This repository needs to be initialized using Startup.Services.AddAppSettings(IHostingEnvironment hostingEnvironment)");

            section = typeof(TSection).New<TSection>();
            if (section.SectionPath.IsNullOrEmpty())
                throw new Exception($"The section path must be defined for all sections. { sectionType }");
            section = section.Deserialize(Configuration) as TSection;
            if (section == null)
                throw new Exception($"The section could not be deserialized correctly, check your deserialization code. { sectionType }");

            Sections.Add(sectionType, section);
            return section;
        }

        /// <summary>
        /// Gets a ExpandoObject object with the full application configuration for this environment.
        /// It includes app settings, environment variables.
        /// </summary>
        /// <returns>The ExpandoObject object of the whole application configuration.</returns>
        public virtual ExpandoObject GetFullConfigurationJsonObject()
        {
            var obj = new ExpandoObject();
            AddConfigurationSection(Configuration, obj);
            return obj;
        }

        #endregion Methods (Public)

        #region Methods (Helpers)

        /// <summary>
        /// Adds a configuration section to a ExpandoObject.
        /// </summary>
        /// <param name="configuration">The configuration section or root used to get the whole app configuration.</param>
        /// <param name="parent">The parent to add property or object children to.</param>
        private static void AddConfigurationSection(IConfiguration configuration, ExpandoObject parent)
        {
            configuration?.GetChildren().ForEach(section =>
            {
                if (section.Value != null)
                {
                    parent.AddOrUpdate(section.Key, section.Value);
                    return;
                }

                var child = new ExpandoObject();
                parent.AddOrUpdate(section.Key, child);
                AddConfigurationSection(section, child);
            });
        }

        #endregion Methods (Helpers)
    }
}