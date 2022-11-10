using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Model.Files;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Files endpoints:
    /// <para>- GET: /FoldersSync/</para>
    /// <para>- Get: /FoldersSync#Delete</para>
    /// <para>- GET: /FoldersSync#Modify</para>
    /// <para>- POST: /FoldersSync#Create</para>
    /// <para>- Post: /FoldersSync#Rename</para>
    /// </summary>
    public class FoldersSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FilesApi _filesApi;

        public FoldersSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _filesApi = new FilesApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /FoldersSync/
        /// </summary>
        /// <returns>Returns a list of folders</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get folders endpoint
            var response = await _filesApi.GetFoldersAsync(xeroToken.AccessToken, xeroTenantId);

            var formattedResponse = "";
            response.ForEach(folder => formattedResponse += folder.ToJson() + "\n");

            ViewBag.jsonResponse = formattedResponse;
            return View(response);
        }

        /// <summary>
        /// Get: /FoldersSync#Delete
        /// </summary>
        /// <param name="folderId">Id of folder to delete</param>
        /// <returns>Returns an action result to redirect user to get folders page</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(string folderId)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call delete folder endpoint
            await _filesApi.DeleteFolderAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(folderId));
            
            return RedirectToAction("Index", "FoldersSync");
        }

        /// <summary>
        /// GET: /FoldersSync#Modify
        /// </summary>
        /// <param name="folderId">Folder id of folder to modify</param>
        /// <param name="folderName">Folder name of folder to modify</param>
        /// <returns></returns>
        [HttpGet("/FoldersSync/{folderId}")]
        public IActionResult Modify(string folderId, string folderName)
        {
            ViewBag.folderId = folderId;
            ViewBag.folderName = folderName;
            return View();
        }

        /// <summary>
        /// GET: /Folders#Create
        /// <para>Helper method to return a view for the create folder page</para>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /FoldersSync#Create
        /// </summary>
        /// <param name="name">Name of folder to create</param>
        /// <param name="email">Email associated with folder to create</param>
        /// <returns>Returns an action result to redirect user to get folders page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string name, string email)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            var newFolder = new Folder
            {
                Name = name,
                Email = email,
                Id = Guid.NewGuid()
            };

            // Call create folder endpoint
            await _filesApi.CreateFolderAsync(xeroToken.AccessToken, xeroTenantId, newFolder);

            return RedirectToAction("Index", "FoldersSync");
        }

        /// <summary>
        /// Post: /FoldersSync#Rename
        /// </summary>
        /// <param name="folderId">Folder id of folder to rename</param>
        /// <param name="newName">New name for folder</param>
        /// <returns>Returns an action result to redirect user to get folders page</returns>
        [HttpPost]
        public async Task<ActionResult> Rename(string folderId, string newName)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Rename folder
            var folder = await _filesApi.GetFolderAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(folderId));
            folder.Name = newName;

            // Call update folder endpoint
            await _filesApi.UpdateFolderAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(folderId), folder);

            return RedirectToAction("Index", "FoldersSync");
        }

        #endregion
    }
}
