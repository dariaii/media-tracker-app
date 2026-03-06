using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MediaTracker.Core.Services
{
    public interface IFileService
    {
        Task<string> UploadProfilePicture(IFormFile file);
    }

    public class FileService(IHostEnvironment webHostEnvironment) : IFileService
    {
        private readonly IHostEnvironment _webHostEnvironment = webHostEnvironment;

        public async Task<string> UploadProfilePicture(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string dirPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot\\profile-pictures");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(dirPath, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return uniqueFileName;
            }

            return null;
        }
    }
}