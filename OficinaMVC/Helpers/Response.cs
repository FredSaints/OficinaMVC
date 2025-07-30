namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Represents a standard response for operations, indicating success, message, and results.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the results of the operation.
        /// </summary>
        public object Results;
    }
}
