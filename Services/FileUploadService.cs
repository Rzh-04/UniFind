using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace UniversityLostAndFound.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public FileUploadService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string folderName = "items")
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Invalid image file format. Only JPG, PNG, GIF, and WEBP images are allowed.");
            }

            // Create target folder under wwwroot if it doesn't exist
            var uploadsRoot = _configuration["UploadsPath"]
                ?? Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uploadsFolderPath = Path.Combine(uploadsRoot, folderName);
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Generate unique filename to avoid overwrites
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path for browser consumption
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var relativePath = imagePath.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar);
            var uploadsRoot = _configuration["UploadsPath"]
                ?? Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uploadsPrefix = "uploads" + Path.DirectorySeparatorChar;
            if (relativePath.StartsWith(uploadsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath[uploadsPrefix.Length..];
            }

            var fullPath = Path.Combine(uploadsRoot, relativePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Ignore delete errors during cleanup
                }
            }
        }
    }
}
