using IronOcr;
using Microsoft.AspNetCore.Mvc;

namespace IronOCRDemo.Controllers
{
    public class TesseractController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReadPdf()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.OCRText = "Please upload a PDF file.";
                return View();
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            try
            {
                var ocrTesseract = new IronTesseract();
                using var ocrInput = new OcrInput();
                ocrInput.LoadPdf(filePath);
                ocrInput.Deskew(); // Optional: improves scan quality

                var result = ocrTesseract.Read(ocrInput);

                ViewBag.OCRText = result.Text;
                ViewBag.Confidence = result.Confidence;
            }
            catch (Exception ex)
            {
                ViewBag.OCRText = "Error: " + ex.Message;
            }

            return View();
        }
    }
}

