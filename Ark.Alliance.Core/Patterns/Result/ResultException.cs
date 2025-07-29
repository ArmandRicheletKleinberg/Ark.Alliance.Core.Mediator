namespace Ark.Alliance.Core
{
    /// <inheritdoc />
    /// <summary>
    /// This Exception class is created from a result.
    /// </summary>
    public class ResultException : Exception
    {
        #region Constructors

        /// <summary>
        /// Creates a <see cref="ResultException"/> instance.
        /// </summary>
        /// <param name="message">The message of the Exception if any.</param>
        /// <param name="innerException">The inner Exception if any.</param>
        public ResultException(string message = null, Exception innerException = null)
            : base(message, innerException)
        { }

        #endregion Constructors
    }
}