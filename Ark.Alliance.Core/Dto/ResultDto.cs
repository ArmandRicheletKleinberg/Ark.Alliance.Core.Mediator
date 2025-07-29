namespace Ark.Alliance.Core
{
    /// <summary>
    /// This DTO is a wrapper around a normal Result to send it back as a service model.
    /// </summary>
    public class ResultDto
    {
        /// <summary>
        /// The result status of the method call.
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        /// The reason in text why a not succeeded result has occured.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The exception text if unexpected error.
        /// </summary>
        public ExceptionDto Exception { get; set; }
    }

    /// <inheritdoc />
    /// <summary>
    /// This DTO is a wrapper around a normal Result to send it back as a service model.
    /// </summary>
    /// <typeparam name="TReturn">The type of data to return.</typeparam>
    public class ResultDto<TReturn> : ResultDto
    {
        /// <summary>
        /// The array of data returned from the method call if any.
        /// </summary>
        public TReturn Data { get; set; }
    }
}