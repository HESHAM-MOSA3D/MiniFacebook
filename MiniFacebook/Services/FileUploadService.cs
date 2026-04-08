using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MiniFacebook.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            return "/" + folderName + "/" + uniqueFileName;
        }

        public void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl) || fileUrl.Contains("default-avatar", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                if (fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(fileUrl);
                    fileUrl = uri.AbsolutePath;
                }

                fileUrl = fileUrl.TrimStart('/', '\\');
                var filePath = Path.Combine(_env.WebRootPath, fileUrl);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully: {Path}", filePath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {Path}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Url}", fileUrl);
            }
        }
    }
}