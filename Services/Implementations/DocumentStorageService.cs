using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Implementations
{
    /// <summary>
    /// Disk-tabanlı belge yükleme servisi.
    /// Dosyaları wwwroot/uploads/{subFolder}/ altında GUID adıyla kaydeder.
    /// Yalnızca PDF, PNG, JPG, JPEG uzantılarına izin verir.
    /// </summary>
    public class DocumentStorageService : IDocumentStorageService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly HashSet<string> _allowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".png", ".jpg", ".jpeg" };

        public DocumentStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <inheritdoc />
        public async Task<string> UploadAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş ya da seçilmedi.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    $"'{extension}' uzantısı desteklenmiyor. Yalnızca PDF, PNG, JPG ve JPEG yüklenebilir.");

            // Güvenli hedef dizin oluştur
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
            var targetDir   = Path.Combine(uploadsRoot, subFolder.Trim('/').Replace("..", ""));

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(targetDir, safeFileName);

            // Path traversal koruması
            var normalizedTarget = Path.GetFullPath(physicalPath);
            var normalizedRoot   = Path.GetFullPath(uploadsRoot);
            if (!normalizedTarget.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Geçersiz dosya yolu tespit edildi.");

            try
            {
                using var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                throw new IOException($"Dosya kaydedilemedi: {ex.Message}", ex);
            }

            // Tarayıcıdan erişilebilir URL yolu döndür
            return $"/uploads/{subFolder.Trim('/')}/{safeFileName}";
        }

        /// <inheritdoc />
        public void Delete(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var physicalPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            var normalizedPath = Path.GetFullPath(physicalPath);
            var normalizedRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));

            if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                return; // Güvenlik: wwwroot/uploads dışı silmeye izin yok

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
    }
}
