using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;


namespace XeroNetStandardApp.Controllers
{
    public class OrganisationInfo : Controller
  {

    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public OrganisationInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /Organisation/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AccountingApi = new AccountingApi();
      var response = await AccountingApi.GetOrganisationsAsync(accessToken, xeroTenantId);
      // var response = await AccountingApi.GetOrganisationsAsyncWithHttpInfo(accessToken, xeroTenantId);

      var organisation_info = new Organisation();
      
      organisation_info = response._Organisations[0];
      ViewBag.jsonResponse = response.ToJson();

      return View(organisation_info);
    }
  }
}