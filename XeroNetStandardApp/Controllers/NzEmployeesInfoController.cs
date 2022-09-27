using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.PayrollNz;

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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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
    public async Task<ActionResult> Create(string firstName, string lastName)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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
      await PayrollNZApi.CreateEmployeeAsync(accessToken, xeroTenantId, employee);

      return RedirectToAction("Index", "NzEmployeesInfo");
    }
  }
}