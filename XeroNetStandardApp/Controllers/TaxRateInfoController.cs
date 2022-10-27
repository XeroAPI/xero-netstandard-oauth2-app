using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using static Xero.NetStandard.OAuth2.Model.Accounting.TaxRate;

namespace XeroNetStandardApp.Controllers
{
  public class TaxRateInfoController : Controller
    {
        private readonly ILogger<TaxRateInfoController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public TaxRateInfoController(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<TaxRateInfoController> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }

        // GET: /TaxRateInfo/
        public async Task<ActionResult> Index()
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

            var AccountingApi = new AccountingApi();

            var response = await AccountingApi.GetTaxRatesAsync(accessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();

            return View(response._TaxRates);
        }

        // GET: /TaxRateInfo#Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TaxRateInfo#Create
        [HttpPost]
        public async Task<ActionResult> Create(String name, String status, String reportTaxType, decimal rate)
        {
            // Authentication   
            var client = new XeroClient(XeroConfig.Value);
            var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
            var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();
            
            var taxComponent = new TaxComponent() {
                Name = "State Tax",
                Rate = rate
            };
            var taxComponents = new List<TaxComponent>();
            taxComponents.Add(taxComponent);

            var taxRate = new TaxRate()
            {
                Name = name,
                Status = (StatusEnum)Enum.Parse(typeof(StatusEnum), status),
                ReportTaxType = (ReportTaxTypeEnum)Enum.Parse(typeof(ReportTaxTypeEnum), reportTaxType),
                TaxComponents = taxComponents
            };

            var taxRates = new TaxRates();
            var taxRatesList = new List<TaxRate>();
            taxRatesList.Add(taxRate);
            taxRates._TaxRates = taxRatesList;

            var AccountingApi = new AccountingApi();

            await AccountingApi.CreateTaxRatesAsync(accessToken, xeroTenantId, taxRates);

            return RedirectToAction("Index", "TaxRateInfo");
        }
    }
}
