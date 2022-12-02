using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller supporting get organisations endpoint 
    /// </summary>
    public class OrganisationInfo : ApiAccessorController<AccountingApi>
    {
        public OrganisationInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        /// <summary>
        /// GET: /Organisation/
        /// </summary>
        /// <returns>Returns first organisation object associated with account</returns>
        public async Task<ActionResult> Index()
        {
            // Call get organisation endpoint
            var response = await Api.GetOrganisationsAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._Organisations[0]);
        }
    }
}