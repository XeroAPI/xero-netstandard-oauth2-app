using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;

namespace XeroNetStandardApp.Controllers
{
    public class BankfeedStatements : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;

        public BankfeedStatements(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
        }

        // GET: /BankfeedStatements/
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Authentication   
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

            // Instantiate BankFeed API and retrieve all statements 
            var BankFeedsApi = new BankFeedsApi();
            var response = await BankFeedsApi.GetStatementsAsync(accessToken, xeroTenantId);
            ViewBag.jsonResponse = response.ToJson();

            var statements = response.Items;
            return View(statements.Where(statement => statement.Status == Statement.StatusEnum.DELIVERED));
        }

        // GET: /BankfeedStatements#Create
        [HttpGet]
        public async Task<IActionResult> Create()
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

            var bankFeedsApi = new BankFeedsApi();
            var connections = await bankFeedsApi.GetFeedConnectionsAsync(accessToken, xeroTenantId);

            return View(connections.Items.Select(item => item.Id.ToString()));
        }

        // POST: /BankfeedStatements#Create
        [HttpPost]
        public async Task<ActionResult> Create(
          string feedConnectionId,
          string startBalanceAmount,
          string startBalanceIndicator
        )
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

            var BankfeedsApi = new BankFeedsApi();

            Enum.TryParse<CreditDebitIndicator>(startBalanceIndicator, out var startIndicatorEnum);

            StartBalance startBalance = new StartBalance
            {
                Amount = decimal.Parse(startBalanceAmount),
                CreditDebitIndicator = startIndicatorEnum
            };


            StatementLine statementLine = new StatementLine
            {
                PostedDate = DateTime.Today,
                Description = "neat",
                Amount = 10,
                CreditDebitIndicator = startIndicatorEnum,
                TransactionId = Guid.NewGuid().ToString()
            };

            EndBalance endBalance = new EndBalance
            {
                Amount = decimal.Parse(startBalanceAmount) + statementLine.Amount,
                CreditDebitIndicator = startIndicatorEnum
            };

            List<StatementLine> statementLines = new List<StatementLine>();
            statementLines.Add(statementLine);

            var statement = new Statement
            {
                FeedConnectionId = new Guid(feedConnectionId),
                StartDate = DateTime.Today.AddDays(-20),
                EndDate = DateTime.Today,
                StartBalance = startBalance,
                EndBalance = endBalance,
                StatementLines = statementLines,
            };

            List<Statement> statementList = new List<Statement>();
            statementList.Add(statement);

            Statements statements = new Statements
            {
                Pagination = new Pagination(),
                Items = statementList
            };

            await BankfeedsApi.CreateStatementsAsync(accessToken, xeroTenantId, statements);

            return RedirectToAction("Index", "BankfeedStatements");
        }

    }
}