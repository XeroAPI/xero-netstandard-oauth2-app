using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Files;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Files endpoints:
    /// <para>- GET: /FilesSync/</para>
    /// <para>- POST: /FilesSync/FileUpload#Upload</para>
    /// <para>- POST: /FilesSync#Rename</para>
    /// </summary>
    public class FilesSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FilesApi _filesApi;

        public FilesSync(IOptions<XeroConfiguration> xeroConfig)
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

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }

        /// <summary>
        /// Get: /Files#Delete
        /// </summary>
        /// <param name="fileID">File id of file to delete</param>
        /// <returns>Returns action result to redirect user to get files page</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(string fileID)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call delete file endpoint
            await _filesApi.DeleteFileAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(fileID));

            return RedirectToAction("Index", "FilesSync");
        }

        /// <summary>
        /// GET: /FilesSync#Modify
        /// </summary>
        /// <param name="fileId">File id of file to modify</param>
        /// <param name="fileName">File name of file to modify</param>
        /// <returns></returns>
        [HttpGet("/FilesSync/{fileId}")]
        public IActionResult Modify(string fileId, string fileName)
        {
            ViewBag.fileID = fileId;
            ViewBag.fileName = fileName;
            return View();
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /FilesSync/FileUpload#Upload
        /// </summary>
        /// <param name="files">Files to add to account</param>
        /// <returns>Returns action result to redirect user to get files page</returns>
        [HttpPost("FilesSyncFileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Loop through all files and upload each one to account
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    byte[] byteArray;
                    using (var ms = new MemoryStream())
                    {
                        await formFile.CopyToAsync(ms);
                        byteArray = ms.ToArray();
                    }

                    await _filesApi.UploadFileAsync(
                        xeroToken.AccessToken,
                        xeroTenantId,
                        byteArray,
                        formFile.FileName,
                        formFile.FileName,
                        formFile.ContentType
                    );
                }
            }

            return RedirectToAction("Index", "FilesSync");
        }

        /// <summary>
        /// POST: /FilesSync#Rename
        /// </summary>
        /// <param name="fileID">File id of file to rename</param>
        /// <param name="newName">New name value for file</param>
        /// <returns>Returns action result redirecting user to get files page</returns>
        [HttpPost]
        public async Task<ActionResult> Rename(string fileID, string newName)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Update file object
            FileObject file = await _filesApi.GetFileAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(fileID));
            file.Name = newName;

            // Call update file endpoint
            await _filesApi.UpdateFileAsync(xeroToken.AccessToken, xeroTenantId, Guid.Parse(fileID), file);

            return RedirectToAction("Index", "FilesSync");
        }

        #endregion

    }
}
