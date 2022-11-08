using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Identity;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace XeroNetStandardApp.Controllers
{
    public class IdentityInfo : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;

        public IdentityInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
        }

        // GET: /IdentityInfo/
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

            var IdentityApi = new IdentityApi();
            var response = await IdentityApi.GetConnectionsAsync(accessToken);

            foreach (var connection in response)
            {
                ViewBag.jsonResponse += connection.ToJson();
            }

            var connections = response;

            return View(connections);
        }

        // GET: /Contacts#Delete
        [HttpGet]
        public async Task<ActionResult> Delete(string connectionId)
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

            Guid connectionIdGuid = Guid.Parse(connectionId);

            var IdentityApi = new IdentityApi();
            await IdentityApi.DeleteConnectionAsync(accessToken, connectionIdGuid);

            return RedirectToAction("Index", "IdentityInfo");
        }
    }
}