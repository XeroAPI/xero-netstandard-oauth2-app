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
    public class ManualJournalInfo : ApiAccessorController<AccountingApi>
    {
        public ManualJournalInfo(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig) { }

        #region GET Endpoints

        /// <summary>
        /// GET: /ManualJournalInfo/
        /// </summary>
        /// <returns>Returns a list of manual journals</returns>
        public async Task<ActionResult> Index()
        {
            // Call get manual journals endpoint
            var response = await Api.GetManualJournalsAsync(XeroToken.AccessToken, TenantId);

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
            // Call get manual journal for a specific journal
            var response = await Api.GetManualJournalAsync(XeroToken.AccessToken, TenantId, manualJournalId);

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
            await Api.CreateManualJournalsAsync(XeroToken.AccessToken, TenantId, manualJournals);

            return RedirectToAction("Index", "ManualJournalInfo");
        }

        #endregion
    }
}
