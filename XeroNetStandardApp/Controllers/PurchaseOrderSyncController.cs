using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;


namespace XeroNetStandardApp.Controllers
{
    public class PurchaseOrderSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public PurchaseOrderSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
      this.httpClientFactory = httpClientFactory;
    }

    // GET: /PurchaseOrderSync/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AccountingApi = new AccountingApi();

      var response = await AccountingApi.GetPurchaseOrdersAsync(accessToken, xeroTenantId);
      var purchaseOrders = response._PurchaseOrders;
      ViewBag.jsonResponse = response.ToJson();

      return View(purchaseOrders);
    }

    // GET: /PurchaseOrderSync#Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var accountingApi = new AccountingApi();
      var contacts = await accountingApi.GetContactsAsync(accessToken, xeroTenantId);
      return View(contacts._Contacts.Select(contact => contact.ContactID.ToString()));
    }

    // POST: /PurchaseOrderSync#Create
    [HttpPost]
    public async Task<ActionResult> Create(string contactId, string LineDescription, string LineQuantity, string LineUnitAmount, string LineAccountCode)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var contact = new Contact();
      contact.ContactID = Guid.Parse(contactId);
      
      var line = new LineItem() {
        Description = LineDescription,
        Quantity = decimal.Parse(LineQuantity),
        UnitAmount = decimal.Parse(LineUnitAmount),
        AccountCode = LineAccountCode
      };

      var lines = new List<LineItem>() {
        line
      };

      var purchaseOrder = new PurchaseOrder() {
        Contact = contact,
        Date = DateTime.Today,
        DeliveryDate = DateTime.Today.AddDays(30),
        LineAmountTypes = LineAmountTypes.Exclusive,
        LineItems = lines
      };

      var purchaseOrderList = new List<PurchaseOrder>();
      purchaseOrderList.Add(purchaseOrder);

      var purchaseOrders = new PurchaseOrders();
      purchaseOrders._PurchaseOrders = purchaseOrderList;

      var objectFullName = purchaseOrders.GetType().FullName;
      String result = JsonConvert.SerializeObject(purchaseOrders);

      var AccountingApi = new AccountingApi();
      var response = await AccountingApi.CreatePurchaseOrdersAsync(accessToken, xeroTenantId, purchaseOrders);
      
      var updatedUTC = response._PurchaseOrders[0].UpdatedDateUTC;

      return RedirectToAction("Index", "PurchaseOrderSync");
    }


    // POST: /PurchaseOrderSync/FileUpload#Upload
    [HttpPost("PurchaseOrderSyncFileUpload")]
    public async Task<IActionResult> Upload(List<IFormFile> files, string purchaseOrderId)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var invoiceID = Guid.Parse(purchaseOrderId);
      long size = files.Sum(f => f.Length);
                      
      var filePaths = new List<string>();
      foreach (var formFile in files)
      {
          if (formFile.Length > 0)
          {
            // full path to file in temp location
            var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
            filePaths.Add(filePath);

            byte [] byteArray; 

            // using (var stream = new FileStream(filePath, FileMode.Create))
            // {
            //     await formFile.CopyToAsync(stream);
            // }

            using (MemoryStream data = new MemoryStream())
            {
              formFile.CopyTo(data);
              byteArray = data.ToArray();
            }

            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.CreatePurchaseOrderAttachmentByFileNameAsync(accessToken, xeroTenantId, invoiceID, formFile.FileName, byteArray);
          }
      }



      // process uploaded files
      // Don't rely on or trust the FileName property without validation.

      return Ok(new { count = files.Count, size, filePaths });
    }
  }
}