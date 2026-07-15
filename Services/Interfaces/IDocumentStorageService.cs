namespace DitibStasbourg.Services.Interfaces
{
    /// <summary>
    /// Disk tabanlı belge yükleme servisi.
    /// wwwroot/uploads/{subFolder}/ altında güvenli şekilde dosya kaydeder.
    /// </summary>
    public interface IDocumentStorageService
    {
        /// <summary>
        /// Dosyayı wwwroot/uploads/{subFolder}/ altına kaydeder.
        /// Yalnızca PDF, PNG, JPG, JPEG kabul edilir.
        /// </summary>
        /// <param name="file">Yüklenecek dosya</param>
        /// <param name="subFolder">Alt klasör adı, örn. "dernekler/5" veya "gorevliler/42"</param>
        /// <returns>Tarayıcıdan erişilebilir URL: /uploads/{subFolder}/{guid}.ext</returns>
        Task<string> UploadAsync(IFormFile file, string subFolder);

        /// <summary>
        /// wwwroot/uploads/ içindeki bir dosyayı fiziksel olarak siler.
        /// </summary>
        /// <param name="relativePath">Kaydedilmiş URL yolu, örn. /uploads/dernekler/5/abc.pdf</param>
        void Delete(string relativePath);
    }
}
