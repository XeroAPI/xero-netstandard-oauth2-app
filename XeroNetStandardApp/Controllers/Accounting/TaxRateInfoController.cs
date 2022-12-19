using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Xero.NetStandard.OAuth2.Model.Accounting.TaxRate;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /TaxRateInfo/</para>
    /// <para>- POST: /TaxRateInfo#Create</para>
    /// </summary>
    public class TaxRateInfoController : ApiAccessorController<AccountingApi>
    {
        public TaxRateInfoController(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /TaxRateInfo/
        /// </summary>
        /// <returns>Returns a list of tax rates</returns>
        public async Task<IActionResult> Index()
        {
            // Call get tax rates endpoint
            var response = await Api.GetTaxRatesAsync(XeroToken.AccessToken, TenantId);

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
        public async Task<IActionResult> Create(string name, string status, string reportTaxType, decimal rate)
        {
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
            await Api.CreateTaxRatesAsync(XeroToken.AccessToken, TenantId, taxRates);

            return RedirectToAction("Index", "TaxRateInfo");
        }

        #endregion
    }
}
