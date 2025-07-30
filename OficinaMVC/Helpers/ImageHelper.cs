using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides functionality to upload images to the server.
    /// </summary>
    public class ImageHelper : IImageHelper
    {
        /// <inheritdoc />
        public async Task<string> UploadImageAsync(IFormFile imageFile, string folder)
        {
            string guid = Guid.NewGuid().ToString();
            string file = $"{guid}.jpg";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{folder}");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, file);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{file}";
        }
    }
}