using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Files;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


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
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

      var FilesApi = new FilesApi();

      var response = await FilesApi.GetFilesAsync(accessToken, xeroTenantId);

      var filesItems = response.Items;

      var jsonString = "";

      foreach(FileObject file in filesItems){
        jsonString += file.ToJson();
      }
      ViewBag.jsonResponse = jsonString;

      return View(filesItems);
    }

    // Get: /Files#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string fileID)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();      
      
      Guid fileIDGuid = Guid.Parse(fileID);


      var filesApi = new FilesApi();
      await filesApi.DeleteFileAsync(accessToken, xeroTenantId, fileIDGuid);
      
      return RedirectToAction("Index", "FilesSync");
    }


    // POST: /FilesSync/FileUpload#Upload
    // CAUSING EXCEPTION!
    [HttpPost("FilesSyncFileUpload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

      var FilesApi = new FilesApi();

      // Convet IFormFile to byte array
      byte[] byteArray; 
      using (MemoryStream data = new MemoryStream())
      {
        file.CopyTo(data);
        byteArray = data.ToArray();
      }
      
      // Upload file
      var response = await FilesApi.UploadFileAsync(
          accessToken,
          xeroTenantId,
          null,
          byteArray,
          file.FileName,
          file.FileName,
          file.ContentType
      );

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
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();      
      
      Guid fileIDGuid = Guid.Parse(fileID);

      var filesApi = new FilesApi();
      FileObject file = await filesApi.GetFileAsync(accessToken, xeroTenantId, fileIDGuid);
      file.Name = newName;

      var response = await filesApi.UpdateFileAsync(accessToken, xeroTenantId, fileIDGuid, file);

      return RedirectToAction("Index", "FilesSync");
    }

  }
}
