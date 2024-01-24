using File_Public.Models;
using File_Public.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Compression;

namespace File_Public.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileStorageService _fileStorageService;

        public HomeController(
            ILogger<HomeController> logger,
            IFileStorageService fileStorageService
            )
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
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
    }
}
