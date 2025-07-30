namespace OficinaMVC.Models
{
    /// <summary>
    /// ViewModel representing error details for error views.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the request identifier associated with the error.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the RequestId should be shown.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Gets or sets the original path where the error occurred.
        /// </summary>
        public string? OriginalPath { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with the error.
        /// </summary>
        public int StatusCode { get; set; }
    }
}