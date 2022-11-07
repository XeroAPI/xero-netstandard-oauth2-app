using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller supporting get accounts endpoint
    /// </summary>
    public class AccountsInfoController : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public AccountsInfoController(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _accountingApi = new AccountingApi();
        }

        /// <summary>
        /// GET: /GetAccounts
        /// </summary>
        /// <returns>Returns list of accounts</returns>
        public async Task<IActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            var response = await _accountingApi.GetAccountsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();

            // Limit response to max of 10 accounts in display
            return View(response._Accounts.Take(10));
        }
    }
}
