using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DitibStasbourg.Controllers
{
    [Authorize]
    public class SecureFileController : Controller
    {
        private readonly IFileStorageService _fileStorage;

        public SecureFileController(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        [HttpGet]
        public IActionResult Download(string fileName, string subFolder = "")
        {
            // Mandatory claim check for security
            if (!User.HasClaim("Permission", "Admin-DataImport") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            try
            {
                var filePath = _fileStorage.GetFilePath(fileName, subFolder);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var mimeType = "application/octet-stream";
                if (fileName.EndsWith(".xlsx")) mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                
                return PhysicalFile(filePath, mimeType, fileName);
            }
            catch (System.UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
