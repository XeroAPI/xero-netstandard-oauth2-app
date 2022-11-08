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

namespace XeroNetStandardApp.Controllers.Files
{
    /// <summary>
    /// Controller implementing methods demonstrating following file endpoints:
    /// <para>- GET: /FilesSync/</para>
    /// <para>- GET: /Associations#Get</para>
    /// <para>- GET: /AssociationsSync#Create</para>
    /// <para>- GET: /AssociationsSync#Delete</para>
    /// <para>- POST: /Associations#Create</para>
    /// </summary>
    public class AssociationsSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FilesApi _filesApi;

        public AssociationsSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _filesApi = new FilesApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /FilesSync/
        /// </summary>
        /// <returns>Returns a list of files</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get files endpoint
            var response = await _filesApi.GetFilesAsync(xeroToken.AccessToken, xeroTenantId);
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
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            Console.WriteLine(fileId);
            Console.WriteLine(new Guid(fileId).ToString());

            // Call get file associations endpoint
            var response = await _filesApi.GetFileAssociationsAsync(xeroToken.AccessToken, xeroTenantId, new Guid(fileId));
            return View(response);
        }


        /// <summary>
        /// GET: /AssociationsSync#Create
        /// </summary>
        /// <returns>View for create file association page</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get files endpoint
            var files = await _filesApi.GetFilesAsync(xeroToken.AccessToken, xeroTenantId);

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
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call delete file association endpoint
            await _filesApi.DeleteFileAssociationAsync(xeroToken.AccessToken, xeroTenantId, new Guid(fileId), new Guid(objectId));
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
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

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
            await _filesApi.CreateFileAssociationAsync(xeroToken.AccessToken, xeroTenantId, fileIdGuid, association);
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
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            var accountingApi = new AccountingApi();
            var invoices = await accountingApi.GetInvoicesAsync(xeroToken.AccessToken, xeroTenantId);
            return invoices._Invoices.Select(invoice => invoice.InvoiceID.ToString());
        }
        #endregion
    }
}
