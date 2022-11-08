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
    public class FinancialStatementsSync : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public FinancialStatementsSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }


        // GET: /BalanceSheet/
        public async Task<ActionResult> BalanceSheet()
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

            // Requesting for today's cash position
            var response = await FinanceApi.GetFinancialStatementBalanceSheetAsync(accessToken, xeroTenantId, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /Cashflow/
        public async Task<ActionResult> Cashflow()
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

            // Requesting for past 12 months' cashflow statement
            var response = await FinanceApi.GetFinancialStatementCashflowAsync(accessToken, xeroTenantId, null, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /ContactExpense/
        public async Task<ActionResult> ContactExpense()
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

            // Requesting for expense by contact report for the past 12 months excluding manual journals
            var response = await FinanceApi.GetFinancialStatementContactsExpenseAsync(accessToken, xeroTenantId, null, null, null, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /ContactRevenue/
        public async Task<ActionResult> ContactRevenue()
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

            // Requesting for revenue by contact report for the past 12 months excluding manual journals
            var response = await FinanceApi.GetFinancialStatementContactsRevenueAsync(accessToken, xeroTenantId, null, null, null, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /ProfitAndLoss/
        public async Task<ActionResult> ProfitAndLoss()
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

            // Requesting for past 12 months' profit and loss statement
            var response = await FinanceApi.GetFinancialStatementProfitAndLossAsync(accessToken, xeroTenantId, null, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /TrialBalance/
        public async Task<ActionResult> TrialBalance()
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

            // Requesting for today's trial balance
            var response = await FinanceApi.GetFinancialStatementTrialBalanceAsync(accessToken, xeroTenantId, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response);
        }


        // GET: /FinancialStatementsSync#Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}