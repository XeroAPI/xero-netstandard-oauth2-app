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
    public class InvoiceSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public InvoiceSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
      this.httpClientFactory = httpClientFactory;
    }

    // GET: /InvoiceSync/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AccountingApi = new AccountingApi();

      var sevenDaysAgo = DateTime.Now.AddDays(-7).ToString("yyyy, MM, dd");
      var invoicesFilter = "Date >= DateTime(" + sevenDaysAgo + ")";

      var response = await AccountingApi.GetInvoicesAsync(accessToken, xeroTenantId, null, invoicesFilter);
      var invoices = response._Invoices;
      ViewBag.jsonResponse = response.ToJson();
      return View(invoices);
    }

    // GET: /InvoiceSync#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /InvoiceSync#Create
    [HttpPost]
    public async Task<ActionResult> Create(string Name, string LineDescription, string LineQuantity, string LineUnitAmount, string LineAccountCode)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var contact = new Contact();
      contact.Name = Name;
      
      var line = new LineItem() {
        Description = LineDescription,
        Quantity = decimal.Parse(LineQuantity),
        UnitAmount = decimal.Parse(LineUnitAmount),
        AccountCode = LineAccountCode
      };

      var lines = new List<LineItem>() {
        line
      };

      var invoice = new Invoice() {
        Type = Invoice.TypeEnum.ACCREC,
        Contact = contact,
        Date = DateTime.Today,
        DueDate = DateTime.Today.AddDays(30),
        LineItems = lines
      };

      var invoiceList = new List<Invoice>();
      invoiceList.Add(invoice);

      var invoices = new Invoices();
      invoices._Invoices = invoiceList;

      var objectFullName = invoices.GetType().FullName;
      String result = JsonConvert.SerializeObject(invoices);

      var AccountingApi = new AccountingApi();
      var response = await AccountingApi.CreateInvoicesAsync(accessToken, xeroTenantId, invoices);
      
      var updatedUTC = response._Invoices[0].UpdatedDateUTC;

      return RedirectToAction("Index", "InvoiceSync");
    }


    // POST: /InvoiceSync/FileUpload#Upload

    [HttpPost("InvoiceFileUpload")]
    public async Task<IActionResult> Upload(List<IFormFile> files, string invoiceId)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var invoiceID = Guid.Parse(invoiceId);
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
            var response = await AccountingApi.CreateInvoiceAttachmentByFileNameAsync(accessToken, xeroTenantId, invoiceID, formFile.FileName, byteArray);
          }
      }

      // process uploaded files
      // Don't rely on or trust the FileName property without validation.

      return Ok(new { count = files.Count, size, filePaths });
    }
  }
}