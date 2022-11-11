using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Finance endpoints:
    /// <para>- GET: /BalanceSheet/</para>
    /// <para>- GET: /Cashflow/</para>
    /// <para>- GET: /ContactExpense/</para>
    /// <para>- GET: /ContactRevenue/</para>
    /// <para>- GET: /ProfitAndLoss/</para>
    /// <para>- GET: /TrialBalance/</para>
    /// </summary>
    public class FinancialStatementsSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FinanceApi _financeApi;

        public FinancialStatementsSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _financeApi = new FinanceApi();
        }


        /// <summary>
        /// GET: /BalanceSheet/
        /// </summary>
        /// <returns>Returns a list of financial statement balance sheets</returns>
        public async Task<ActionResult> BalanceSheet()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);


            // Call get financial statement balance sheets endpoint
            var response = await _financeApi.GetFinancialStatementBalanceSheetAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /Cashflow/
        /// </summary>
        /// <returns>Returns list of financial statement cash flows</returns>
        public async Task<ActionResult> Cashflow()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get financial statement cash flow endpoint
            var response = await _financeApi.GetFinancialStatementCashflowAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ContactExpense/
        /// </summary>
        /// <returns>Returns a list of financial statement contacts expenses</returns>
        public async Task<ActionResult> ContactExpense()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get financial statement contacts expenses endpoint
            var response = await _financeApi.GetFinancialStatementContactsExpenseAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ContactRevenue/
        /// </summary>
        /// <returns>Returns a list of financial statement contacts revenue</returns>
        public async Task<ActionResult> ContactRevenue()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get financial statement contacts revenue endpoint
            var response = await _financeApi.GetFinancialStatementContactsRevenueAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ProfitAndLoss/
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ProfitAndLoss()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get finanical statement profit and loss endpoint
            var response = await _financeApi.GetFinancialStatementProfitAndLossAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /TrialBalance/
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> TrialBalance()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get financial statement trial balance endpoint
            var response = await _financeApi.GetFinancialStatementTrialBalanceAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }
    }
}