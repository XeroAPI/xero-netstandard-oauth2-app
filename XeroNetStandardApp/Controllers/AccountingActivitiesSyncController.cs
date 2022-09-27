using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;


namespace XeroNetStandardApp.Controllers
{
  public class AccountingActivitiesSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public AccountingActivitiesSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
      this.httpClientFactory = httpClientFactory;
    }


    // GET: /AccountingUsage/
    public async Task<ActionResult> AccountingUsage()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for 12 months prior to the end month will be used
      var response = await FinanceApi.GetAccountingActivityAccountUsageAsync(accessToken, xeroTenantId, null, null);

      ViewBag.jsonResponse = response.ToJson();

      return View(response);
    }


    // GET: /LockHistory/
    public async Task<ActionResult> LockHistory()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for a history of locking of accounting books until now
      var response = await FinanceApi.GetAccountingActivityLockHistoryAsync(accessToken, xeroTenantId, null);

      ViewBag.jsonResponse = response.ToJson();

      return View(response);
    }


    // GET: /ReportHistory/
    public async Task<ActionResult> ReportHistory()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting, for a specified organisation, a summary of all the reports published to this day.
      var response = await FinanceApi.GetAccountingActivityReportHistoryAsync(accessToken, xeroTenantId, null);

      ViewBag.jsonResponse = response.ToJson();

      return View(response);
    }


    // GET: /UserActivities/
    public async Task<ActionResult> UserActivities()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting, for a specified organisation, a summary of all the reports published to this day.
      var response = await FinanceApi.GetAccountingActivityUserActivitiesAsync(accessToken, xeroTenantId, null);

      ViewBag.jsonResponse = response.ToJson();

      return View(response);
    }


    // GET: /AccountingUsageSync#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }
  }
}