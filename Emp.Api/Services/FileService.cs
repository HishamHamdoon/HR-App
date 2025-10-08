
using Emp.Api.Services.IServices;

namespace Emp.Api.Services
{
    public class FileService : IFileService
    {

        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // Target directory: wwwroot/uploads/{folderName}
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            // Unique file name
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path (for DB storage or URL generation)
            return Path.Combine("uploads", folderName, fileName).Replace("\\", "/");
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
