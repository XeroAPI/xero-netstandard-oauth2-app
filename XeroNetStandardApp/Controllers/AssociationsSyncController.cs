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
using Microsoft.AspNetCore.Routing;


namespace XeroNetStandardApp.Controllers
{
  public class AssociationsSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public AssociationsSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
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

      ViewBag.jsonResponse = response.ToJson();

      return View(filesItems);
    }

    // GET: /Associations#Get
    [HttpGet("/AssociationsSync/{fileId}")]
    public async Task<IActionResult> LoadAssociations(string fileId)
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
      var response = await FilesApi.GetFileAssociationsAsync(accessToken, xeroTenantId, new Guid(fileId));
      return View(response);
    }



   // GET: /AssociationsSync#Create
    [HttpGet]
    public async Task<IActionResult> Create()
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
      var files = await FilesApi.GetFilesAsync(accessToken, xeroTenantId);

      var AccountingApi = new AccountingApi();
      var invoices = await AccountingApi.GetInvoicesAsync(accessToken, xeroTenantId);
      var invoiceIds = invoices._Invoices.Select(invoice => invoice.InvoiceID.ToString());

      ViewBag.invoiceIds = invoiceIds;

      return View(files.Items.Select( item => item.Id.ToString()));
    }

    // POST: /Associations#Create
    [HttpPost]
    public async Task<ActionResult> Create(string fileId, string invoiceId, string objectType)
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

      var fileIdGuid = new Guid(fileId);
      var invoiceIdGuid = new Guid(invoiceId);
      Enum.TryParse<ObjectType>(objectType, out var objectTypeEnum);

      Association association = new Association{
          FileId = fileIdGuid,
          ObjectId = invoiceIdGuid,
          ObjectType = objectTypeEnum,
          ObjectGroup = ObjectGroup.Invoice
      };

      var response = await FilesApi.CreateFileAssociationAsync(accessToken, xeroTenantId, fileIdGuid, association);

      return RedirectToAction("LoadAssociations", new RouteValueDictionary(
        new {
          controller = "AssociationsSync", 
          action = "LoadAssociations",
          fileId = fileId
        }
      ));
    }

    // Get: /AssociationsSync#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string fileId, string objectId)
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
      
      var fileIdGuid = new Guid(fileId);
      var objectIdGuid = new Guid(objectId);

      var FilesApi = new FilesApi();
      await FilesApi.DeleteFileAssociationAsync(accessToken, xeroTenantId, fileIdGuid, objectIdGuid);

      return RedirectToAction("LoadAssociations", new RouteValueDictionary(
        new {
          controller = "AssociationsSync", 
          action = "LoadAssociations",
          fileId = fileId
        }
      ));
    }

  }
}
