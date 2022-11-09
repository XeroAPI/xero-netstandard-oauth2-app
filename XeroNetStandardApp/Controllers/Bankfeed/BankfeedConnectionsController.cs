using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /BankfeedConnections/</para>
    /// <para>- POST: /BankfeedConnections#Create</para>
    /// <para>- Get: /Bankfeeds#Delete</para>
    /// </summary>
    public class BankfeedConnections : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly BankFeedsApi _bankFeedsApi;

        public BankfeedConnections(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _bankFeedsApi = new BankFeedsApi();
        }

        #region GET Endpoint
        
        /// <summary>
        /// GET: /BankfeedConnections/
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get feed connections endpoint
            var response = await _bankFeedsApi.GetFeedConnectionsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }

        /// <summary>
        /// GET: /BankfeedConnections#Create
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// POST: /BankfeedConnections#Create
        /// </summary>
        /// <param name="accountToken">Account token of feed connection to create</param>
        /// <param name="accountNumber">Account number of feed connection to create</param>
        /// <param name="accountType">Account type of feed connection to create</param>
        /// <param name="accountName">Account name of feed connection to create</param>
        /// <param name="currency">Currency of feed connection to create</param>
        /// <param name="country">Country of feed connection to create</param>
        /// <returns>Return action result redirecting user to get feed connections page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(
          string accountToken,
          string accountNumber,
          string accountType,
          string accountName,
          string currency,
          string country
        )
        {

            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct feed connections object
            Enum.TryParse<FeedConnection.AccountTypeEnum>(accountType, out var accountTypeEnum);
            Enum.TryParse<CurrencyCode>(currency, out var currencyCode);
            Enum.TryParse<CountryCode>(country, out var countryCode);

            var feedConnections = new FeedConnections
            {
                Pagination = new Pagination(),
                Items = new List<FeedConnection>
                {
                    new FeedConnection
                    {
                        AccountToken = accountToken,
                        AccountNumber = accountNumber,
                        AccountType = accountTypeEnum,
                        AccountName = accountName,
                        Currency = currencyCode,
                        Country = countryCode
                    }
                }
            };

            // Call create feed connection endpoint
            await _bankFeedsApi.CreateFeedConnectionsAsync(xeroToken.AccessToken, xeroTenantId, feedConnections);

            return RedirectToAction("Index", "BankfeedConnections");
        }


        /// <summary>
        /// Get: /Bankfeeds#Delete
        /// </summary>
        /// <param name="bankfeedConnectionId">Bank feed connection id of bank feed to delete</param>
        /// <returns>Returns action result to redirect user to get bank feed connections page</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(string bankfeedConnectionId)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct feedConnections object
            var feedConnections = new FeedConnections
            {
                Items = new List<FeedConnection>
                {
                    new FeedConnection{ Id = Guid.Parse(bankfeedConnectionId) }
                }
            };

            // Call delete feed connection endpoint
            await _bankFeedsApi.DeleteFeedConnectionsAsync(xeroToken.AccessToken, xeroTenantId, feedConnections);

            return RedirectToAction("Index", "BankfeedConnections");
        }

        #endregion
    }
}