using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /BankfeedConnections/</para>
    /// <para>- POST: /BankfeedConnections#Create</para>
    /// <para>- Get: /Bankfeeds#Delete</para>
    /// </summary>
    public class BankfeedConnections : ApiAccessorController<BankFeedsApi>
    {
        public BankfeedConnections(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoint
        
        /// <summary>
        /// GET: /BankfeedConnections/
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Call get feed connections endpoint
            var response = await Api.GetFeedConnectionsAsync(XeroToken.AccessToken, TenantId);

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
            await Api.CreateFeedConnectionsAsync(XeroToken.AccessToken, TenantId, feedConnections);

            return RedirectToAction("Index", "BankfeedConnections");
        }


        /// <summary>
        /// Get: /Bankfeeds#Delete
        /// </summary>
        /// <param name="bankfeedConnectionId">Bank feed connection id of bank feed to delete</param>
        /// <returns>Returns action result to redirect user to get bank feed connections page</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(string bankfeedConnectionId)
        {
            // Construct feedConnections object
            var feedConnections = new FeedConnections
            {
                Items = new List<FeedConnection>
                {
                    new FeedConnection{ Id = Guid.Parse(bankfeedConnectionId) }
                }
            };

            // Call delete feed connection endpoint
            await Api.DeleteFeedConnectionsAsync(XeroToken.AccessToken, TenantId, feedConnections);

            return RedirectToAction("Index", "BankfeedConnections");
        }

        #endregion
    }
}