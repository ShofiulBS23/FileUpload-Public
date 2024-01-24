using File_Public.Data;
using File_Public.DbModels;
using File_Public.Extensions;
using File_Public.Models;
using File_Public.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
