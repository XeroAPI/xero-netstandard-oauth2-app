using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;

namespace XeroNetStandardApp.Controllers
{
    public class BankTransactionsInfo : Controller
  {

    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public BankTransactionsInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /BankTransactionsInfo/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AccountingApi = new AccountingApi();
      var response = await AccountingApi.GetBankTransactionsAsync(accessToken, xeroTenantId);
      var bankTransactions = response._BankTransactions;
      ViewBag.jsonResponse = response.ToJson();

      return View(bankTransactions);
    }

  }
}