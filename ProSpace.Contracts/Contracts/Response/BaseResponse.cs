using System.Text.Json.Serialization;

namespace ProSpace.Contracts.Responses
{
    /// <summary>
    /// Represents the base structure for all API responses.
    /// Used directly to return operational failures and validation errors.
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Gets a value indicating whether the operation completed successfully.
        /// </summary>
        /// <example>false</example>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Gets the collection of error messages describing why the operation failed.
        /// This property is omitted from the JSON payload if it is null or empty.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? Errors { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was a success.</param>
        /// <param name="errors">The collection of error messages generated during execution.</param>
        protected BaseResponse(bool isSuccess, IEnumerable<string>? errors)
        {
            IsSuccess = isSuccess;
            Errors = errors != null && errors.Any() ? errors : null;
        }

        /// <summary>
        /// Creates a failed response containing a single error message string.
        /// </summary>
        /// <param name="singleError">The textual description of the application error.</param>
        /// <returns>A failed <see cref="BaseResponse"/> with <see cref="IsSuccess"/> set to false.</returns>
        public static BaseResponse Failure(string singleError)
            => new(false, [singleError]);

        /// <summary>
        /// Creates a failed response containing a collection of structured errors.
        /// </summary>
        /// <param name="errors">A collection of textual error descriptions.</param>
        /// <returns>A failed <see cref="BaseResponse"/> with <see cref="IsSuccess"/> set to false.</returns>
        public static BaseResponse Failure(IEnumerable<string> errors)
            => new(false, errors);
    }

    /// <summary>
    /// Represents an API response for resource creation or deletion operations that return only an identifier.
    /// Excludes the 'Data' payload property from the schema and JSON structure.
    /// </summary>
    public class BaseIdResponse : BaseResponse
    {
        /// <summary>
        /// Gets the unique identifier of the affected, created, or deleted record.
        /// </summary>
        public Guid Id { get; init; }


        /// <summary>
        /// Initializes a new successful instance of the <see cref="BaseIdResponse"/> class with an identifier.
        /// </summary>
        /// <param name="id">The unique identifier associated with the success response.</param>
        private BaseIdResponse(Guid id) : base(true, null)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new failed instance of the <see cref="BaseIdResponse"/> class.
        /// </summary>
        /// <param name="errors">The collection of error messages generated during execution.</param>
        private BaseIdResponse(IEnumerable<string>? errors) : base(false, errors)
        {
            Id = Guid.Empty;
        }

        /// <summary>
        /// Creates a successful response containing only a unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the processed record.</param>
        /// <returns>A successful <see cref="BaseIdResponse"/> containing the ID.</returns>
        public static BaseIdResponse Success(Guid id) => new(id);

        /// <summary>
        /// Creates a failed response containing a single error message typed as <see cref="BaseIdResponse"/>.
        /// </summary>
        /// <param name="singleError">The textual description of the application error.</param>
        /// <returns>A failed <see cref="BaseIdResponse"/> containing the error message.</returns>
        public static new BaseIdResponse Failure(string singleError) => new([singleError]);

        /// <summary>
        /// Creates a failed response containing a collection of structured errors typed as <see cref="BaseIdResponse"/>.
        /// </summary>
        /// <param name="errors">A collection of textual error descriptions.</param>
        /// <returns>A failed <see cref="BaseIdResponse"/> containing the errors collection.</returns>
        public static new BaseIdResponse Failure(IEnumerable<string> errors) => new(errors);
    }

    /// <summary>
    /// Represents a generic wrapper for API responses that deliver a data payload.
    /// Excludes the 'Id' property from the schema, ensuring a clean object representation.
    /// </summary>
    /// <typeparam name="T">The type of the data transfer object payload. Must be a reference type.</typeparam>
    public class BaseResponse<T> : BaseResponse where T : class
    {
        /// <summary>
        /// Gets the primary data payload object of the operation.
        /// This property is omitted from the JSON payload if it is null.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        /// <summary>
        /// Initializes a new successful instance of the <see cref="BaseResponse{T}"/> class with a data payload.
        /// </summary>
        /// <param name="data">The retrieved or processed single data payload object.</param>
        private BaseResponse(T data) : base(true, null)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new failed instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="errors">The collection of error messages generated during execution.</param>
        private BaseResponse(IEnumerable<string>? errors) : base(false, errors)
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful response containing a data transfer object.
        /// </summary>
        /// <param name="data">The retrieved single data payload object.</param>
        /// <returns>A successful <see cref="BaseResponse{T}"/> containing the payload.</returns>
        public static BaseResponse<T> Success(T data) => new(data);

        /// <summary>
        /// Creates a failed response containing a single error message typed as <see cref="BaseResponse{T}"/>.
        /// </summary>
        /// <param name="singleError">The textual description of the application error.</param>
        /// <returns>A failed <see cref="BaseResponse{T}"/> containing the error message.</returns>
        public static new BaseResponse<T> Failure(string singleError) => new([singleError]);

        /// <summary>
        /// Creates a failed response containing a collection of structured errors typed as <see cref="BaseResponse{T}"/>.
        /// </summary>
        /// <param name="errors">A collection of textual error descriptions.</param>
        /// <returns>A failed <see cref="BaseResponse{T}"/> containing the errors collection.</returns>
        public static new BaseResponse<T> Failure(IEnumerable<string> errors) => new(errors);
    }
}
