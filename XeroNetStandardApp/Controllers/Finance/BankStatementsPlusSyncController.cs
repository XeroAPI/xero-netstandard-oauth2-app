using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xero.NetStandard.OAuth2.Model.Finance;


namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following Finance endpoints
    /// <para>- GET: /BankStatementsPlusSync/</para>
    /// </summary>
    public class BankStatementsPlusSync : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly FinanceApi _financeApi;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroConfig"></param>
        public BankStatementsPlusSync(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _financeApi = new FinanceApi();
        }

        /// <summary>
        /// GET: /BankStatementsPlusSync/
        /// </summary>
        /// <returns>Returns a list of bank statement responses</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            var accountingApi = new AccountingApi();

            // Call get bank statement accounting endpoint
            var whereQuery = "Status==\"ACTIVE\" AND Type==\"BANK\"";
            var accountsResponse = await accountingApi.GetAccountsAsync(xeroToken.AccessToken, xeroTenantId, null, whereQuery);

            BankStatementAccountingResponse bankStatementsResponse = null;
            if (accountsResponse._Accounts.Count > 0)
            {
                bankStatementsResponse = await _financeApi.GetBankStatementAccountingAsync(
                    xeroToken.AccessToken,
                    xeroTenantId,
                    accountsResponse._Accounts[0].AccountID.Value,
                    "2021-04-01",
                    "2022-03-01"
                );
            }
            

            ViewBag.jsonResponse = JsonConvert.SerializeObject(bankStatementsResponse);
            return View(bankStatementsResponse);
        }
    }
}