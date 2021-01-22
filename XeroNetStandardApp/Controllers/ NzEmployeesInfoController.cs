using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.PayrollNz;
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
  public class NzEmployeesInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public NzEmployeesInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /NzEmployeesInfo#Index
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

      var PayrollNZApi = new PayrollNzApi();
      var response = await PayrollNZApi.GetEmployeesAsync(accessToken, xeroTenantId);

      ViewBag.jsonResponse = response.ToJson();

      var timesheetResponse = await PayrollNZApi.GetTimesheetsAsync(accessToken, xeroTenantId);
      Console.WriteLine("--- timesheet Response ---");
      Console.WriteLine(timesheetResponse.ToString());

      var employees = response._Employees;

      return View(employees);
    }

    // GET: /NzEmployeesInfo#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /NzEmployeesInfo#Create
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
        City = "Milton Keynes",
        PostCode = "1234"
      };

      Employee employee = new Employee() {
        FirstName = firstName,
        LastName = lastName,
        DateOfBirth = dob,
        Address = homeAddress,
        Gender = Employee.GenderEnum.M,
        Title = "worker"
      };

      var PayrollNZApi = new PayrollNzApi();
      var response = await PayrollNZApi.CreateEmployeeAsync(accessToken, xeroTenantId, employee);

      return RedirectToAction("Index", "NzEmployeesInfo");
    }
  }
}