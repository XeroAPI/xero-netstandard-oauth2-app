using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


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
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

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
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }


            var accountingApi = new AccountingApi();
            var contacts = await accountingApi.GetContactsAsync(accessToken, xeroTenantId);
            return View(contacts._Contacts.Select(contact => contact.ContactID.ToString()));
        }

        // POST: /PurchaseOrderSync#Create
        [HttpPost]
        public async Task<ActionResult> Create(string contactId, string LineDescription, string LineQuantity, string LineUnitAmount, string LineAccountCode)
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
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

            var contact = new Contact();
            contact.ContactID = Guid.Parse(contactId);

            var line = new LineItem()
            {
                Description = LineDescription,
                Quantity = decimal.Parse(LineQuantity),
                UnitAmount = decimal.Parse(LineUnitAmount),
                AccountCode = LineAccountCode
            };

            var lines = new List<LineItem>() {
        line
      };

            var purchaseOrder = new PurchaseOrder()
            {
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
            string result = JsonConvert.SerializeObject(purchaseOrders);

            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.CreatePurchaseOrdersAsync(accessToken, xeroTenantId, purchaseOrders);

            var updatedUTC = response._PurchaseOrders[0].UpdatedDateUTC;

            return RedirectToAction("Index", "PurchaseOrderSync");
        }


        // POST: /PurchaseOrderSync/FileUpload#Upload
        [HttpPost("PurchaseOrderSyncFileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files, string purchaseOrderId)
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
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

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

                    byte[] byteArray;

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