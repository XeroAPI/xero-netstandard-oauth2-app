using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;


namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller supporting get organisations endpoint 
    /// </summary>
    public class OrganisationInfo : Controller
    {

        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public OrganisationInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _accountingApi = new AccountingApi();
        }

        /// <summary>
        /// GET: /Organisation/
        /// </summary>
        /// <returns>Returns first organisation object associated with account</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get organisation endpoint
            var response = await _accountingApi.GetOrganisationsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._Organisations[0]);
        }
    }
}