namespace BitwiseSharp.Types
{
    /// <summary>
    /// Represents the result of an operation, encapsulating either a success value or an error message.
    /// </summary>
    /// <typeparam name="T">The type of the value that is returned when the operation is successful.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the value of the operation result if it was successful.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the error message if the operation was not successful.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Initializes a successful <see cref="Result{T}"/> with the specified value.
        /// </summary>
        /// <param name="value">The value of the successful operation.</param>
        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Error = null;
        }

        /// <summary>
        /// Initializes a failed <see cref="Result{T}"/> with the specified error message.
        /// </summary>
        /// <param name="error">The error message describing the failure.</param>
        private Result(string error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        /// <summary>
        /// Creates a successful <see cref="Result{T}"/> with the specified value.
        /// </summary>
        /// <param name="value">The value of the successful operation.</param>
        /// <returns>A successful <see cref="Result{T}"/> containing the specified value.</returns>
        public static Result<T> Success(T value) => new Result<T>(value);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with the specified error message.
        /// </summary>
        /// <param name="error">The error message describing the failure.</param>
        /// <returns>A failed <see cref="Result{T}"/> containing the specified error message.</returns>
        public static Result<T> Failure(string error) => new Result<T>(error);
    }
}
