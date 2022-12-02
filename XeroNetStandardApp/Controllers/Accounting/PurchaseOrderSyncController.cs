using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Http;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following account endpoints:
    /// <para>- GET: /PurchaseOrderSync/</para>
    /// <para>- POST: /PurchaseOrderSync#Create</para>
    /// <para>- POST: /PurchaseOrderSync/FileUpload#Upload</para>
    /// </summary>
    public class PurchaseOrderSync : ApiAccessorController<AccountingApi>
    {
        public PurchaseOrderSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /PurchaseOrderSync/
        /// </summary>
        /// <returns>Returns a list of purchase orders</returns>
        public async Task<ActionResult> Index()
        {
            // Call get purchase orders endpoint
            var response = await Api.GetPurchaseOrdersAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._PurchaseOrders);
        }

        /// <summary>
        /// GET: /PurchaseOrderSync#Create
        /// <para>Helper method to populate create page for purchase orders</para>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var contacts = await Api.GetContactsAsync(XeroToken.AccessToken, TenantId);
            return View(contacts._Contacts.Select(contact => contact.ContactID.ToString()));
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /PurchaseOrderSync#Create
        /// </summary>
        /// <param name="contactId">Contact id of contact in purchase order to create</param>
        /// <param name="lineDescription">Line description of line item in purchase order to create</param>
        /// <param name="lineQuantity">Line quantity of line item in purchase order to create</param>
        /// <param name="lineUnitAmount">Line unit amount of line item in purchase order to create</param>
        /// <param name="lineAccountCode">Line account code of line item in purchase order to create</param>
        /// <returns>Return action result to redirect user to get purchase orders page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string contactId, string lineDescription, string lineQuantity, string lineUnitAmount, string lineAccountCode)
        {
            // Construct purchase orders object
            var lines = new List<LineItem>
            {
                new LineItem
                {
                    Description = lineDescription,
                    Quantity = decimal.Parse(lineQuantity),
                    UnitAmount = decimal.Parse(lineUnitAmount),
                    AccountCode = lineAccountCode
                }
            };

            var purchaseOrder = new PurchaseOrder
            {
                Contact = new Contact { ContactID = Guid.Parse(contactId) },
                Date = DateTime.Today,
                DeliveryDate = DateTime.Today.AddDays(30),
                LineAmountTypes = LineAmountTypes.Exclusive,
                LineItems = lines
            };

            var purchaseOrders = new PurchaseOrders
            {
                _PurchaseOrders = new List<PurchaseOrder> { purchaseOrder }
            };

            // Call create purchase order endpoint
            await Api.CreatePurchaseOrdersAsync(XeroToken.AccessToken, TenantId, purchaseOrders);

            return RedirectToAction("Index", "PurchaseOrderSync");
        }


        /// <summary>
        /// POST: /PurchaseOrderSync/FileUpload#Upload
        /// </summary>
        /// <param name="files"></param>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpPost("PurchaseOrderSyncFileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files, string purchaseOrderId)
        {
            // Read files and attach them to a new purchase order
            long size = files.Sum(f => f.Length);
            var filePaths = new List<string>();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();
                    filePaths.Add(filePath);

                    byte[] byteArray;
                    using (var ms = new MemoryStream())
                    {
                        await formFile.CopyToAsync(ms);
                        byteArray = ms.ToArray();
                    }

                    await Api.CreatePurchaseOrderAttachmentByFileNameAsync(XeroToken.AccessToken, TenantId, Guid.Parse(purchaseOrderId), formFile.FileName, byteArray);
                }
            }
            
            return Ok(new { count = files.Count, size, filePaths });
        }

        #endregion

    }
}