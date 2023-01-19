using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
    /// <para>- POST: /FilesSync#Modify</para>
    /// </summary>
    public class FilesSync : ApiAccessorController<FilesApi>
    {
        public FilesSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /FilesSync/
        /// </summary>
        /// <returns>Returns a list of files</returns>
        public async Task<IActionResult> Index()
        {
            // Call get files endpoint
            var response = await Api.GetFilesAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }

        /// <summary>
        /// Get: /Files#Delete
        /// </summary>
        /// <param name="fileId">File id of file to delete</param>
        /// <returns>Returns action result to redirect user to get files page</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(string fileId)
        {
            // Call delete file endpoint
            await Api.DeleteFileAsync(XeroToken.AccessToken, TenantId, Guid.Parse(fileId));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// GET: /FilesSync#Modify
        /// </summary>
        /// <param name="fileId">File id of file to modify</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Modify(Guid fileId)
        {
            var file = await Api.GetFileAsync(XeroToken.AccessToken, TenantId, fileId);
            return View(file);
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

                    await Api.UploadFileAsync(
                        XeroToken.AccessToken,
                        TenantId,
                        byteArray,
                        formFile.FileName,
                        formFile.FileName,
                        formFile.ContentType
                    );
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /FilesSync#Modify
        /// </summary>
        /// <param name="fileId">File id of file to rename</param>
        /// <param name="name">New name value for file</param>
        /// <returns>Returns action result redirecting user to get files page</returns>
        [HttpPost]
        public async Task<IActionResult> Modify(Guid fileId, string name)
        {
            // Update file object
            var file = await Api.GetFileAsync(XeroToken.AccessToken, TenantId, fileId);
            file.Name = name;

            // Call update file endpoint
            await Api.UpdateFileAsync(XeroToken.AccessToken, TenantId, fileId, file);

            return RedirectToAction("Index");
        }

        #endregion

    }
}
