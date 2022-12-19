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
    /// <para>- GET: /Contacts/</para>
    /// <para>- POST: /Contacts#Create</para>
    /// </summary>
    public class ContactsInfo : ApiAccessorController<AccountingApi>
    {
        public ContactsInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}
         
        #region GET Endpoints

        /// <summary>
        /// GET: /Contacts/
        /// </summary>
        /// <returns>Returns a list of contacts</returns>
        public async Task<IActionResult> Index()
        {
            // Call get contacts endpoint
            var response = await Api.GetContactsAsync(XeroToken.AccessToken, TenantId);

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
        public async Task<IActionResult> Create(string name, string emailAddress)
        {
            // Create contact object
            var contact = new Contact
            {
                Name = name,
                EmailAddress = emailAddress
            };
            var contacts = new Contacts { _Contacts = new List<Contact> { contact } };

            await Api.CreateContactsAsync(XeroToken.AccessToken, TenantId, contacts);

            return RedirectToAction("Index", "ContactsInfo");
        }

        #endregion
    }
}