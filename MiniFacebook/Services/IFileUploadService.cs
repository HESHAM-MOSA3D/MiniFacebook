using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MiniFacebook.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        void DeleteFile(string fileUrl);
    }
}
