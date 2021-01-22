using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.PayrollUk;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace XeroNetStandardApp.Controllers
{
  public class UkEmployeesInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public UkEmployeesInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /UkEmployeesInfo#Index
    public async Task<ActionResult> Index()
    {
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

      var PayrollUKApi = new PayrollUkApi();
      var response = await PayrollUKApi.GetEmployeesAsync(accessToken, xeroTenantId);

      var timesheetResponse = await PayrollUKApi.GetTimesheetsAsync(accessToken, xeroTenantId);
      Console.WriteLine("--- timesheet Response ---");
      Console.WriteLine(timesheetResponse.ToString());

      var employees = response._Employees;
      ViewBag.jsonResponse = response.ToJson();

      return View(employees);
    }

    // GET: /UkEmployeesInfo#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /UkEmployeesInfo#Create
    [HttpPost]
    public async Task<ActionResult> Create(string firstName, string lastName, string DateOfBirth)
    {
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

      DateTime dob = DateTime.Today.AddYears(-20);

      Address homeAddress = new Address() {
        AddressLine1 = "171 Midsummer",
        City = "Milton Keyness",
        PostCode = "MK9 1EB"
      };

      Employee employee = new Employee() {
        FirstName = firstName,
        LastName = lastName,
        DateOfBirth = dob,
        Address = homeAddress,
        Gender = Employee.GenderEnum.M,
        Title = "worker"
      };

      var PayrollAUApi = new PayrollUkApi();
      var response = await PayrollAUApi.CreateEmployeeAsync(accessToken, xeroTenantId, employee);

      return RedirectToAction("Index", "UkEmployeesInfo");
    }
  }
}