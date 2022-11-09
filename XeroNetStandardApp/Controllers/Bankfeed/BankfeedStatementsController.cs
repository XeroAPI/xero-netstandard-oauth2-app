using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using System.Linq;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following bankfeed endpoints:
    /// <para>- GET: /BankfeedStatements/</para>
    /// <para>- POST: /BankfeedStatements#Create</para>
    /// </summary>
    public class BankfeedStatements : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly BankFeedsApi _bankFeedsApi;

        public BankfeedStatements(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _bankFeedsApi = new BankFeedsApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /BankfeedStatements/
        /// </summary>
        /// <returns>Return a list of delivered statements</returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get statements endpoint
            var response = await _bankFeedsApi.GetStatementsAsync(xeroToken.AccessToken, xeroTenantId);

            var statements = response.Items;
            ViewBag.jsonResponse = response.ToJson();
            return View(statements.Where(statement => statement.Status == Statement.StatusEnum.DELIVERED));
        }

        /// <summary>
        /// GET: /BankfeedStatements#Create
        /// <para>Helper method to populate create page for creating a new statement</para>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get feed connection endpoint
            var connections = await _bankFeedsApi.GetFeedConnectionsAsync(xeroToken.AccessToken, xeroTenantId);

            return View(connections.Items.Select(item => item.Id.ToString()));
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /BankfeedStatements#Create
        /// </summary>
        /// <param name="feedConnectionId">Feed connection id of statement to create</param>
        /// <param name="startBalanceIndicator">Start balance indicator of statement to create</param>
        /// <param name="startBalanceAmount">Start balance amount of statement to create</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(
            string feedConnectionId,
            string startBalanceAmount,
            string startBalanceIndicator
        )
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct statement object
            Enum.TryParse<CreditDebitIndicator>(startBalanceIndicator, out var startBalanceIndicatorEnum);
            var statements = ConstructStatements(
                new Guid(feedConnectionId),
                decimal.Parse(startBalanceAmount),
                startBalanceIndicatorEnum
            );

            // Call create statement endpoint
            await _bankFeedsApi.CreateStatementsAsync(xeroToken.AccessToken, xeroTenantId, statements);

            return RedirectToAction("Index", "BankfeedStatements");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to construct a mock statement
        /// </summary>
        /// <param name="feedConnectionId">Feed connection id of statement to create</param>
        /// <param name="startBalanceAmount">Start balance amount of statement to create</param>
        /// <param name="startBalanceIndicator">Start balance indicator of statement to create</param>
        /// <returns></returns>
        private Statements ConstructStatements(Guid feedConnectionId, decimal startBalanceAmount, CreditDebitIndicator startBalanceIndicator)
        {
            var startBalance = new StartBalance
            {
                Amount = startBalanceAmount,
                CreditDebitIndicator = startBalanceIndicator
            };

            var endBalance = new EndBalance
            {
                Amount = startBalanceAmount + 10,
                CreditDebitIndicator = startBalanceIndicator
            };

            var statementLines = new List<StatementLine>
            {
                new StatementLine
                {
                    PostedDate = DateTime.Today,
                    Description = "mock description",
                    Amount = 10,
                    CreditDebitIndicator = startBalanceIndicator,
                    TransactionId = Guid.NewGuid().ToString()
                }
            };

            return new Statements
            {
                Pagination = new Pagination(),
                Items = new List<Statement>
                {
                    new Statement
                    {
                        FeedConnectionId = feedConnectionId,
                        StartDate = DateTime.Today.AddDays(-20),
                        EndDate = DateTime.Today,
                        StartBalance = startBalance,
                        EndBalance = endBalance,
                        StatementLines = statementLines,
                    }
                }
            };
        }

        #endregion
    }
}