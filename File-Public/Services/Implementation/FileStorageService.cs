using File_Public.Data;
using File_Public.DbModels;
using File_Public.Extensions;
using File_Public.Models;
using File_Public.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace File_Public.Services.Implementation
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _baseFilePath;

        public FileStorageService(
            ApplicationDbContext context,
            IConfiguration configuration
        )
        {
            _context = context;
            _configuration = configuration;

            _baseFilePath = _configuration.GetSection("FileStorage").GetValue<string>("FileLocation") ?? String.Empty;
        }
        public async Task<List<VmFileModel>> GetFilesAsListAsync(GetFileDto dto)
        {
            if(dto.clientid.IsNullOrEmpty() || dto.type.IsNullOrEmpty()) {
                throw new ArgumentException($"client id or type or both are missing");
            }

            var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == dto.type);

            if(type.IsNullOrEmpty()) {
                throw new ArgumentException($"Provided file type[{dto.type}] is not supported");
            }

            IQueryable<Document> query = MakeQuery(dto);

            var files = await query.Select(x => new VmFileNameAndExtension {
                DocName = x.DocName,
                DocExt = x.DocExt
             }).ToListAsync();

            var res =  GetFilesFromLocalAsListStorage(files);
            return res;
        }
        public async Task<List<VmFileNameAndExtension>> GetFilesStatusAsync(GetFileDto dto)
        {
            try {
                if (dto.clientid.IsNullOrEmpty()) {
                    throw new ArgumentException($"Client id is required");
                }

                if (dto.type.IsNullOrEmpty()) {
                    throw new ArgumentException("Doc type is required");
                }

                var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == dto.type);

                if (type.IsNullOrEmpty()) {
                    throw new ArgumentException($"Provided file type[{dto.type}] is not supported");
                }

                IQueryable<Document> query = MakeQuery(dto);

                var files = await query.Select(x => new VmFileNameAndExtension {
                    DocName = x.DocName,
                    DocExt = x.DocExt,
                    ClientId = x.ClientId.ToString(),
                    DocType = x.DocType,
                    Isin = x.ISIN,
                    Language = x.Language
                }).ToListAsync();

                return files;
            }catch(Exception ex) {
                throw ex;
            }
            
        }
        public async Task<byte[]> GetZipBytesArray(GetFileDto dto)
        {
            try {
                if (dto.clientid.IsNullOrEmpty()) {
                    throw new ArgumentException($"Client id is required");
                }
                if (dto.type.IsNullOrEmpty()) {
                    throw new ArgumentException($"Doc type is required");
                }

                var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == dto.type);

                if (type.IsNullOrEmpty()) {
                    throw new ArgumentException($"Provided file type[{dto.type}] is not supported");
                }

                IQueryable<Document> query = MakeQuery(dto);

                var files = await query.Select(x => new VmFileNameAndExtension {
                    DocName = x.DocName,
                    DocExt = x.DocExt,
                    ClientId = x.ClientId.ToString(),
                    DocType = x.DocType
                }).ToListAsync();

                using (var memoryStream = new MemoryStream()) {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                        foreach (var file in files) {
                            string filePath = $"{_baseFilePath}\\{file.ClientId}\\{file.DocType}\\{file.DocName}.{file.DocExt}";
                            var entry = archive.CreateEntry(Path.GetFileName(filePath));
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(filePath, FileMode.Open)) {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    }
                    var zipBytes = memoryStream.ToArray();

                    memoryStream.Dispose();
                    return zipBytes;
                }
            } catch(Exception ex) {
                throw ex;
            }

        }

        private List<VmFileModel> GetFilesFromLocalAsListStorage(List<VmFileNameAndExtension> fileList)
        {
            List<string> files = new();
            List <VmFileModel> ResultList = new();

            foreach (var file in fileList) {
                files.Add($"{_baseFilePath}\\{file.DocName}.{file.DocExt}");
            }

            foreach (var file in files) {
                var filePath = file;

                if (File.Exists(filePath)) {
                    var fileBytes = File.ReadAllBytes(filePath);

                    //var fileContent = new ByteArrayContent(fileBytes);
                    var fileContentx = new MemoryStream(fileBytes);
                    ResultList.Add(new VmFileModel {
                        FileName = Path.GetFileName(filePath),
                        Content = fileContentx
                    });
                    

                }
            }
            return ResultList;
        }

        public async Task<MultipartContent> GetFilesAsync(GetFileDto dto)
        {
            if(dto.clientid.IsNullOrEmpty() || dto.type.IsNullOrEmpty()) {
                throw new ArgumentException($"client id or type or both are missing");
            }

            var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == dto.type);

            if(type.IsNullOrEmpty()) {
                throw new ArgumentException($"Provided file type[{dto.type}] is not supported");
            }

            IQueryable<Document> query = MakeQuery(dto);

            var files = await query.Select(x => new VmFileNameAndExtension {
                DocName = x.DocName,
                DocExt = x.DocExt
             }).ToListAsync();

            var res =  GetFilesFromLocalStorage(files);
            return res;
        }
        public async Task<VmFileStreamAndName> GetFileAsync(VmFileNameAndExtension file)
        {
            try {
                if (file.ClientId.IsNullOrEmpty()) {
                    throw new Exception("Client id is required");
                }

                var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == file.DocType);

                if (type.IsNullOrEmpty()) {
                    throw new Exception($"Invalid doc type[{file.DocType}]!");
                }

                string path = $"{_baseFilePath}\\{file.ClientId}\\{file.DocType}\\{file.DocName}.{file.DocExt}";

                if (Path.Exists(path)) {
                    var fileStream = new FileStream(path, FileMode.Open);

                    var dto = new VmFileStreamAndName {
                        FileStream = fileStream,
                        Name = Path.GetFileName(path)
                    };
                    return dto;
                }

                throw new Exception("No file found in the local storage!");
            } catch(Exception ex) {
                throw ex;
            }
        }

        private MultipartContent GetFilesFromLocalStorage(List<VmFileNameAndExtension> fileList)
        {
            try {

                List<string> files = new();

                foreach(var file in fileList) {
                    files.Add($"{_baseFilePath}\\{file.DocName}.{file.DocExt}");
                }

                //var files = new List<string> { "file1.txt", "file2.txt", "file3.txt" };

                var multipartContent = new MultipartContent("mixed");

                foreach (var file in files) {
                    var filePath = file;

                    if (File.Exists(filePath)) {
                        var fileBytes = File.ReadAllBytes(filePath);

                        var fileContent = new ByteArrayContent(fileBytes);
                        var filenameHeader = $"attachment; filename=\"{Path.GetFileName(filePath)}\"";
                        fileContent.Headers.Add("Content-Disposition", filenameHeader);
                        fileContent.Headers.Add("Content-Type", GetContentType(filePath));

                        multipartContent.Add(fileContent);
                    }
                }
                return multipartContent;
            } catch(Exception ex) {
                throw ex;
            }
        }
        private string GetContentType(string fileName)
        {
            if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) {
                return "text/plain";
            } else if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) {
                return "application/pdf";
            } else {
                return "application/octet-stream"; // Default content type
            }

        }
       private IQueryable<Document> MakeQuery(GetFileDto dto)
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(dto.clientid)) {
                query = query.Where(d => d.ClientId.ToString() == dto.clientid);
            }

            if (!dto.Isin.IsNullOrEmpty()) {
                query = query.Where(d => d.ISIN == dto.Isin);
            }

            if (!dto.lang.IsNullOrEmpty()) {
                query = query.Where(d => d.Language == dto.lang);
            }

            if (!dto.type.IsNullOrEmpty()) {
                query = query.Where(d => d.DocType == dto.type);
            }

            return query;
        }
    }
}
