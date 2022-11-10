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
    public class IdentityInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly IdentityApi _identityApi;

        public IdentityInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _identityApi = new IdentityApi();
        }

        /// <summary>
        /// GET: /IdentityInfo/
        /// </summary>
        /// <returns>Returns a list of connections</returns>
        public async Task<ActionResult> Index()
        {
            // Token setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            
            // Call get connections endpoint
            var response = await _identityApi.GetConnectionsAsync(xeroToken.AccessToken);

            response.ForEach(connection => ViewBag.jsonResponse += connection.ToJson());
            return View(response);
        }

        /// <summary>
        /// GET: /Contacts#Delete
        /// </summary>
        /// <param name="connectionId">Id of connection to delete</param>
        /// <returns>Returns action result to redirect user to get connections page</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(string connectionId)
        {
            // Token setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);

            // Call delete connection endpoint
            await _identityApi.DeleteConnectionAsync(xeroToken.AccessToken, Guid.Parse(connectionId));

            return RedirectToAction("Index", "IdentityInfo");
        }
    }
}