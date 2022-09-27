using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.PayrollUk;

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
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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
    public async Task<ActionResult> Create(string firstName, string lastName)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);
      var xeroTenantId = TokenUtilities.GetCurrentTenantId().ToString();

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
      await PayrollAUApi.CreateEmployeeAsync(accessToken, xeroTenantId, employee);

      return RedirectToAction("Index", "UkEmployeesInfo");
    }
  }
}