using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;

namespace XeroNetStandardApp.Controllers
{

    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /Currency/</para>
    /// <para>- POST: /Currency#Create</para>
    /// </summary>
    public class CurrencySync : ApiAccessorController<AccountingApi>
    {
        public CurrencySync(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){} //constructor
         
        #region GET Endpoints

        /// <summary>
        /// GET: /Currencies/
        /// </summary>
        /// <returns>Returns a list of currencies</returns>
        public async Task<IActionResult> Index()
        {
            var response = await Api.GetCurrenciesAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson(); 
            return View(response._Currencies);
        }

        /// <summary>
        /// GET: /Currencies#Create
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
        /// POST: /Currencies#Create
        /// </summary>
        /// <param name="CurrencyCode">3 letter currency code</param>
        /// <param name="CurrencyDescription">Description of currency</param>
        /// <returns>Returns action result to redirect user to get currencies page</returns>
        [HttpPost]
        public async Task<IActionResult> Create(string CurrencyCode, string CurrencyDescription)
        {
            // Create currency object
            var currency = new Currency
            {
                Code = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), CurrencyCode),
                Description = CurrencyDescription
            };

            await Api.CreateCurrencyAsync(XeroToken.AccessToken, TenantId, currency);

            return RedirectToAction("Index", "CurrencySync");
        }

        #endregion
    }
}