using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /ManualJournalInfo/</para>
    /// <para>- POST: /ManualJournalInfo#Create</para>
    /// </summary>
    public class ManualJournalInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public ManualJournalInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _accountingApi = new AccountingApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /ManualJournalInfo/
        /// </summary>
        /// <returns>Returns a list of manual journals</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get manual journals endpoint
            var response = await _accountingApi.GetManualJournalsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._ManualJournals);
        }

        /// <summary>
        /// GET: /ManualJournalInfo/#manualJournalId
        /// </summary>
        /// <param name="manualJournalId">Id of a specific manual journal to get</param>
        /// <returns>Returns a specific journal specified by provided id if exists</returns>
        [HttpGet]
        public async Task<IActionResult> FindJournal(Guid manualJournalId)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get manual journal for a specific journal
            var response = await _accountingApi.GetManualJournalAsync(xeroToken.AccessToken, xeroTenantId, manualJournalId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._ManualJournals.First());
        }


        /// <summary>
        /// GET: /ManualJournalInfo#Create
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
        /// POST: /ManualJournalInfo#Create
        /// </summary>
        /// <param name="narration">Narration of manual journal to create</param>
        /// <param name="taxType">Tax type of manual journal lines to create</param>
        /// <returns>Returns action result to redirect user to get journals page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string narration, string taxType)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct manual journals object
            // Manual journals must contain at least two journal lines
            var manualJournalLines = new List<ManualJournalLine>
            {
                new ManualJournalLine
                {
                    LineAmount = 100,
                    AccountCode = "400",
                    TaxType = taxType
                }, 
                new ManualJournalLine
                {
                    LineAmount = -100,
                    AccountCode = "120",
                    TaxType = taxType
                }
            };

            var manualJournals = new ManualJournals
            {
                _ManualJournals = new List<ManualJournal>
                {
                    new ManualJournal
                    {
                        Narration = narration,
                        JournalLines = manualJournalLines,
                        Date = DateTime.Today
                    }
                }
            };

            // Call create manual journal endpoint
            await _accountingApi.CreateManualJournalsAsync(xeroToken.AccessToken, xeroTenantId, manualJournals);

            return RedirectToAction("Index", "ManualJournalInfo");
        }

        #endregion
    }
}
