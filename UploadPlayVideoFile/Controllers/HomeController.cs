using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using UploadPlayVideoFile.Models;

namespace UploadPlayVideoFile.Controllers
{
    public class HomeController : Controller
    {
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm", ".flv",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".zip", ".rar", ".7z", ".tar", ".gz"
        };

        private const long MaxFileSize = 500L * 1024 * 1024; // 500 MB

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [HttpGet]
        public ActionResult MediaFiles()
        {
            List<GetMediaFiles_Result> listMediaFiles;
            using (var db = new TestingDBEntities())
            {
                listMediaFiles = db.GetMediaFiles().ToList();
            }
            return View(listMediaFiles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MediaFiles(HttpPostedFileBase httpPostedFileBase)
        {
            if (httpPostedFileBase == null || httpPostedFileBase.ContentLength == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("MediaFiles");
            }

            string fileExtension = Path.GetExtension(httpPostedFileBase.FileName);
            if (!AllowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "File type not allowed. Supported: video, PDF, documents, images, archives.";
                return RedirectToAction("MediaFiles");
            }

            if (httpPostedFileBase.ContentLength > MaxFileSize)
            {
                TempData["Error"] = $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)} MB.";
                return RedirectToAction("MediaFiles");
            }

            try
            {
                string fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(httpPostedFileBase.FileName));
                string uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                string uploadPath = Server.MapPath("~/UploadMediaFiles/");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fullPath = Path.Combine(uploadPath, uniqueFileName);
                httpPostedFileBase.SaveAs(fullPath);

                using (var db = new TestingDBEntities())
                {
                    var mediaFile = new MediaFile
                    {
                        filename = Path.GetFileNameWithoutExtension(httpPostedFileBase.FileName),
                        filesize = (int)(httpPostedFileBase.ContentLength / 1024),
                        filepath = "~/UploadMediaFiles/" + uniqueFileName
                    };
                    db.MediaFiles.Add(mediaFile);
                    db.SaveChanges();
                }

                TempData["Success"] = "File uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Upload failed: " + ex.Message;
            }

            return RedirectToAction("MediaFiles");
        }

        [HttpPost]
        public JsonResult InitializeChunkedUpload(string fileName, long fileSize)
        {
            try
            {
                string fileExtension = Path.GetExtension(fileName);
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, error = "File type not allowed." });
                }

                if (fileSize > MaxFileSize)
                {
                    return Json(new { success = false, error = $"File size exceeds {MaxFileSize / (1024 * 1024)} MB limit." });
                }

                string uploadId = Guid.NewGuid().ToString("N");
                string tempPath = Server.MapPath($"~/App_Data/Temp/{uploadId}/");
                Directory.CreateDirectory(tempPath);

                var metadata = new
                {
                    OriginalFileName = fileName,
                    FileSize = fileSize,
                    TotalChunks = 0,
                    UploadedChunks = new List<int>()
                };

                System.IO.File.WriteAllText(
                    Path.Combine(tempPath, "metadata.json"),
                    Newtonsoft.Json.JsonConvert.SerializeObject(metadata)
                );

                return Json(new { success = true, uploadId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UploadChunk(string uploadId, int chunkIndex, int totalChunks)
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(new { success = false, error = "No chunk data received." });
                }

                var chunk = Request.Files[0];
                string tempPath = Server.MapPath($"~/App_Data/Temp/{uploadId}/");

                if (!Directory.Exists(tempPath))
                {
                    return Json(new { success = false, error = "Invalid upload session." });
                }

                string chunkPath = Path.Combine(tempPath, $"chunk_{chunkIndex}");
                chunk.SaveAs(chunkPath);

                return Json(new { success = true, chunkIndex });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CompleteChunkedUpload(string uploadId, int totalChunks)
        {
            try
            {
                string tempPath = Server.MapPath($"~/App_Data/Temp/{uploadId}/");
                string metadataPath = Path.Combine(tempPath, "metadata.json");

                if (!System.IO.File.Exists(metadataPath))
                {
                    return Json(new { success = false, error = "Invalid upload session." });
                }

                var metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(
                    System.IO.File.ReadAllText(metadataPath)
                );

                string originalFileName = (string)metadata.OriginalFileName;
                string fileExtension = Path.GetExtension(originalFileName);
                string sanitizedName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName));
                string uniqueFileName = $"{sanitizedName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                string uploadDir = Server.MapPath("~/UploadMediaFiles/");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string finalPath = Path.Combine(uploadDir, uniqueFileName);

                using (var outputStream = new FileStream(finalPath, FileMode.Create))
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        string chunkPath = Path.Combine(tempPath, $"chunk_{i}");
                        if (!System.IO.File.Exists(chunkPath))
                        {
                            return Json(new { success = false, error = $"Missing chunk {i}." });
                        }

                        byte[] chunkData = System.IO.File.ReadAllBytes(chunkPath);
                        outputStream.Write(chunkData, 0, chunkData.Length);
                    }
                }

                long fileSize = new FileInfo(finalPath).Length;

                using (var db = new TestingDBEntities())
                {
                    var mediaFile = new MediaFile
                    {
                        filename = Path.GetFileNameWithoutExtension(originalFileName),
                        filesize = (int)(fileSize / 1024),
                        filepath = "~/UploadMediaFiles/" + uniqueFileName
                    };
                    db.MediaFiles.Add(mediaFile);
                    db.SaveChanges();
                }

                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch { }

                return Json(new { success = true, fileName = uniqueFileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteFile(int id)
        {
            try
            {
                using (var db = new TestingDBEntities())
                {
                    var file = db.MediaFiles.Find(id);
                    if (file == null)
                    {
                        return Json(new { success = false, error = "File not found." });
                    }

                    string physicalPath = Server.MapPath(file.filepath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }

                    db.MediaFiles.Remove(file);
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            string result = (fileName ?? "").ToLower();
            result = Regex.Replace(result, @"\&+", "and");
            result = result.Replace("'", "");
            result = Regex.Replace(result, @"[^a-z0-9]", "-");
            result = Regex.Replace(result, @"-+", "-");
            result = result.Trim('-');
            return string.IsNullOrEmpty(result) ? "file" : result;
        }
    }
}
