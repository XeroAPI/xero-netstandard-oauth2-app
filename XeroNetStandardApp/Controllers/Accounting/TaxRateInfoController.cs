using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Api;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Xero.NetStandard.OAuth2.Model.Accounting.TaxRate;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /TaxRateInfo/</para>
    /// <para>- POST: /TaxRateInfo#Create</para>
    /// </summary>
    public class TaxRateInfoController : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public TaxRateInfoController(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _accountingApi = new AccountingApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /TaxRateInfo/
        /// </summary>
        /// <returns>Returns a list of tax rates</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get tax rates endpoint
            var response = await _accountingApi.GetTaxRatesAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._TaxRates);
        }

        /// <summary>
        /// GET: /TaxRateInfo#Create
        /// <para>Helper method for returning a view for create tax rate</para>
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
        /// POST: /TaxRateInfo#Create
        /// </summary>
        /// <param name="name">Name of tax component to create</param>
        /// <param name="status">Status of tax rate to create</param>
        /// <param name="reportTaxType">Report tax type of tax rate to create</param>
        /// <param name="rate">Rate of tax component to create</param>
        /// <returns>Returns action result to redirect user to get tax rates page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string name, string status, string reportTaxType, decimal rate)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct tax rates object
            var taxRates = new TaxRates
            {
                _TaxRates = new List<TaxRate>
                {
                    new TaxRate
                    {
                        Name = name,
                        Status = (StatusEnum)Enum.Parse(typeof(StatusEnum), status),
                        ReportTaxType = (ReportTaxTypeEnum)Enum.Parse(typeof(ReportTaxTypeEnum), reportTaxType),
                        TaxComponents = new List<TaxComponent>{
                            new TaxComponent
                            {
                                Name = name,
                                Rate = rate
                            }
                        }
                    }
                }
            };

            // Call create tax rates endpoint
            await _accountingApi.CreateTaxRatesAsync(xeroToken.AccessToken, xeroTenantId, taxRates);

            return RedirectToAction("Index", "TaxRateInfo");
        }

        #endregion
    }
}
