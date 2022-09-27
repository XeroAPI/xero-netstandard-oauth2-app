using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;

namespace XeroNetStandardApp.Controllers
{
  public class BankfeedConnections : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public BankfeedConnections(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /BankfeedConnections/
    [HttpGet]
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var BankFeedsApi = new BankFeedsApi();

      var response = await BankFeedsApi.GetFeedConnectionsAsync(accessToken, xeroTenantId);
      
      ViewBag.jsonResponse = response.ToJson();

      return View(response.Items);
    }

    // GET: /BankfeedConnections#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /BankfeedConnections#Create
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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var BankfeedsApi = new BankFeedsApi();

      Enum.TryParse<FeedConnection.AccountTypeEnum>(accountType, out var accountTypeEnum);
      Enum.TryParse<CurrencyCode>(currency, out var currencyCode);
      Enum.TryParse<CountryCode>(country, out var countryCode);

      var feedConnection = new FeedConnection{
        AccountToken = accountToken, 
        AccountNumber = accountNumber,
        AccountType =  accountTypeEnum,
        AccountName = accountName,
        Currency = currencyCode,
        Country = countryCode
      };

      List<FeedConnection> list = new List<FeedConnection>();
      list.Add(feedConnection);

      FeedConnections items = new FeedConnections{
        Pagination = new Pagination(),
        Items = list
      };

      await BankfeedsApi.CreateFeedConnectionsAsync(accessToken, xeroTenantId, items);

      return RedirectToAction("Index", "BankfeedConnections");
    }


    // Get: /Bankfeeds#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string bankfeedConnectionId)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      Guid bankfeedConnectionIdGuid = Guid.Parse(bankfeedConnectionId);

      List<FeedConnection> list = new List<FeedConnection>();
      list.Add(
        new FeedConnection {
          Id = bankfeedConnectionIdGuid
        }
      );

      var feedConnections = new FeedConnections{
          Items = list
      };

      var BankFeedsApi = new BankFeedsApi();
      await BankFeedsApi.DeleteFeedConnectionsAsync(accessToken, xeroTenantId, feedConnections);
      
      return RedirectToAction("Index", "BankfeedConnections");
    }

  }
}