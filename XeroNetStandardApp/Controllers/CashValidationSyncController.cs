using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;


namespace XeroNetStandardApp.Controllers
{
  public class CashValidationSync : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    private readonly IHttpClientFactory httpClientFactory;

    public CashValidationSync(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
      this.httpClientFactory = httpClientFactory;
    }

    // GET: /CashValidationSync/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var FinanceApi = new FinanceApi();

      // Requesting for today's cash position
      var response = await FinanceApi.GetCashValidationAsync(accessToken, xeroTenantId, null, null, null);

      ViewBag.jsonResponse = JsonConvert.SerializeObject(response);

      return View(response);
    }

    // GET: /CashValidationSync#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }
  }
}