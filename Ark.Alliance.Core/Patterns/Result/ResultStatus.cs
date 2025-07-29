// ReSharper disable UnusedMember.Global
namespace Ark.Alliance.Core
{
    /// <summary>
    /// This class is used as a generic result status of a method.
    /// It enumerates a lot of common scenarios.
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Success.
        /// </summary>
        Success,

        /// <summary>
        /// Failure.
        /// </summary>
        Failure,

        /// <summary>
        /// An unexpected failure has occured.
        /// </summary>
        Unexpected,

        /// <summary>
        /// Unauthorized.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Already.
        /// </summary>
        Already,

        /// <summary>
        /// Not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// Bad pre requisites.
        /// </summary>
        BadPrerequisites,

        /// <summary>
        /// Bad parameters are given.
        /// </summary>
        BadParameters,

        /// <summary>
        /// The method call was canceled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The method call has timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// The operation has no connection to execute.
        /// </summary>
        NoConnection,

        /// <summary>
        /// The operation is not implemented.
        /// </summary>
        NotImplemented
    }
}
