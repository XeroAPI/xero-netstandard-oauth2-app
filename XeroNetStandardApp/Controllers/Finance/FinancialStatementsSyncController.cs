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
    public class FinancialStatementsSync : ApiAccessorController<FinanceApi>
    {
        public FinancialStatementsSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        /// <summary>
        /// GET: /BalanceSheet/
        /// </summary>
        /// <returns>Returns a list of financial statement balance sheets</returns>
        public async Task<ActionResult> BalanceSheet()
        {
            // Call get financial statement balance sheets endpoint
            var response = await Api.GetFinancialStatementBalanceSheetAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /Cashflow/
        /// </summary>
        /// <returns>Returns list of financial statement cash flows</returns>
        public async Task<ActionResult> Cashflow()
        {
            // Call get financial statement cash flow endpoint
            var response = await Api.GetFinancialStatementCashflowAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ContactExpense/
        /// </summary>
        /// <returns>Returns a list of financial statement contacts expenses</returns>
        public async Task<ActionResult> ContactExpense()
        {
            // Call get financial statement contacts expenses endpoint
            var response = await Api.GetFinancialStatementContactsExpenseAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ContactRevenue/
        /// </summary>
        /// <returns>Returns a list of financial statement contacts revenue</returns>
        public async Task<ActionResult> ContactRevenue()
        {
            // Call get financial statement contacts revenue endpoint
            var response = await Api.GetFinancialStatementContactsRevenueAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /ProfitAndLoss/
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ProfitAndLoss()
        {
            // Call get finanical statement profit and loss endpoint
            var response = await Api.GetFinancialStatementProfitAndLossAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }


        /// <summary>
        /// GET: /TrialBalance/
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> TrialBalance()
        {
            // Call get financial statement trial balance endpoint
            var response = await Api.GetFinancialStatementTrialBalanceAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response);
        }
    }
}