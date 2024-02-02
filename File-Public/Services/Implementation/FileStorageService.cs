using AutoMapper;
using File_Public.Constants;
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
        private readonly IMapper _mapper;

        public FileStorageService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IMapper mappper
        )
        {
            _context = context;
            _configuration = configuration;
            _mapper = mappper;

            _baseFilePath = _configuration.GetSection("FileStorage").GetValue<string>("FileLocation") ?? String.Empty;
        }
        public async Task<List<VmFileModel>> GetFilesAsListAsync(GetFileDto dto)
        {
            if(dto.ClientId.IsNullOrEmpty() || dto.DocGroup.IsNullOrEmpty()) {
                throw new ArgumentException($"client id or type or both are missing");
            }

            var type = await _context.DocGroups.FirstOrDefaultAsync(x => x.DocGroup == dto.DocGroup);

            if(type.IsNullOrEmpty()) {
                throw new ArgumentException($"Provided file type[{dto.DocGroup}] is not supported");
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
                if (dto.ClientId.IsNullOrEmpty()) {
                    throw new ArgumentException($"Client id is required");
                }

                if (dto.DocGroup.IsNullOrEmpty()) {
                    throw new ArgumentException("Doc type is required");
                }

                var type = await _context.DocGroups.FirstOrDefaultAsync(x => x.DocGroup == dto.DocGroup);

                if (type.IsNullOrEmpty()) {
                    throw new ArgumentException($"Provided file type[{dto.DocGroup}] is not supported");
                }

                IQueryable<Document> query = MakeQuery(dto);

                var files = await query.Select(x => new VmFileNameAndExtension {
                    DocName = x.DocName,
                    DocExt = x.DocExt,
                    ClientId = x.ClientId.ToString(),
                    DocGroup = x.DocGroup,
                    Isin = x.ISIN,
                    Language = x.Language,
                    DocDate  = x.DocDate
                }).ToListAsync();

                return files;
            }catch(Exception ex) {
                throw ex;
            }
            
        }
        public async Task<byte[]> GetZipBytesArray(GetFileDto dto)
        {
            try {
                if (dto.ClientId.IsNullOrEmpty()) {
                    throw new ArgumentException($"Client id is required");
                }
                if (dto.DocGroup.IsNullOrEmpty()) {
                    throw new ArgumentException($"Doc type is required");
                }

                var type = await _context.DocGroups.FirstOrDefaultAsync(x => x.DocGroup == dto.DocGroup);

                if (type.IsNullOrEmpty()) {
                    throw new ArgumentException($"Provided file type[{dto.DocGroup}] is not supported");
                }

                IQueryable<Document> query = MakeQuery(dto);

                var files = await query.Select(x => new VmFileNameAndExtension {
                    DocName = x.DocName,
                    DocExt = x.DocExt,
                    ClientId = x.ClientId.ToString(),
                    DocGroup = x.DocGroup,
                    Isin = x.ISIN,
                    Language = x.Language,
                    DocDate = x.DocDate
                }).ToListAsync();

                using (var memoryStream = new MemoryStream()) {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                        foreach (var file in files) {
                            //string fileName = $"{file.Isin}_{file.Language}_{file.DocGroup}.{file.DocExt}";
                            string fileName = $"{file.DocName}_{file.Language}.{file.DocExt}";

                            string filePath = $"{_baseFilePath}\\{file.ClientId}\\{file.DocGroup}\\{file.DocDate.ToString(DateTimeConstant.DateTimeFormat)}\\{fileName}";
                            //var entry = archive.CreateEntry(Path.GetFileName(filePath));
                            string fileNameForClient = $"{file.Isin}_{file.Language}_{file.DocGroup}.{file.DocExt}";
                            var entry = archive.CreateEntry(fileNameForClient);
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
            if(dto.ClientId.IsNullOrEmpty() || dto.DocGroup.IsNullOrEmpty()) {
                throw new ArgumentException($"client id or type or both are missing");
            }

            var type = await _context.DocGroups.FirstOrDefaultAsync(x => x.DocGroup == dto.DocGroup);

            if(type.IsNullOrEmpty()) {
                throw new ArgumentException($"Provided file type[{dto.DocGroup}] is not supported");
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

                var type = await _context.DocGroups.FirstOrDefaultAsync(x => x.DocGroup == file.DocGroup);

                if (type.IsNullOrEmpty()) {
                    throw new Exception($"Invalid doc type[{file.DocGroup}]!");
                }


                var queryparams = _mapper.Map<GetFileDto>(file);
                IQueryable<Document> query = MakeQuery(queryparams);
                var fileDetails = await query.OrderByDescending(x => x.UploadDate).FirstOrDefaultAsync();
                

                if (!fileDetails.IsNullOrEmpty()) {
                    //string fileName = $"{file.Isin}_{file.Language}_{file.DocName}.{file.DocExt}";
                    string fileName = $"{fileDetails.DocName}_{fileDetails.Language}.{fileDetails.DocExt}";

                    string path = $"{_baseFilePath}\\{fileDetails.ClientId}\\{fileDetails.DocGroup}\\{fileDetails.DocDate.ToString(DateTimeConstant.DateTimeFormat)}\\{fileName}";


                    if (Path.Exists(path)) {
                        var fileStream = new FileStream(path, FileMode.Open);

                        var dto = new VmFileStreamAndName {
                            FileStream = fileStream,
                            Name = $"{fileDetails.ISIN}_{fileDetails.Language}_{fileDetails.DocGroup}.{fileDetails.DocExt}"
                        };
                        return dto;
                    }

                    throw new Exception("No file found in the local storage!");
                }
                throw new Exception("No file found in the database!");

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

            if (!string.IsNullOrEmpty(dto.ClientId)) {
                query = query.Where(d => d.ClientId.ToString() == dto.ClientId);
            }

            if (!dto.ISIN.IsNullOrEmpty()) {
                query = query.Where(d => d.ISIN == dto.ISIN);
            }

            if (!dto.Lang.IsNullOrEmpty()) {
                query = query.Where(d => d.Language == dto.Lang);
            }

            if (!dto.DocGroup.IsNullOrEmpty()) {
                query = query.Where(d => d.DocGroup == dto.DocGroup);
            }

            if (!dto.DocName.IsNullOrEmpty()) {
                query = query.Where(d => d.DocName == dto.DocName);
            }

            if (!dto.DocExt.IsNullOrEmpty()) {
                query = query.Where(d => d.DocExt == dto.DocExt);
            }

            if (dto.DocDate.HasValue) {
                query = query.Where(d => d.DocDate == dto.DocDate);
            }

            return query;
        }
    }
}
