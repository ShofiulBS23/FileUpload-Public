using File_Public.Data;
using File_Public.Extensions;
using File_Public.Models;
using File_Public.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            var files = await _fileStorageService.GetFilesStatusAsync(dto);

            if (files.Count == 1) {
                var file = await _fileStorageService.GetFileAsync(files[0]);

                if (file.IsNullOrEmpty()) {
                    return NotFound("File not found in local storage");
                }
                return File(file.FileStream, "application/octet-stream", file.Name);
            } else if (files.Count > 1) {
                    return View(files);
                }
            return BadRequest("No file available");
        }


        public async Task<IActionResult> GetFiles(GetFileDto dto)
        {
            var zipBytes = await _fileStorageService.GetZipBytesArray(dto);
            return File(zipBytes, "application/zip", $"{dto.clientid}_{dto.type}.zip");
        }


        public async Task<IActionResult> GetFilesFromFrontEnd(GetFileDto dto)
        {
            MultipartContent multipartContent = await _fileStorageService.GetFilesAsync(dto);
            return new ObjectResult(multipartContent) { StatusCode = 200 };
        }
        public async Task<IActionResult> GetFilesMulti(GetFileDto dto)
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
    }
}
