using File_Public.Models;
using System.IO;

namespace File_Public.Services.Interface
{
    public interface IFileStorageService
    {
        Task<MultipartContent> GetFilesAsync(GetFileDto dto);
        Task<VmFileStreamAndName> GetFileAsync(VmFileNameAndExtension file);
        Task<byte[]> GetZipBytesArray(GetFileDto dto);
        Task<List<VmFileModel>> GetFilesAsListAsync(GetFileDto dto);
        Task<List<VmFileNameAndExtension>> GetFilesStatusAsync(GetFileDto dto);
    }
}
