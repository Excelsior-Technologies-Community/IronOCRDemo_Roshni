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
        private readonly IWebHostEnvironment _ev;
        public HomeController(IWebHostEnvironment ev)
        {
            _ev = ev;
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
        public IActionResult BarcodeQR()
        {
            return View();
        }
        [HttpPost]
        public IActionResult BarcodeQR(IFormFile file)
        {
            IronOcr.License.LicenseKey = "IRONSUITE.EXCELSIORTECHNOLOGIES.8187-3708CDF60F-B3MFZGNEBBLCW6W5-BXTNM4MPHNMU-OTOEBFCLRDSW-ETTEAFOQHJNT-KBYQIADCMQPR-QVRLMSV47S2Q-GLWMCW-TRH25WCBAZKQUA-SANDBOX-BDIXLR.TRIAL.EXPIRES.27.MAR.2026";
            if (file != null && file.Length > 0)
            {
                // Create uploads folder path
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // Create folder if not exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a safe file path
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the uploaded image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Initialize IronTesseract with barcode reading enabled
                var ocrTesseract = new IronTesseract();
                ocrTesseract.Configuration.ReadBarCodes = true;

                // Perform OCR and Barcode Reading
                using (var ocrInput = new OcrInput())
                {
                    ocrInput.AddImage(filePath); // You can also use LoadImage()
                    var ocrResult = ocrTesseract.Read(ocrInput);

                    ViewBag.TextData = ocrResult.Text;

                    if (ocrResult.Barcodes.Count() > 0)
                    {
                        ViewBag.BarcodeData = ocrResult.Barcodes;
                    }
                    else
                    {
                        ViewBag.BarcodeData = null;
                    }
                }
            }
            else
            {
                ViewBag.TextData = "No file uploaded.";
                ViewBag.BarcodeData = null;
            }

            return View();
        }

        [HttpGet]
        public IActionResult RSDocument()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RSDocument(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                IronOcr.License.LicenseKey = "IRONSUITE.EXCELSIORTECHNOLOGIES.8187-3708CDF60F-B3MFZGNEBBLCW6W5-BXTNM4MPHNMU-OTOEBFCLRDSW-ETTEAFOQHJNT-KBYQIADCMQPR-QVRLMSV47S2Q-GLWMCW-TRH25WCBAZKQUA-SANDBOX-BDIXLR.TRIAL.EXPIRES.27.MAR.2026";

                // Save uploaded file to wwwroot/uploads
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var filePath = Path.Combine(uploadsPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // OCR read scanned document
                var Ocr = new IronTesseract();
                Ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.Auto;
                Ocr.Configuration.EngineMode = TesseractEngineMode.LstmOnly; // Better for printed text
                Ocr.Language = OcrLanguage.English; // You can add others if needed

                using (var input = new OcrInput(filePath))
                {
                    var result = Ocr.Read(input);
                    ViewBag.ExtractedText = result.Text;
                }
            }
            else
            {
                ViewBag.ExtractedText = "Please upload a valid file.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult ReadTable()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadTable(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                IronOcr.License.LicenseKey = "IRONSUITE.EXCELSIORTECHNOLOGIES.8187-3708CDF60F-B3MFZGNEBBLCW6W5-BXTNM4MPHNMU-OTOEBFCLRDSW-ETTEAFOQHJNT-KBYQIADCMQPR-QVRLMSV47S2Q-GLWMCW-TRH25WCBAZKQUA-SANDBOX-BDIXLR.TRIAL.EXPIRES.27.MAR.2026";

                // Save file
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath);
                var filePath = Path.Combine(uploadsPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // OCR
                var ocr = new IronTesseract();
                ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.Auto;
                ocr.Language = OcrLanguage.English;

                string fullText;
                using (var input = new OcrInput(filePath))
                {
                    input.DeNoise();              // Noise reduction
                    input.Deskew();               // Straighten the image
                    input.ToGrayScale();          // Convert to grayscale
                    input.EnhanceResolution();
                    var result = ocr.Read(input);
                    fullText = result.Text;
                }

                // Parse lines as table (basic parsing by line and tab/space separation)
                var tableData = new List<List<string>>();
                var lines = fullText.Split('\n');
                foreach (var line in lines)
                {
                    var row = new List<string>();
                    var columns = line.Split(new[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    row.AddRange(columns);
                    if (row.Count > 0)
                        tableData.Add(row);
                }

                ViewBag.Table = tableData;
            }

            return View();
        }

        public IActionResult ReadLicensePlate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadLicensePlate(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                IronOcr.License.LicenseKey = "IRONSUITE.EXCELSIORTECHNOLOGIES.8187-3708CDF60F-B3MFZGNEBBLCW6W5-BXTNM4MPHNMU-OTOEBFCLRDSW-ETTEAFOQHJNT-KBYQIADCMQPR-QVRLMSV47S2Q-GLWMCW-TRH25WCBAZKQUA-SANDBOX-BDIXLR.TRIAL.EXPIRES.27.MAR.2026";

                // Save uploaded file to wwwroot/uploads
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath);
                var filePath = Path.Combine(uploadsPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var ocr = new IronTesseract();
                using var input = new OcrInput(filePath);

                try
                {
                    // Perform license plate recognition
                    OcrLicensePlateResult result = ocr.ReadLicensePlate(input);

                    ViewBag.LicensePlateText = result.Text;
                    ViewBag.LicensePlateBox = result.Licenseplate.ToString(); // Rectangle box
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error reading license plate: " + ex.Message;
                }
            }

            return View();
        }

        public IActionResult ReadPassport()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadPassport(IFormFile passportImage)
        {
            if (passportImage != null && passportImage.Length > 0)
            {
                try
                {
                    // Save uploaded image to wwwroot/uploads
                    var uploadsFolder = Path.Combine(_ev.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, passportImage.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        passportImage.CopyTo(stream);
                    }

                    IronOcr.License.LicenseKey = "IRONSUITE.EXCELSIORTECHNOLOGIES.8187-3708CDF60F-B3MFZGNEBBLCW6W5-BXTNM4MPHNMU-OTOEBFCLRDSW-ETTEAFOQHJNT-KBYQIADCMQPR-QVRLMSV47S2Q-GLWMCW-TRH25WCBAZKQUA-SANDBOX-BDIXLR.TRIAL.EXPIRES.27.MAR.2026";

                    var ocr = new IronTesseract();
                    using var input = new OcrInput(filePath);
                    var result = ocr.ReadPassport(input);

                    ViewBag.GivenNames = result.PassportInfo.GivenNames;
                    ViewBag.Surname = result.PassportInfo.Surname;
                    ViewBag.Country = result.PassportInfo.Country;
                    ViewBag.PassportNumber = result.PassportInfo.PassportNumber;
                    ViewBag.DateOfBirth = result.PassportInfo.DateOfBirth;
                    ViewBag.DateOfExpiry = result.PassportInfo.DateOfExpiry;
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Failed to process image: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Error = "Please upload a passport image.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult ReadPhoto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadPhoto(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Save uploaded file to wwwroot/uploads
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                try
                {
                    var ocr = new IronTesseract();
                    using var input = new OcrInput(filePath); // Works with .jpg, .png, and .tiff
                    var result = ocr.Read(input);

                    // Combine results into one string for display
                    string output = $"Confidence: {result.Confidence}\n\n{result.Text}";
                    ViewBag.Output = output;
                }
                catch (Exception ex)
                {
                    ViewBag.Output = "Error reading image: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Output = "No file selected.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult ReadScreen()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadScreen(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                try
                {
                    var ocr = new IronTesseract();
                    using var input = new OcrInput();
                    input.LoadImage(filePath);

                    OcrPhotoResult result = ocr.ReadScreenShot(input); // Using ReadScreenShot

                    string output = $"OCR Text:\n{result.Text}\n\n" +
                                    $"First Region X: {result.TextRegions.First().Region.X}\n" +
                                    $"Last Region Width: {result.TextRegions.Last().Region.Width}\n" +
                                    $"Confidence: {result.Confidence}";

                    ViewBag.Output = output;
                }
                catch (Exception ex)
                {
                    ViewBag.Output = "Error reading screenshot: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Output = "No file selected.";
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
