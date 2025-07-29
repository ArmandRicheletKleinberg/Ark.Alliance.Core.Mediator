using System.Collections;

namespace Ark.Alliance.Core
{
    /// <summary>
    /// This DTO is used to transfer Exceptions from server to client.
    /// </summary>
    public class ExceptionDto
    {
        #region Properties (Public)

        /// <summary>
        /// Gets or sets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        public IDictionary Data { get; set; }

        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        public ExceptionDto InnerException { get; set; }

        /// <summary>
        /// Gets or sets a message that describes the current exception.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public string StackTrace { get; set; }

        #endregion Properties (Public)
    }
}