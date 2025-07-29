// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace Ark.Alliance.Core
{
    /// <summary>
    /// A generic result of a method call.
    /// </summary>
    public class Result
    {
        #region Static Methods (Safe Execute)

        /// <summary>
        /// Safely executes an action that returns a Result.
        /// If any exception occurs during the action execution, an unexpected result is returned.
        /// </summary>
        /// <param name="actionToSafelyExecute">The action to safely execute.</param>
        /// <param name="actionToExecuteIfException">The optional action to execute if an exception occurs</param>
        /// <returns>The result returned by the action or Unexpected if an exception is thrown.</returns>
        public static Result SafeExecute(Func<Result> actionToSafelyExecute, Action<Exception> actionToExecuteIfException = null)
        {
            try
            {
                return actionToSafelyExecute();
            }
            catch (Exception exception)
            {
                actionToExecuteIfException?.Invoke(exception);
                return new Result(exception);
            }
        }

        /// <summary>
        /// Safely executes an action that returns a Result.
        /// If any exception occurs during the action execution, an unexpected result is returned.
        /// </summary>
        /// <param name="actionToSafelyExecute">The action to safely execute.</param>
        /// <param name="actionToExecuteIfException">The optional action to execute if an exception occurs</param>
        /// <returns>The result returned by the action or Unexpected if an exception is thrown.</returns>
        public static async Task<Result> SafeExecute(Func<Task<Result>> actionToSafelyExecute, Action<Exception> actionToExecuteIfException = null)
        {
            try
            {
                return await actionToSafelyExecute();
            }
            catch (Exception exception)
            {
                actionToExecuteIfException?.Invoke(exception);
                return new Result(exception);
            }
        }

        #endregion Static Methods (Safe Execute)

        #region Static (Shortcuts)

        /// <summary>
        /// Success.
        /// </summary>
        public static Result Success => new();

        /// <summary>
        /// Failure.
        /// </summary>
        public static Result Failure => new(ResultStatus.Failure);

        /// <summary>
        /// An unexpected failure has occured.
        /// </summary>
        public static Result Unexpected => new(ResultStatus.Unexpected);

        /// <summary>
        /// Unauthorized.
        /// </summary>
        public static Result Unauthorized => new(ResultStatus.Unauthorized);

        /// <summary>
        /// Already.
        /// </summary>
        public static Result Already => new(ResultStatus.Already);

        /// <summary>
        /// Not found.
        /// </summary>
        public static Result NotFound => new(ResultStatus.NotFound);

        /// <summary>
        /// Bad pre requisites.
        /// </summary>
        public static Result BadPrerequisites => new(ResultStatus.BadPrerequisites);

        /// <summary>
        /// Bad parameters are given.
        /// </summary>
        public static Result BadParameters => new(ResultStatus.BadParameters);

        /// <summary>
        /// The method call was canceled.
        /// </summary>
        public static Result Cancelled => new(ResultStatus.Cancelled);

        /// <summary>
        /// The method call has timed out.
        /// </summary>
        public static Result Timeout => new(ResultStatus.Timeout);

        /// <summary>
        /// The operation has no connection to execute.
        /// </summary>
        public static Result NoConnection => new(ResultStatus.NoConnection);

        /// <summary>
        /// The operation is not implemented
        /// </summary>
        public static Result NotImplemented => new(ResultStatus.NotImplemented);

        #endregion Static (Shortcuts)

        #region Constructors

        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="status">The result status of the method call.</param>
        public Result(ResultStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="exception">The exception if unexpected error.</param>
        public Result(Exception exception)
        {
            Status = ResultStatus.Unexpected;
            Exception = exception;
        }

        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="result">The another generic result to take the status/exception.</param>
        public Result(Result result)
        {
            Status = result.Status;
            Reason = result.Reason;
            Exception = result.Exception;
        }

        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="status">The result status of the method call.</param>
        /// <param name="exception">The exception if unexpected error.</param>
        public Result(ResultStatus status = ResultStatus.Success, Exception exception = null)
        {
            Status = status;
            Exception = exception;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The result status of the method call.
        /// </summary>
        public ResultStatus Status { get; protected set; }

        /// <summary>
        /// The reason in text why a not succeeded result has occured.
        /// </summary>
        public string Reason { get; protected set; }

        /// <summary>
        /// The exception if unexpected error.
        /// </summary>
        public Exception Exception { get; protected set; }

        #endregion Properties

        #region Properties (Computed)

        /// <summary>
        /// Whether the status is a success.
        /// </summary>
        public bool IsSuccess => Status == ResultStatus.Success;

        /// <summary>
        /// Whether the status is not a success.
        /// </summary>
        public bool IsNotSuccess => Status != ResultStatus.Success;

        /// <summary>
        /// Whether the status is a failure.
        /// </summary>
        public bool IsFailure => Status == ResultStatus.Failure;

        /// <summary>
        /// Whether the status is an unexpected error.
        /// </summary>
        public bool IsUnexpected => Status == ResultStatus.Unexpected;

        /// <summary>
        /// Whether the status is not an unexpected error.
        /// </summary>
        public bool IsNotUnexpected => Status != ResultStatus.Unexpected;

        /// <summary>
        /// Whether the status is an unauthorized call.
        /// </summary>
        public bool IsUnauthorized => Status == ResultStatus.Unauthorized;

        /// <summary>
        /// Whether the call was already done.
        /// </summary>
        public bool IsAlready => Status == ResultStatus.Already;

        /// <summary>
        /// Whether the call was not already done.
        /// </summary>
        public bool IsNotAlready => Status != ResultStatus.Already;

        /// <summary>
        /// Whether the call logic was not found.
        /// </summary>
        public bool IsNotFound => Status == ResultStatus.NotFound;

        /// <summary>
        /// Whether the call has bad prerequisites.
        /// </summary>
        public bool IsBadPrerequisites => Status == ResultStatus.BadPrerequisites;

        /// <summary>
        /// Whether the call has bad parameters.
        /// </summary>
        public bool IsBadParameters => Status == ResultStatus.BadParameters;

        /// <summary>
        /// Whether the status is a user cancellation.
        /// </summary>
        public bool IsCancelled => Status == ResultStatus.Cancelled;

        /// <summary>
        /// Whether the status is a time out.
        /// </summary>
        public bool IsTimeout => Status == ResultStatus.Timeout;

        /// <summary>
        /// Whether the operation has no connection to execute.
        /// </summary>
        public bool IsNoConnection => Status == ResultStatus.NoConnection;

        /// <summary>
        /// Whether the operation is not implemented.
        /// </summary>
        public bool IsNotImplemented => Status == ResultStatus.NotImplemented;

        #endregion Properties (Computed)

        #region Methods (Public Helpers)

        /// <summary>
        /// Sets the status of the result.
        /// </summary>
        /// <param name="status">The status to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public Result WithStatus(ResultStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Sets the reason of the result.
        /// </summary>
        /// <param name="reason">The reason to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public Result WithReason(string reason)
        {
            Reason = reason;
            return this;
        }

        /// <summary>
        /// Adds the reason to the potentially existed reason of this result.
        /// The new reason will be inserted in from the old reason.
        /// </summary>
        /// <param name="reason">The reason to add in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public Result AddReason(string reason)
        {
            Reason = $"{reason}{(Reason != null ? $"{Environment.NewLine}{Reason}" : "")}";
            return this;
        }

        /// <summary>
        /// Sets the exception of the result.
        /// </summary>
        /// <param name="exception">The exception to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public Result WithException(Exception exception)
        {
            Exception = exception;
            return this;
        }

        /// <summary>
        /// When the result is a success then continues with another action that returns a success in order to chain a bunch of actions.
        /// </summary>
        /// <param name="resultFunctionToContinueWith">A function that returns a result to continue with</param>
        /// <returns>This result if not success or the result of the function to continue with if success.</returns>
        public Result WhenSuccessContinueWith(Func<Result, Result> resultFunctionToContinueWith)
        {
            if (IsNotSuccess)
                return this;

            return resultFunctionToContinueWith(this);
        }

        /// <summary>
        /// When the result is a success then continues with another action that returns a success in order to chain a bunch of actions.
        /// </summary>
        /// <param name="resultFunctionToContinueWith">A function that returns a result to continue with</param>
        /// <returns>This result if not success or the result of the function to continue with if success.</returns>
        public async Task<Result> WhenSuccessContinueWith(Func<Result, Task<Result>> resultFunctionToContinueWith)
        {
            if (IsNotSuccess)
                return this;

            return await resultFunctionToContinueWith(this);
        }

        /// <summary>
        /// Converts a failed result to an exception to be thrown.
        /// </summary>
        /// <returns>The Exception with the result data.</returns>
        public virtual Exception ToException()
            => new ResultException($"{Status} - {Reason}", Exception);

        #endregion Methods (Public Helpers)

        #region Methods (Override)

        /// <summary>
        /// String representation of the result.
        /// </summary>
        /// <returns>The string representation of the result.</returns>
        public override string ToString()
        {
            return Status.ToString();
        }

        #endregion Methods (Override)
    }

    /// <inheritdoc />
    /// <summary>
    /// A generic result of a method call with data.
    /// </summary>
    public class Result<T> : Result
    {
        #region Static Methods (Safe Execute)

        /// <summary>
        /// Safely executes an action that returns a Result.
        /// If any exception occurs during the action execution, an unexpected result is returned.
        /// </summary>
        /// <param name="actionToSafelyExecute">The action to safely execute.</param>
        /// <returns>The result returned by the action or Unexpected if an exception is thrown.</returns>
        public static Result<T> SafeExecute(Func<Result<T>> actionToSafelyExecute)
        {
            try
            {
                return actionToSafelyExecute();
            }
            catch (Exception exception)
            {
                return new Result<T>(exception);
            }
        }

        /// <summary>
        /// Safely executes an action that returns a Result.
        /// If any exception occurs during the action execution, an unexpected result is returned.
        /// </summary>
        /// <param name="actionToSafelyExecute">The action to safely execute.</param>
        /// <returns>The result returned by the action or Unexpected if an exception is thrown.</returns>
        public static async Task<Result<T>> SafeExecute(Func<Task<Result<T>>> actionToSafelyExecute)
        {
            try
            {
                return await actionToSafelyExecute();
            }
            catch (Exception exception)
            {
                return new Result<T>(exception);
            }
        }

        #endregion Static Methods (Safe Execute)

        #region Static (Shortcuts)

        /// <summary>
        /// Success.
        /// </summary>
        public new static Result<T> Success => new();

        /// <summary>
        /// Failure.
        /// </summary>
        public new static Result<T> Failure => new(ResultStatus.Failure);

        /// <summary>
        /// An unexpected failure has occured.
        /// </summary>
        public new static Result<T> Unexpected => new(ResultStatus.Unexpected);

        /// <summary>
        /// Unauthorized.
        /// </summary>
        public new static Result<T> Unauthorized => new(ResultStatus.Unauthorized);

        /// <summary>
        /// Already.
        /// </summary>
        public new static Result<T> Already => new(ResultStatus.Already);

        /// <summary>
        /// Not found.
        /// </summary>
        public new static Result<T> NotFound => new(ResultStatus.NotFound);

        /// <summary>
        /// Bad pre requisites.
        /// </summary>
        public new static Result<T> BadPrerequisites => new(ResultStatus.BadPrerequisites);

        /// <summary>
        /// Bad parameters are given.
        /// </summary>
        public new static Result<T> BadParameters => new(ResultStatus.BadParameters);

        /// <summary>
        /// The method call was canceled.
        /// </summary>
        public new static Result<T> Cancelled => new(ResultStatus.Cancelled);

        /// <summary>
        /// The method call has timed out.
        /// </summary>
        public new static Result<T> Timeout => new(ResultStatus.Timeout);

        /// <summary>
        /// The operation has no connection to execute.
        /// </summary>
        public new static Result<T> NoConnection => new(ResultStatus.NoConnection);

        /// <summary>
        /// The operation is not implemented
        /// </summary>
        public new static Result<T> NotImplemented => new(ResultStatus.NotImplemented);

        #endregion Static (Shortcuts)

        #region Constructors

        /// <inheritdoc />
        /// <summary>
        /// Creates a Result instance.
        /// Simply checks if there is some data to flag as success or failure.
        /// </summary>
        /// <param name="data">The data returned from the method call if any.</param>
        public Result(T data)
            : base(ResultStatus.Success)
        {
            Data = data;
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="exception">The exception if unexpected error.</param>
        public Result(Exception exception)
            : base(exception)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="result">The another generic result to take the status/exception.</param>
        public Result(Result result)
            : base(result)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="status">The result status of the method call.</param>
        /// <param name="exception">The exception if unexpected error.</param>
        public Result(ResultStatus status, Exception exception)
            : base(status, exception)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Creates a <see cref="Result"/> instance.
        /// </summary>
        /// <param name="status">The result status of the method call.</param>
        /// <param name="data">The data returned from the method call if any.</param>
        /// <param name="exception">The exception if unexpected error.</param>
        public Result(ResultStatus status = ResultStatus.Success, T data = default, Exception exception = null)
            : base(status, exception)
        {
            Data = data;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The data returned from the method call if any.
        /// </summary>
        public T Data { get; protected set; }

        #endregion Properties

        #region Properties (Computed)

        /// <summary>
        /// Whether the result has some data.
        /// </summary>
        public bool HasData => Data != null;

        #endregion Properties (Computed)

        #region Methods (Public Helpers)

        /// <summary>
        /// Sets the status of the result.
        /// </summary>
        /// <param name="status">The status to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public new Result<T> WithStatus(ResultStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Sets the reason of the result.
        /// </summary>
        /// <param name="reason">The reason to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public new Result<T> WithReason(string reason)
        {
            Reason = reason;
            return this;
        }

        /// <summary>
        /// Adds the reason to the potentially existed reason of this result.
        /// The new reason will be inserted in from the old reason.
        /// </summary>
        /// <param name="reason">The reason to add in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public new Result<T> AddReason(string reason)
        {
            Reason = $"{reason}{(Reason != null ? $"{Environment.NewLine}{Reason}" : "")}";
            return this;
        }

        /// <summary>
        /// Sets the exception of the result.
        /// </summary>
        /// <param name="exception">The exception to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public new Result<T> WithException(Exception exception)
        {
            Exception = exception;
            return this;
        }

        /// <summary>
        /// Sets the data of the result.
        /// </summary>
        /// <param name="data">The data to set in the result.</param>
        /// <returns>The same result instance in order to chain the property set.</returns>
        public Result<T> WithData(T data)
        {
            Data = data;
            return this;
        }

        /// <summary>
        /// When the result is a success then continues with another action that returns a success in order to chain a bunch of actions.
        /// </summary>
        /// <param name="resultFunctionToContinueWith">A function that returns a result to continue with</param>
        /// <returns>This result if not success or the result of the function to continue with if success.</returns>
        public Result WhenSuccessContinueWith(Func<Result<T>, Result> resultFunctionToContinueWith)
        {
            if (IsNotSuccess)
                return this;

            return resultFunctionToContinueWith(this);
        }

        #endregion Methods (Public Helpers)

        #region Methods (Override)

        /// <summary>
        /// Converts a failed result to an exception to be thrown.
        /// </summary>
        /// <returns>The Exception with the result data.</returns>
        public override Exception ToException()
        {
            var ex = new ResultException($"{Status} - {Reason}", Exception);
            if (Data != null)
            {
                ex.Data["Data"] = Data;
            }
            return ex;
        }
        /// <inheritdoc />
        /// <summary>
        /// String representation of the result.
        /// </summary>
        /// <returns>The string representation of the result.</returns>
        public override string ToString()
        {
            return $"{Status} - {(Reason != null ? $" - {Reason}" : "")} - {(Data != null ? $" - {Data?.GetType().Name}:{Data}" : "")}";
        }

        #endregion Methods (Override)
    }
}