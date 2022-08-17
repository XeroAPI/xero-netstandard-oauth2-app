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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for today's cash position
      var response = await FinanceApi.GetFinancialStatementBalanceSheetAsync(accessToken, xeroTenantId, null);
    
      ViewBag.jsonResponse = response.ToJson();
      
      return View(response);
    }


    // GET: /Cashflow/
    public async Task<ActionResult> Cashflow()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for past 12 months' cashflow statement
      var response = await FinanceApi.GetFinancialStatementCashflowAsync(accessToken, xeroTenantId, null, null);
    
      ViewBag.jsonResponse = response.ToJson();
      
      return View(response);
    }


    // GET: /ContactExpense/
    public async Task<ActionResult> ContactExpense()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for expense by contact report for the past 12 months excluding manual journals
      var response = await FinanceApi.GetFinancialStatementContactsExpenseAsync (accessToken, xeroTenantId, null, null, null, null);
    
      ViewBag.jsonResponse = response.ToJson();
      
      return View(response);
    }
    
    
    // GET: /ContactRevenue/
    public async Task<ActionResult> ContactRevenue()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for revenue by contact report for the past 12 months excluding manual journals
      var response = await FinanceApi.GetFinancialStatementContactsRevenueAsync (accessToken, xeroTenantId, null, null, null, null);
    
      ViewBag.jsonResponse = response.ToJson();
      
      return View(response);
    }


    // GET: /ProfitAndLoss/
    public async Task<ActionResult> ProfitAndLoss()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for past 12 months' profit and loss statement
      var response = await FinanceApi.GetFinancialStatementProfitAndLossAsync(accessToken, xeroTenantId, null, null);
    
      ViewBag.jsonResponse = response.ToJson();
      
      return View(response);
    }
    

    // GET: /TrialBalance/
    public async Task<ActionResult> TrialBalance()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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