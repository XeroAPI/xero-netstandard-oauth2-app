using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller supporting get accounts endpoint
    /// </summary>
    public class AccountsInfoAccessorController : ApiAccessorController<AccountingApi>
    {
        public AccountsInfoAccessorController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig){}

        /// <summary>
        /// GET: /GetAccounts
        /// </summary>
        /// <returns>Returns list of accounts</returns>
        public async Task<IActionResult> Index()
        {
            var response = await Api.GetAccountsAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();

            

            // Limit response to max of 10 accounts in display
            return View(response._Accounts.Take(10));
        }
    }
}
