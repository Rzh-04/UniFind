using Microsoft.AspNetCore.Http;

namespace UniversityLostAndFound.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folderName = "items");
        void DeleteImage(string imagePath);
    }
}
