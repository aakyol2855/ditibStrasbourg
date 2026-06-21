using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public FileStorageService(IWebHostEnvironment env)
        {
            // Store outside wwwroot as per security requirements
            _basePath = Path.Combine(env.ContentRootPath, "InternalStorage", "Uploads");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder = "")
        {
            if (file == null || file.Length == 0) return string.Empty;

            var targetFolder = Path.Combine(_basePath, subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public string GetFilePath(string fileName, string subFolder = "")
        {
            // Security hardening: neutralize path traversal
            var cleanFileName = Path.GetFileName(fileName);
            var baseDirectory = Path.GetFullPath(_basePath);
            var targetFolder = Path.GetFullPath(Path.Combine(baseDirectory, subFolder ?? string.Empty));
            var fullPath = Path.GetFullPath(Path.Combine(targetFolder, cleanFileName));

            if (!fullPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Path traversal attempt blocked.");
            }

            return fullPath;
        }

        public void DeleteFile(string fileName, string subFolder = "")
        {
            var filePath = GetFilePath(fileName, subFolder);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
