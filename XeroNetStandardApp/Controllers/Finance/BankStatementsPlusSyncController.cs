using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Model.Finance;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace XeroNetStandardApp.Controllers.Finance
{
    public class BankStatementsPlusSync : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public BankStatementsPlusSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }


        // GET: /BankStatementsPlusSync/
        public async Task<ActionResult> Index()
        {
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

            var FinanceApi = new FinanceApi();
            var AccountingApi = new AccountingApi();

            var where = "Status==\"ACTIVE\" AND Type==\"BANK\"";
            var accountsResponse = await AccountingApi.GetAccountsAsync(accessToken, xeroTenantId, null, where);


            Guid? accountId = accountsResponse._Accounts[0].AccountID;
            Guid accountIdGuid = accountId.Value;
            var fromDate = "2021-04-01";
            var toDate = "2022-03-01";
            var bankStatementsResponse = await FinanceApi.GetBankStatementAccountingAsync(accessToken, xeroTenantId, accountIdGuid, fromDate, toDate);

            ViewBag.jsonResponse = JsonConvert.SerializeObject(bankStatementsResponse);

            return View(bankStatementsResponse);
        }


        // GET: /BankStatementsPlusSync#Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}