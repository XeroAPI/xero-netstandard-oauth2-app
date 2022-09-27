using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Asset;

namespace XeroNetStandardApp.Controllers
{
  public class AssetsInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public AssetsInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /Assets/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AssetApi = new AssetApi();

      var response = await AssetApi.GetAssetsAsync(accessToken, xeroTenantId, AssetStatusQueryParam.DRAFT);

      var assetItems = response.Items;

      ViewBag.jsonResponse = response.ToJson();

      return View(assetItems);
    }

    // GET: /AssetsInfo#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /AssetsInfo#Create
    [HttpPost]
    public async Task<ActionResult> Create(string Name, string Number)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var asset = new Asset() {
        AssetName = Name,
        AssetNumber = Number
      };

      var AssetApi = new AssetApi();
      await AssetApi.CreateAssetAsync(accessToken, xeroTenantId, asset);

      return RedirectToAction("Index", "AssetsInfo");
    }
  }
}