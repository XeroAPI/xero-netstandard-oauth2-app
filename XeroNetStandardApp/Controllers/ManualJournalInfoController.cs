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

namespace XeroNetStandardApp.Controllers
{
    public class ManualJournalInfo : Controller
    {
        private readonly ILogger<ManualJournalInfo> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;

        public ManualJournalInfo(IOptions<XeroConfiguration> XeroConfig, IHttpClientFactory httpClientFactory, ILogger<ManualJournalInfo> logger)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
        }

        // GET: /ManualJournalInfo/
        public async Task<ActionResult> Index()
        {
            // Authentication
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }
            var AccountingApi = new AccountingApi();

            var response = await AccountingApi.GetManualJournalsAsync(accessToken, xeroTenantId, null, null);

            ViewBag.jsonResponse = response.ToJson();

            return View(response._ManualJournals);


        }

        [HttpGet]
        public async Task<IActionResult> FindJournal(Guid manualJournalId)
        {
            // Authentication
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }
            var AccountingApi = new AccountingApi();

            var response = await AccountingApi.GetManualJournalAsync(accessToken, xeroTenantId, manualJournalId);

            ViewBag.jsonResponse = response.ToJson();

            return View(response._ManualJournals.First());

        }


        // GET: /ManualJournalInfo#Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ManualJournalInfo#Create
        [HttpPost]
        public async Task<ActionResult> Create(String narration, String taxType)
        {
            // Authentication
            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }

            // Creates two journal lines
            var manualJournalLines = new List<ManualJournalLine>();

            var credit = new ManualJournalLine
            {

                LineAmount = new decimal(100.0),
                AccountCode = "400",
                Description = "Credit",
                Tracking = new List<TrackingCategory>(),
                TaxType = taxType
            };
            manualJournalLines.Add(credit);

            var debit = new ManualJournalLine
            {
                LineAmount = new decimal(-100.0),
                AccountCode = "120",
                Description = "Debit",
                Tracking = new List<TrackingCategory>(),
                TaxType = taxType
            };
            manualJournalLines.Add(debit);

            var AccountingApi = new AccountingApi();

            // Picks first tracking category and option if available, and attaches to journal line
            var trackingCategoriesResponse = await AccountingApi.GetTrackingCategoriesAsync(accessToken, xeroTenantId, null, null);
            if (trackingCategoriesResponse._TrackingCategories.Count != 0 && trackingCategoriesResponse._TrackingCategories.First().Options.First().TrackingOptionID != null)
            {
                var trackingCategory = new TrackingCategory
                {
                    TrackingCategoryID = trackingCategoriesResponse._TrackingCategories.First().TrackingCategoryID,
                    TrackingOptionID = trackingCategoriesResponse._TrackingCategories.First().Options.First().TrackingOptionID,
                    Name = trackingCategoriesResponse._TrackingCategories.First().Name,
                    Status = trackingCategoriesResponse._TrackingCategories.First().Status
                };
                credit.Tracking.Add(trackingCategory);
                debit.Tracking.Add(trackingCategory);
            }


            // Create manual journals
            ManualJournal manualJournal = new ManualJournal
            {
                Narration = narration,
                JournalLines = manualJournalLines,
                Date = DateTime.Today
            };

            var manualJournals = new ManualJournals();
            var manualJournalsList = new List<ManualJournal>();
            manualJournalsList.Add(manualJournal);
            manualJournals._ManualJournals = manualJournalsList;

            await AccountingApi.CreateManualJournalsAsync(accessToken, xeroTenantId, manualJournals, null);

            return RedirectToAction("Index", "ManualJournalInfo");
        }


    }
}
