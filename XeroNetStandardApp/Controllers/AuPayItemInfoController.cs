using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;

namespace XeroNetStandardApp.Controllers
{
    public class AuPayItemInfoController : Controller
    {
        private readonly ILogger<AuPayItemInfoController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public AuPayItemInfoController(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuPayItemInfoController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }

        // GET: /AuPayItem/
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

            var PayrollAUApi = new PayrollAuApi();
            var response = await PayrollAUApi.GetPayItemsAsync(accessToken, xeroTenantId);
            
            // Extracts the name from the different pay item types
            var earnings = response._PayItems.EarningsRates
              .Select(x => x.Name)
              .ToList();
            var leave = response._PayItems.LeaveTypes
              .Select(x => x.Name)
              .ToList();
            var reimbursements = response._PayItems.ReimbursementTypes
              .Select(x => x.Name)
              .ToList();
            var payItemList = response._PayItems.DeductionTypes
              .Select(x => x.Name)
              .ToArray()
              .Concat(earnings)
              .Concat(leave)
              .Concat(reimbursements);

            // Sends the Pay item information to View
            ViewBag.jsonResponse = response.ToJson();
            return View(payItemList);

        }
    }
}