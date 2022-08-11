using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.PayrollAu;

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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

      var PayrollAUApi = new PayrollAuApi();
      var response = await PayrollAUApi.GetEmployeesAsync(accessToken, xeroTenantId);
      ViewBag.jsonResponse = response.ToJson();

      var employees = response._Employees;

      return View(employees);
    }

    // GET: /AuEmployeesInfo#Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: /AuEmployeesInfo#Create
    [HttpPost]
    public async Task<ActionResult> Create(string firstName, string lastName)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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

      var employees = new List<Employee>() { employee };

      var PayrollAUApi = new PayrollAuApi();
      await PayrollAUApi.CreateEmployeeAsync(accessToken, xeroTenantId, employees);

      return RedirectToAction("Index", "AuEmployeesInfo");
    }
  }
}