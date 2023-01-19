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
    public class CashValidationSync : ApiAccessorController<FinanceApi>
    {
        public CashValidationSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        /// <summary>
        /// GET: /CashValidationSync/
        /// </summary>
        /// <returns>Returns a list of cash validations</returns>
        public async Task<IActionResult> Index()
        {
            // Call get cash validation endpoint
            var response = await Api.GetCashValidationAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return View(response);
        }
    }
}