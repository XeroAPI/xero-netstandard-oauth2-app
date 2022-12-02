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
    public class FoldersSync : ApiAccessorController<FilesApi>
    {
        public FoldersSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /FoldersSync/
        /// </summary>
        /// <returns>Returns a list of folders</returns>
        public async Task<ActionResult> Index()
        {
            // Call get folders endpoint
            var response = await Api.GetFoldersAsync(XeroToken.AccessToken, TenantId);

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
            // Call delete folder endpoint
            await Api.DeleteFolderAsync(XeroToken.AccessToken, TenantId, Guid.Parse(folderId));
            
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
            var newFolder = new Folder
            {
                Name = name,
                Email = email,
                Id = Guid.NewGuid()
            };

            // Call create folder endpoint
            await Api.CreateFolderAsync(XeroToken.AccessToken, TenantId, newFolder);

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
            // Rename folder
            var folder = await Api.GetFolderAsync(XeroToken.AccessToken, TenantId, Guid.Parse(folderId));
            folder.Name = newName;

            // Call update folder endpoint
            await Api.UpdateFolderAsync(XeroToken.AccessToken, TenantId, Guid.Parse(folderId), folder);

            return RedirectToAction("Index", "FoldersSync");
        }

        #endregion
    }
}
