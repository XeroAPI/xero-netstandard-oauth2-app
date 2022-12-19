using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /BankTransactionsInfo/</para>
    /// </summary>
    public class BankTransactionsInfo : ApiAccessorController<AccountingApi>
    {

        public BankTransactionsInfo(IOptions<XeroConfiguration> xeroConfig): base(xeroConfig) { }

        /// <summary>
        /// GET: /BankTransactionsInfo/
        /// </summary>
        /// <returns>Returns list of bank transactions</returns>
        public async Task<IActionResult> Index()
        {
            // Call get bank transaction endpoint
            var response = await Api.GetBankTransactionsAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._BankTransactions);
        }

    }
}