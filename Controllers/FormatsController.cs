using IronOcr;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace IronOCRDemo.Controllers
{
    public class FormatsController : Controller
    {
        
        [HttpGet]
        public IActionResult InputClass()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InputClass(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Save image to wwwroot/uploads
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", imageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // OCR
                var ocr = new IronTesseract();
                using (var input = new OcrInput())
                {
                    input.AddImage(filePath);
                    input.DeNoise();
                    input.Deskew();
                    var result = ocr.Read(input);
                    ViewBag.OcrText = result.Text;
                }

                ViewBag.ImagePath = "/uploads/" + imageFile.FileName;
            }

            return View();
        }

        [HttpGet]
        public IActionResult RegionOcr()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegionOcr(IFormFile imageFile, int x, int y, int width, int height)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, imageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var ocr = new IronTesseract();
                using (var input = new OcrInput())
                {
                    
                    input.AddImage(filePath, new Rectangle(x, y, width, height));

                    var result = ocr.Read(input);
                    ViewBag.OcrText = result.Text;
                }

                ViewBag.ImagePath = "/uploads/" + imageFile.FileName;
            }

            return View();
        }

        [HttpGet]
        public IActionResult OcrDraw()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OcrDraw(IFormFile imageFile, int x, int y, int width, int height)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, imageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var ocr = new IronTesseract();
                using (var input = new OcrInput())
                {
                    // Use IronSoftware.Drawing.Rectangle
                    var region = new Rectangle(x, y, width, height);
                    input.AddImage(filePath, region);

                    var result = ocr.Read(input);
                    ViewBag.OcrText = string.IsNullOrWhiteSpace(result.Text)
                        ? "No text detected in selected region."
                        : result.Text;
                }
            }
            else
            {
                ViewBag.OcrText = "No image uploaded.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult OcrMultiPageTiff()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OcrMultiPageTiff(IFormFile tiffFile)
        {
            if (tiffFile == null || tiffFile.Length == 0)
            {
                ViewBag.OcrText = "Please upload a valid multi-page TIFF file.";
                return View();
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, tiffFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await tiffFile.CopyToAsync(stream);
            }

            try
            {
                var ocr = new IronTesseract();
                using (var input = new OcrInput())
                {
                    // Try loading all pages automatically:
                    input.LoadImage(filePath); // Automatically detects all pages if multipage

                    // Optional: load specific frames
                    // input.LoadImageFrames(filePath, new int[] { 0, 1 }); // 0-based index

                    var result = ocr.Read(input);
                    ViewBag.OcrText = result.Text;
                }
            }
            catch (Exception ex)
            {
                ViewBag.OcrText = "Error processing TIFF file: " + ex.Message;
            }

            return View();
        }
    }
}


