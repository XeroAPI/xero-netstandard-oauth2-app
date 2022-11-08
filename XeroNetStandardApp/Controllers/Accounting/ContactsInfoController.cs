using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace XeroNetStandardApp.Controllers
{
    public class ContactsInfo : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;

        public ContactsInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
        }

        // GET: /Contacts/
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
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.GetContactsAsync(accessToken, xeroTenantId);
            ViewBag.jsonResponse = response.ToJson();

            var contacts = response._Contacts;

            return View(contacts);
        }

        // GET: /Contacts#Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Contacts#Create
        [HttpPost]
        public async Task<ActionResult> Create(string name, string emailAddress)
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
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

            var contact = new Contact
            {
                Name = name,
                EmailAddress = emailAddress
            };

            var contacts = new Contacts();
            contacts._Contacts = new List<Contact>() { contact };

            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.CreateContactsAsync(accessToken, xeroTenantId, contacts);

            return RedirectToAction("Index", "ContactsInfo");
        }
    }
}