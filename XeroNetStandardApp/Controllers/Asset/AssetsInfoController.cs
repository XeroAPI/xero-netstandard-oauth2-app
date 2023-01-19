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
    public class AssetsInfo : ApiAccessorController<AssetApi>
    {
        public AssetsInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /Assets/
        /// </summary>
        /// <returns>Returns list of assets</returns>
        public async Task<IActionResult> Index()
        {
            // Call get assets endpoint
            var response = await Api.GetAssetsAsync(XeroToken.AccessToken, TenantId, AssetStatusQueryParam.DRAFT);

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
        public async Task<IActionResult> Create(string name, string number)
        {
            // Construct asset object
            var asset = new Asset
            {
                AssetName = name,
                AssetNumber = number
            };

            // Call create asset endpoint
            await Api.CreateAssetAsync(XeroToken.AccessToken, TenantId, asset);

            return RedirectToAction("Index", "AssetsInfo");
        }

        #endregion
    }
}