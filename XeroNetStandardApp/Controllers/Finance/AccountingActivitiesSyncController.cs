using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers.Finance
{
    /// <summary>
    /// Controller implementing methods demonstrating following finance endpoints:
    /// <para>- GET: /AccountingUsage</para>
    /// <para>- GET: /LockHistory</para>
    /// <para>- GET: /ReportHistory</para>
    /// <para>- GET: /UserActivities</para>
    /// </summary>
    public class AccountingActivitiesSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FinanceApi _financeApi;

        public AccountingActivitiesSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _financeApi = new FinanceApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /AccountingUsage/
        /// </summary>
        /// <returns>Returns information regarding account usage</returns>
        public async Task<ActionResult> AccountingUsage()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call endpoint, requesting account activity. Note, 12 months prior to the end month will be used
            var response = await _financeApi.GetAccountingActivityAccountUsageAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /LockHistory/
        /// </summary>
        /// <returns>Returns information regarding lock history</returns>
        public async Task<ActionResult> LockHistory()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call endpoint, requesting for a history of locking accounting books until now
            var response = await _financeApi.GetAccountingActivityLockHistoryAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ReportHistory/
        /// </summary>
        /// <returns>Returns information regarding report history</returns>
        public async Task<ActionResult> ReportHistory()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Requesting, for a specified organisation, a summary of all the reports published to this day.
            var response = await _financeApi.GetAccountingActivityReportHistoryAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /UserActivities/
        /// </summary>
        /// <returns>Returns information regarding user activities</returns>
        public async Task<ActionResult> UserActivities()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Requesting, for a specified organisation, a summary of all the reports published to this day.
            var response = await _financeApi.GetAccountingActivityUserActivitiesAsync(xeroToken.AccessToken, xeroTenantId);

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