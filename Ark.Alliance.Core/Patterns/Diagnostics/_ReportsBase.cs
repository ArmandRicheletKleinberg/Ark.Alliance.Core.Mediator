using System.ComponentModel;
using System.Dynamic;
using System.Reflection;


namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This class is the base class used to access to the diagnostics reports.
    /// The reports can return some data useful to diagnose problems.
    /// A report is simply a method that returns a Task{Result{}} with no parameters.
    /// The report name will be simply the method name.
    /// </summary>
    /// <example>
    /// public async Task{Result{string[]}} ConnectedUsers() => Task.Run(() => new [] { "John Smith" });
    /// </example>
    public abstract class ReportsBase
    {
        #region Methods (Internal)

        /// <summary>
        /// Initializes the indicators (mocked or not).
        /// </summary>
        /// <param name="mock">Whether the indicators call should me mocked.</param>
        internal Dictionary<string, Report> Init(bool mock)
            => GetType().GetMethods()
                .Where(m => typeof(Task).IsAssignableFrom(m.ReturnType))
                .Where(m => m.ReturnType.GenericTypeArguments?.Length > 0)
                .Where(m => typeof(Result).IsAssignableFrom(m.ReturnType.GenericTypeArguments[0]))
                .Where(m => m.ReturnType.GenericTypeArguments[0].GenericTypeArguments?.Length == 1)
                .ToDictionary(m => m.Name, m => new Report
                {
                    Key = m.Name,
                    Description = m.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    GetFunction = (async () =>
                    {
                        var task = (Task)m.Invoke(this, new object[] { });
                        await task.ConfigureAwait(false);
                        var result = (Result)((dynamic)task).Result;
                        var data = ((dynamic)result).Data;
                        return new Result<object>(result).WithData(data);
                    })
                });

        #endregion Methods (Internal)

        #region Methods (Public)

        /// <summary>
        /// Gets the whole application configuration included the settings and environment variables.
        /// </summary>
        /// <returns>The JSON string of the whole application configuration.</returns>
        [Description("Gets the JSON string of the whole application configuration.")]
        public Task<Result<ExpandoObject>> ApplicationConfiguration()
            => Task.Run(() => new Result<ExpandoObject>(new AppSettingsRepository().GetFullConfigurationJsonObject()));

        #endregion Methods (Public)
    }
}