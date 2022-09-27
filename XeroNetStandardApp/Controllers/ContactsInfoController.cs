using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroNetStandardApp.Controllers
{
  public class ContactsInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public ContactsInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /Contacts/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var AccountingApi = new AccountingApi();
      var response = await AccountingApi.GetContactsAsync(accessToken, xeroTenantId);
      ViewBag.jsonResponse = response.ToJson();

      var contacts = response._Contacts;

      return View(contacts);
    }

    // GET: /Contacts#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /Contacts#Create
    [HttpPost]
    public async Task<ActionResult> Create(string name, string emailAddress)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var contact = new Contact{
          Name = name,
          EmailAddress = emailAddress
      };

      var contacts = new Contacts();
      contacts._Contacts = new List<Contact>() { contact };

      var AccountingApi = new AccountingApi();
      await AccountingApi.CreateContactsAsync(accessToken, xeroTenantId, contacts);

      return RedirectToAction("Index", "ContactsInfo");
    }
  }
}