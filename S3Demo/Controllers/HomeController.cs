using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using S3Demo.Data;
using S3Demo.Models;
using System.Diagnostics;

namespace S3Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        private ApplicationDbContext _db { get; set; }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            List<S3FileDetails> s3Files = new List<S3FileDetails>();
            s3Files = _db.S3FileDetails.ToList();
            return View(s3Files);
        }

        //file upload code to s3 and db
        [HttpPost]
        public async Task<IActionResult> UploadFileToS3(IFormFile file)
        { if(file == null)
            {
                ViewBag.Message = "Choose a file";

                return RedirectToAction(nameof(Index));
            }
         
            var accessKeyId = _config["AccessKey:AccessKeyID"];
            var secretAccessKey = _config["AccessKey:SecretAccessKey"];
            using (var amazonS3Client = new AmazonS3Client(accessKeyId, secretAccessKey))
            {
                using(var mempryStream = new MemoryStream() )
                {
                    file.CopyTo( mempryStream );
                    var request = new TransferUtilityUploadRequest
                    {
                        InputStream = mempryStream,
                        Key = file.FileName,
                        BucketName = "m29hasan0290810",
                        ContentType = file.ContentType
                    };

                    var transferutility = new TransferUtility(amazonS3Client);

                    await transferutility.UploadAsync(request);

                }
            }

            S3FileDetails fileDetails = new S3FileDetails();    
            fileDetails.FileName = file.FileName;
            fileDetails.FileDate = DateTime.Today;
            _db.S3FileDetails.Add(fileDetails);
            _db.SaveChanges();
            ViewBag.Message = "File Uploaded Successfully on S3 Bucket";

            return RedirectToAction(nameof(Index));
             
        }


        public IActionResult DeleteFile(int Id)
        {
            S3FileDetails details = new S3FileDetails();
            details = 
                _db.S3FileDetails.FirstOrDefault(x => x.Id == Id);
            return View(details);
        }
        //delete file from s3
        public async Task<IActionResult> DeleteFileToS3(string fileName)
        {
            var accessKeyId = _config["AccessKey:AccessKeyID"];
            var secretAccessKey = _config["AccessKey:SecretAccessKey"];
            using (var amazonS3Client = new AmazonS3Client(accessKeyId, secretAccessKey))
            {
                var transferUtility = new TransferUtility(amazonS3Client);
                await transferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                {
                    BucketName = "m29hasan0290810",
                    Key = fileName

                });

                S3FileDetails fileDetails = new S3FileDetails();
                fileDetails = _db.S3FileDetails.FirstOrDefault(x => x.FileName.ToLower() == fileName.ToLower());
                _db.S3FileDetails.Remove(fileDetails);
                _db.SaveChanges();
                ViewBag.Success = "File Deleted Successfully on S3 Bucket";

                return RedirectToAction(nameof(Index));
            }

         

        }


        public IActionResult ViewFileForDownload(int Id)
        {
            S3FileDetails details = new S3FileDetails();
            details =
                _db.S3FileDetails.FirstOrDefault(x => x.Id == Id);
            return View(details);
        }

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var accessKeyId = _config["AccessKey:AccessKeyID"];
            var secretAccessKey = _config["AccessKey:SecretAccessKey"];
            using (var amazonS3Client = new AmazonS3Client(accessKeyId, secretAccessKey))
            {
                var transferUtility = new TransferUtility(amazonS3Client);
                var response = await transferUtility.S3Client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = "m29hasan0290810",
                    Key = fileName
                });
                if(response.ResponseStream == null)
                {
                    return NotFound();
                }
                return File(response.ResponseStream, response.Headers.ContentType, fileName);
            }



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