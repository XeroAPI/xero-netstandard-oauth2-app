using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;

namespace XeroNetStandardApp.Controllers
{
    public class AccountsInfoController : Controller
    {

        private readonly ILogger<AccountsInfoController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;

        public AccountsInfoController(IOptions<XeroConfiguration> XeroConfig, ILogger<AccountsInfoController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
        }
        public async Task<IActionResult> Index()
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

            var currencyResponse = await AccountingApi.GetCurrenciesAsync(accessToken, xeroTenantId);
            
            var response = await AccountingApi.GetAccountsAsync(accessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();

            return View(response._Accounts);
        }
    }
}
