using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /BankTransactionsInfo/</para>
    /// </summary>
    public class BankTransactionsInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public BankTransactionsInfo(IOptions<XeroConfiguration> xeroConfig)
        {
           _xeroConfig = xeroConfig;
           _accountingApi = new AccountingApi();
        }

        /// <summary>
        /// GET: /BankTransactionsInfo/
        /// </summary>
        /// <returns>Returns list of bank transactions</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get bank transaction endpoint
            var response = await _accountingApi.GetBankTransactionsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._BankTransactions);
        }

    }
}