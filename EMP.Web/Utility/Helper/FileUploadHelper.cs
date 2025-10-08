namespace EMP.Web.Utility.Helper
{
    public class FileUploadHelper
    {
            private readonly IWebHostEnvironment _env;

            public FileUploadHelper(IWebHostEnvironment env)
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

                // Copy stream
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path (useful for storing in DB)
                return Path.Combine("uploads", folderName, fileName).Replace("\\", "/");
            }
    }
}
