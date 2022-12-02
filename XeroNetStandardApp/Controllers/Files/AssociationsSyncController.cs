using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Files;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.AspNetCore.Routing;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following file endpoints:
    /// <para>- GET: /FilesSync/</para>
    /// <para>- GET: /Associations#Get</para>
    /// <para>- GET: /AssociationsSync#Create</para>
    /// <para>- GET: /AssociationsSync#Delete</para>
    /// <para>- POST: /Associations#Create</para>
    /// </summary>
    public class AssociationsSync : ApiAccessorController<FilesApi>
    {
       
        public AssociationsSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /FilesSync/
        /// </summary>
        /// <returns>Returns a list of files</returns>
        public async Task<ActionResult> Index()
        {
            // Call get files endpoint
            var response = await Api.GetFilesAsync(XeroToken.AccessToken, TenantId);
            var filesItems = response.Items;

            ViewBag.jsonResponse = response.ToJson();
            return View(filesItems);
        }

        /// <summary>
        /// GET: /Associations#Get
        /// </summary>
        /// <param name="fileId">Specific file id to get file association info on</param>
        /// <returns>Returns file association information for a specific file</returns>
        [HttpGet("/AssociationsSync/{fileId}")]
        public async Task<IActionResult> LoadAssociations(string fileId)
        {
            // Call get file associations endpoint
            var response = await Api.GetFileAssociationsAsync(XeroToken.AccessToken, TenantId, new Guid(fileId));
            return View(response);
        }


        /// <summary>
        /// GET: /AssociationsSync#Create
        /// </summary>
        /// <returns>View for create file association page</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Call get files endpoint
            var files = await Api.GetFilesAsync(XeroToken.AccessToken, TenantId);

            ViewBag.invoiceIds = await GetInvoiceIds();
            return View(files.Items.Select(item => item.Id.ToString()));
        }

        /// <summary>
        /// GET: /AssociationsSync#Delete
        /// </summary>
        /// <param name="fileId">file id in file association to delete</param>
        /// <param name="objectId">object id in file association to delete</param>
        /// <returns>Returns action result to redirect user to load associations page</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(string fileId, string objectId)
        {
            // Call delete file association endpoint
            await Api.DeleteFileAssociationAsync(XeroToken.AccessToken, TenantId, new Guid(fileId), new Guid(objectId));
            return RedirectToAction("LoadAssociations", new RouteValueDictionary(new { fileId }));
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /Associations#Create
        /// </summary>
        /// <param name="fileId">File id to create file association with</param>
        /// <param name="invoiceId">Invoice id to create file association with</param>
        /// <param name="objectType">Type of object file association is</param>
        /// <returns>Returns action result to redirect user to load associations page for newly created association</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string fileId, string invoiceId, string objectType)
        {
            // Construct association object
            var fileIdGuid = new Guid(fileId);
            var invoiceIdGuid = new Guid(invoiceId);
            Enum.TryParse<ObjectType>(objectType, out var objectTypeEnum);

            Association association = new Association
            {
                FileId = fileIdGuid,
                ObjectId = invoiceIdGuid,
                ObjectType = objectTypeEnum,
                ObjectGroup = ObjectGroup.Invoice
            };

            // Call create file association endpoint
            await Api.CreateFileAssociationAsync(XeroToken.AccessToken, TenantId, fileIdGuid, association);
            return RedirectToAction("LoadAssociations", new RouteValueDictionary(new { fileId }));
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper method to call accounting api to retrieve list of invoice ids
        /// </summary>
        /// <returns>Returns a list of invoice ids</returns>
        private async Task<IEnumerable<string>> GetInvoiceIds()
        {
            var accountingApi = new AccountingApi();
            var invoices = await accountingApi.GetInvoicesAsync(XeroToken.AccessToken, TenantId);
            return invoices._Invoices.Select(invoice => invoice.InvoiceID.ToString());
        }
        #endregion
    }
}
