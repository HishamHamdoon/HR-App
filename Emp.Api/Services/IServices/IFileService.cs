namespace Emp.Api.Services.IServices
{
    public interface IFileService
        {
            Task<string> SaveFileAsync(IFormFile file, string folderName);
            void DeleteFile(string relativePath);
        }
}
