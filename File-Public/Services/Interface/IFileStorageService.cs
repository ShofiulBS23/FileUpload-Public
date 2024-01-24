using File_Public.Models;
using System.IO;

namespace File_Public.Services.Interface
{
    public interface IFileStorageService
    {
        Task<MultipartContent> GetFilesAsync(GetFileDto dto);
        Task<List<VmFileModel>> GetFilesAsListAsync(GetFileDto dto);
    }
}
