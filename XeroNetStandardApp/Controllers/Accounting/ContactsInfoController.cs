using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{

    /// <summary>
    /// Controller implementing methods demonstrating following accounting endpoints:
    /// <para>- GET: /Contacts/</para>
    /// <para>- POST: /Contacts#Create</para>
    /// </summary>
    public class ContactsInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly AccountingApi _accountingApi;

        public ContactsInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _accountingApi = new AccountingApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /Contacts/
        /// </summary>
        /// <returns>Returns a list of contacts</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get contacts endpoint
            var response = await _accountingApi.GetContactsAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._Contacts);
        }

        /// <summary>
        /// GET: /Contacts#Create
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
        /// POST: /Contacts#Create
        /// </summary>
        /// <param name="name">Name of contact to create</param>
        /// <param name="emailAddress">Email address of contact to create</param>
        /// <returns>Returns action result to redirect user to get contacts page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string name, string emailAddress)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            var contact = new Contact
            {
                Name = name,
                EmailAddress = emailAddress
            };
            var contacts = new Contacts { _Contacts = new List<Contact> { contact } };

            await _accountingApi.CreateContactsAsync(xeroToken.AccessToken, xeroTenantId, contacts);

            return RedirectToAction("Index", "ContactsInfo");
        }

        #endregion
    }
}