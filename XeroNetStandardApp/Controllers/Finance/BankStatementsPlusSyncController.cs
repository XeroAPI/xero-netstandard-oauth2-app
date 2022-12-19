using System;
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
    public class BankStatementsPlusSync : ApiAccessorController<FinanceApi>
    {
        public BankStatementsPlusSync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}
        
        /// <summary>
        /// GET: /BankStatementsPlusSync/
        /// </summary>
        /// <returns>Returns a list of bank statement responses</returns>
        public async Task<IActionResult> Index()
        {
            var accountingApi = new AccountingApi();

            // Call get bank statement accounting endpoint
            var whereQuery = "Status==\"ACTIVE\" AND Type==\"BANK\"";
            var accountsResponse = await accountingApi.GetAccountsAsync(XeroToken.AccessToken, TenantId, where: whereQuery);

            BankStatementAccountingResponse bankStatementsResponse = null;
            if (accountsResponse._Accounts.Count > 0)
            {
                bankStatementsResponse = await Api.GetBankStatementAccountingAsync(
                    XeroToken.AccessToken,
                    TenantId,
                    accountsResponse._Accounts[0].AccountID.Value,
                    "2021-04-01",
                    DateTime.Now.ToString("yyyy-MM-dd")
                );
            }

            ViewBag.jsonResponse = JsonConvert.SerializeObject(bankStatementsResponse, Formatting.Indented);
            return View(bankStatementsResponse);
        }
    }
}