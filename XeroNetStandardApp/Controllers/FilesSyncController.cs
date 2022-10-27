using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Files;


namespace XeroNetStandardApp.Controllers
{
  public class FilesSync : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public FilesSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }

        // GET: /FilesSync/
        public async Task<ActionResult> Index()
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

            var FilesApi = new FilesApi();

            var response = await FilesApi.GetFilesAsync(accessToken, xeroTenantId);

            var filesItems = response.Items;

            var jsonString = "";

            foreach (FileObject file in filesItems)
            {
                jsonString += file.ToJson();
            }
            ViewBag.jsonResponse = jsonString;

            return View(filesItems);
        }

        // Get: /Files#Delete
        [HttpGet]
        public async Task<ActionResult> Delete(string fileID)
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

            Guid fileIDGuid = Guid.Parse(fileID);


            var filesApi = new FilesApi();
            await filesApi.DeleteFileAsync(accessToken, xeroTenantId, fileIDGuid);

            return RedirectToAction("Index", "FilesSync");
        }


        // POST: /FilesSync/FileUpload#Upload
        [HttpPost("FilesSyncFileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

            var FilesApi = new FilesApi();

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                    filePaths.Add(filePath);

                    byte[] byteArray;
                    using (MemoryStream data = new MemoryStream())
                    {
                        formFile.CopyTo(data);
                        byteArray = data.ToArray();
                    }

                    await FilesApi.UploadFileAsync(
                        accessToken,
                        xeroTenantId,
                        byteArray,
                        formFile.FileName,
                        formFile.FileName,
                        formFile.ContentType
                    );
                }
            }

            return RedirectToAction("Index", "FilesSync");
        }

        // GET: /FilesSync#Modify
        [HttpGet("/FilesSync/{fileId}")]
        public IActionResult Modify(string fileId, string fileName)
        {
            ViewBag.fileID = fileId;
            ViewBag.fileName = fileName;
            return View();
        }

        // Put: /FilesSync#Rename
        [HttpPost]
        public async Task<ActionResult> Rename(string fileID, string newName)
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

            Guid fileIDGuid = Guid.Parse(fileID);

            var filesApi = new FilesApi();
            FileObject file = await filesApi.GetFileAsync(accessToken, xeroTenantId, fileIDGuid);
            file.Name = newName;

            var response = await filesApi.UpdateFileAsync(accessToken, xeroTenantId, fileIDGuid, file);

            return RedirectToAction("Index", "FilesSync");
        }

    }
}
