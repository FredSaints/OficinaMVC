namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Defines a helper for uploading images to the server.
    /// </summary>
    public interface IImageHelper
    {
        /// <summary>
        /// Uploads an image file to the specified folder and returns the relative path.
        /// </summary>
        /// <param name="imageFile">The image file to upload.</param>
        /// <param name="folder">The folder to upload the image to.</param>
        /// <returns>The relative path to the uploaded image.</returns>
        Task<string> UploadImageAsync(IFormFile imageFile, string folder);
    }
}
