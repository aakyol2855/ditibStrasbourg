namespace DitibStasbourg.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder = "");
        string GetFilePath(string fileName, string subFolder = "");
        void DeleteFile(string fileName, string subFolder = "");
    }
}
