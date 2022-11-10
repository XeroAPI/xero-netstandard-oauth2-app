using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Finance endpoints:
    /// <para>- GET: /CashValidationSync/</para>
    /// </summary>
    public class CashValidationSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FinanceApi _financeApi;

        public CashValidationSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _financeApi = new FinanceApi();
        }

        /// <summary>
        /// GET: /CashValidationSync/
        /// </summary>
        /// <returns>Returns a list of cash validations</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get cash validation endpoint
            var response = await _financeApi.GetCashValidationAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = JsonConvert.SerializeObject(response);
            return View(response);
        }
    }
}