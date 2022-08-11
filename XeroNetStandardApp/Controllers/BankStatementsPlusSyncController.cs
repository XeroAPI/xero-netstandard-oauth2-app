using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;


namespace XeroNetStandardApp.Controllers
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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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