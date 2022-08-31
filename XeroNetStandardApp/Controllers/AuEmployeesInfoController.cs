using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.PayrollAu;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace XeroNetStandardApp.Controllers
{
  public class AuEmployeesInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public AuEmployeesInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /AuEmployeesInfo#Index
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

      var PayrollAUApi = new PayrollAuApi();
      var response = await PayrollAUApi.GetEmployeesAsync(accessToken, xeroTenantId);
      ViewBag.jsonResponse = response.ToJson();

      var employees = response._Employees;

      return View(employees);
    }

    // GET: /AuEmployeesInfo#FindEmployee
    [HttpGet]
    public async Task<IActionResult> FindEmployee(Guid employeeId)
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
        var PayrollAUApi = new PayrollAuApi();
        var response = await PayrollAUApi.GetEmployeeAsync(accessToken, xeroTenantId, employeeId);
        ViewBag.jsonResponse = response.ToJson();

        return View(response._Employees.First());
    }


    // GET: /AuEmployeesInfo#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /AuEmployeesInfo#Create
    [HttpPost]
    public async Task<ActionResult> Create(string firstName, string lastName, string employeeId)
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

      HomeAddress homeAddress = new HomeAddress() {
        AddressLine1 = "123 Main St",
        AddressLine2 = "St. Kilda",
        Region = State.ACT,
        City = "Waiwhetu",
        PostalCode = "3182", 
        Country = "AUSTRALIA"
      };

      Employee employee = new Employee() {
        FirstName = firstName,
        LastName = lastName,
        DateOfBirth = dob,
        HomeAddress = homeAddress
      };

      // If only the employee ID is a valid Guid will it be used to create/update an employee.
      // Otherwise, a new Guid will be created as CreateEmployeeAsync gets called
      if((employeeId != null) && (Guid.TryParse(employeeId, out var guidOutput)))
        employee.EmployeeID = Guid.Parse(employeeId);

      var employees = new List<Employee>() { employee };

      var objectFullName = employees.GetType().FullName;
      String result = JsonConvert.SerializeObject(employees);

      var PayrollAUApi = new PayrollAuApi();
      var response = await PayrollAUApi.CreateEmployeeAsync(accessToken, xeroTenantId, employees);

      return RedirectToAction("Index", "AuEmployeesInfo");
    }
  }
}