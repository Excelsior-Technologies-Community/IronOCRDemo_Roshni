using IronOCRDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using IronOcr;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace IronOCRDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var ocr = new IronTesseract();

                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    ms.Position = 0;

                    using (var input = new OcrInput(ms))
                    {
                        var result = ocr.Read(input);
                        ViewBag.OCRText = result.Text;
                    }
                }
            }

            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
