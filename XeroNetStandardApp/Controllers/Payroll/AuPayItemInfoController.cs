using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following PayrollAU endpoints:
    /// <para>- GET: /AuPayItem/</para>
    /// </summary>
    public class AuPayItemInfoController : ApiAccessorController<PayrollAuApi>
    {
        public AuPayItemInfoController(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        /// <summary>
        /// GET: /AuPayItem/
        /// </summary>
        /// <returns>Returns a list of pay items</returns>
        public async Task<ActionResult> Index()
        {
            // Call get pay items async
            var response = await Api.GetPayItemsAsync(XeroToken.AccessToken, TenantId);

            // Extracts the name from the different pay item types
            var earnings = response?._PayItems.EarningsRates
              .Select(x => x.Name)
              .ToList();
            var leave = response?._PayItems.LeaveTypes
              .Select(x => x.Name)
              .ToList();
            var reimbursements = response?._PayItems.ReimbursementTypes
              .Select(x => x.Name)
              .ToList();
            var payItemList = response?._PayItems.DeductionTypes
              .Select(x => x.Name)
              .ToArray()
              .Concat(earnings)
              .Concat(leave)
              .Concat(reimbursements);

            ViewBag.jsonResponse = response?.ToJson();
            return View(payItemList);

        }
    }
}