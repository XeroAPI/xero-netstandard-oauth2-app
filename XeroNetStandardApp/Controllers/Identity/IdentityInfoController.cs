using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Identity endpoints:
    /// <para>- GET: /IdentityInfo/</para>
    /// <para>- GET: /Contacts#Delete</para>
    /// </summary>
    public class IdentityInfo : ApiAccessorController<IdentityApi>
    {
        public IdentityInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        /// <summary>
        /// GET: /IdentityInfo/
        /// </summary>
        /// <returns>Returns a list of connections</returns>
        public async Task<IActionResult> Index()
        {
            // Call get connections endpoint
            var response = await Api.GetConnectionsAsync(XeroToken.AccessToken);

            response.ForEach(connection => ViewBag.jsonResponse += connection.ToJson());
            return View(response);
        }

        /// <summary>
        /// GET: /Contacts#Delete
        /// </summary>
        /// <param name="connectionId">Id of connection to delete</param>
        /// <returns>Returns action result to redirect user to get connections page</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(string connectionId)
        {
            // Call delete connection endpoint
            await Api.DeleteConnectionAsync(XeroToken.AccessToken, Guid.Parse(connectionId));

            return RedirectToAction("Index", "IdentityInfo");
        }
    }
}