using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following finance endpoints:
    /// <para>- GET: /AccountingUsage</para>
    /// <para>- GET: /LockHistory</para>
    /// <para>- GET: /ReportHistory</para>
    /// <para>- GET: /UserActivities</para>
    /// </summary>
    public class AccountingActivitiesSync : ApiAccessorController<FinanceApi>
    {
        public AccountingActivitiesSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /AccountingUsage/
        /// </summary>
        /// <returns>Returns information regarding account usage</returns>
        public async Task<ActionResult> AccountingUsage()
        {
            // Call endpoint, requesting account activity. Note, 12 months prior to the end month will be used
            var response = await Api.GetAccountingActivityAccountUsageAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /LockHistory/
        /// </summary>
        /// <returns>Returns information regarding lock history</returns>
        public async Task<ActionResult> LockHistory()
        {
            // Call endpoint, requesting for a history of locking accounting books until now
            var response = await Api.GetAccountingActivityLockHistoryAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ReportHistory/
        /// </summary>
        /// <returns>Returns information regarding report history</returns>
        public async Task<ActionResult> ReportHistory()
        {
            // Requesting, for a specified organisation, a summary of all the reports published to this day.
            var response = await Api.GetAccountingActivityReportHistoryAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /UserActivities/
        /// </summary>
        /// <returns>Returns information regarding user activities</returns>
        public async Task<ActionResult> UserActivities()
        {
            // Requesting, for a specified organisation, a summary of all the reports published to this day.
            var response = await Api.GetAccountingActivityUserActivitiesAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /AccountingUsageSync#Create
        /// <para>Helper method to return View</para>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        #endregion
    }
}