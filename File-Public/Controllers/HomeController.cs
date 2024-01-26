using File_Public.Data;
using File_Public.DbModels;
using File_Public.Extensions;
using File_Public.Models;
using File_Public.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace File_Public.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly ApplicationDbContext _context;

        private readonly string _baseFilePath;

        public HomeController(
            ILogger<HomeController> logger,
            IFileStorageService fileStorageService,
             ApplicationDbContext context,
             IConfiguration configuration
            )
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
            _context = context;

            _baseFilePath = configuration.GetSection("FileStorage").GetValue<string>("FileLocation") ?? String.Empty;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetFileOrZip(GetFileDto dto)
        {
            if (dto.clientid.IsNullOrEmpty() || dto.type.IsNullOrEmpty()) {
               return BadRequest($"client id or type or both are missing");
            }

            var type = await _context.DocTypes.FirstOrDefaultAsync(x => x.DocType == dto.type);

            if (type.IsNullOrEmpty()) {
                return BadRequest($"Provided file type[{dto.type}] is not supported");
            }

            IQueryable<Document> query = MakeQuery(dto);

            var files = await query.Select(x => new VmFileNameAndExtension {
                DocName = x.DocName,
                DocExt = x.DocExt
            }).ToListAsync();

            if (files.Count == 1) {
                string filePath = $"{_baseFilePath}\\{files[0].DocName}.{files[0].DocExt}";

                if (System.IO.File.Exists(filePath)) {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);

                    //var fileContent = new ByteArrayContent(fileBytes);
                    var fileStream = new FileStream(filePath, FileMode.Open);
                    return File(fileStream, "application/octet-stream", Path.GetFileName(filePath));
                }
            } else if (files.Count > 1) {

                using (var memoryStream = new MemoryStream()) {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                        foreach (var file in files) {
                            string filePath = $"{_baseFilePath}\\{file.DocName}.{file.DocExt}";
                            var entry = archive.CreateEntry(Path.GetFileName(filePath));
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(filePath, FileMode.Open)) {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    }
                    var zipBytes = memoryStream.ToArray();

                    memoryStream.Dispose();
                    return File(zipBytes, "application/octet-stream", "multiple_files.zip");
                }
            }
            return BadRequest("No file available");
        }


        public async Task<IActionResult> GetFilesFromFrontEnd(GetFileDto dto)
        {
            MultipartContent multipartContent = await _fileStorageService.GetFilesAsync(dto);
            return new ObjectResult(multipartContent) { StatusCode = 200 };
        }
        public async Task<IActionResult> GetFiles(GetFileDto dto)
        {
            var files = await _fileStorageService.GetFilesAsListAsync(dto);

            foreach (var file in files) {
                // Set the response headers for each file
                Response.Headers.Add("Content-Disposition", $"attachment; filename={file.FileName}");
                Response.ContentType = "application/text";

                // Copy the file content to the response stream
                await file.Content.CopyToAsync(Response.Body, 81920); // 81920 bytes (80 KB) per chunk

                // Ensure the response is fully sent before moving to the next file
                await Response.Body.FlushAsync();
            }

            // Clear the response headers after all files have been sent
            Response.Headers.Clear();

            return new EmptyResult();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IQueryable<Document> MakeQuery(GetFileDto dto)
        {
            var query = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(dto.clientid)) {
                query = query.Where(d => d.ClientId.ToString() == dto.clientid);
            }

            if (!string.IsNullOrEmpty(dto.Isin)) {
                query = query.Where(d => d.ISIN == dto.Isin);
            }

            if (!string.IsNullOrEmpty(dto.lang)) {
                query = query.Where(d => d.Language == dto.lang);
            }

            if (!string.IsNullOrEmpty(dto.type)) {
                query = query.Where(d => d.DocType == dto.type);
            }

            return query;
        }
    }
}
