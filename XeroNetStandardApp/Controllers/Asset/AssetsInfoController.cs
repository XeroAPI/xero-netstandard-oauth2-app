using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Asset;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following asset endpoints:
    /// <para>- GET: /Assets</para>
    /// </summary>
    public class AssetsInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AssetApi _assetApi;

        public AssetsInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _assetApi = new AssetApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /Assets/
        /// </summary>
        /// <returns>Returns list of assets</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get assets endpoint
            var response = await _assetApi.GetAssetsAsync(xeroToken.AccessToken, xeroTenantId, AssetStatusQueryParam.DRAFT);

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }

        /// <summary>
        /// GET: /AssetsInfo#Create
        /// <para>Helper method to return View</para>
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
        /// POST: /AssetsInfo#Create
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns>Returns action result redirecting user to get assets page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string name, string number)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct asset object
            var asset = new Asset
            {
                AssetName = name,
                AssetNumber = number
            };

            // Call create asset endpoint
            await _assetApi.CreateAssetAsync(xeroToken.AccessToken, xeroTenantId, asset);

            return RedirectToAction("Index", "AssetsInfo");
        }

        #endregion
    }
}