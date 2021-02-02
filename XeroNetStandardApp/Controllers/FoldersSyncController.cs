using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;
using Xero.NetStandard.OAuth2.Model.Files;

namespace XeroNetStandardApp.Controllers
{
  public class FoldersSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public FoldersSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
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
      var response = await FilesApi.GetFoldersAsync(accessToken, xeroTenantId);

      var jsonString = "";
      foreach(Folder folder in response)
      {
        jsonString += folder.ToJson();
      }

      ViewBag.jsonResponse = jsonString;


      return View(response);
    }


    // GET: /Folders#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /FoldersSync#Create
    [HttpPost]
    public async Task<ActionResult> Create(string name, string email)
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

      Folder folder = new Folder{
        Name = name,
        Email = email,
        Id = Guid.NewGuid()
      };

      await FilesApi.CreateFolderAsync(accessToken, xeroTenantId, folder);

      return RedirectToAction("Index", "FoldersSync");
    }

    // Get: /FoldersSync#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string FolderId)
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
      
      Guid folderIdGuid = Guid.Parse(FolderId);

      var filesApi = new FilesApi();

      await filesApi.DeleteFolderAsync(accessToken, xeroTenantId, folderIdGuid);
      return RedirectToAction("Index", "FoldersSync");
    }


    // GET: /FoldersSync#Modify
    [HttpGet("/FoldersSync/{folderId}")]
    public IActionResult Modify(string folderId, string folderName)
    {
      ViewBag.folderId = folderId;
      ViewBag.folderName = folderName;
      return View();
    }

    // Put: /FoldersSync#Rename
    [HttpPost]
    public async Task<ActionResult> Rename(string folderId, string newName)
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
      
      Guid folderIdGuid = Guid.Parse(folderId);

      var filesApi = new FilesApi();
      Folder folder = await filesApi.GetFolderAsync(accessToken, xeroTenantId, folderIdGuid);
      folder.Name = newName;

      var response = await filesApi.UpdateFolderAsync(accessToken, xeroTenantId, folderIdGuid, folder);

      return RedirectToAction("Index", "FoldersSync");
    }


  }
}
