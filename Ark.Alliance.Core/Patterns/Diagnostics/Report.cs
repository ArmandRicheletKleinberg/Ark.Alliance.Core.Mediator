using System;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Diagnostics
{
    /// <summary>
    /// This contains the basic information about a report that can be displayed to the user.
    /// </summary>
    public class Report
    {
        #region Properties (Public)

        /// <summary>
        /// The unique key of the report.
        /// By default the method name.
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// The description of the report method.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// This is the function to get the report data.
        /// </summary>
        public virtual Func<Task<Result<object>>> GetFunction { get; internal set; }

        #endregion Properties (Public)
    }
}