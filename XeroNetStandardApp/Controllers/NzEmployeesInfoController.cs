using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.PayrollNz;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following NZ Employee info endpoints:
    /// </summary>
    public class NzEmployeesInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly PayrollNzApi _payrollNzApi;

        public NzEmployeesInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _payrollNzApi = new PayrollNzApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /NzEmployeesInfo#Index
        /// </summary>
        /// <returns>Returns list of NZ employees</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get employees endpoint
            var employees = await _payrollNzApi.GetEmployeesAsync(xeroToken.AccessToken, xeroTenantId);
            ViewBag.jsonResponse = employees.ToJson();

            return View(employees._Employees);
        }

        /// <summary>
        /// GET: /NzEmployeesInfo#Create
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
        /// POST: /NzEmployeesInfo#Create
        /// </summary>
        /// <param name="firstName">Firstname of employee to create</param>
        /// <param name="lastName">Lastname of employee to create</param>
        /// <returns>Return action result to redirect user to get employees page</returns>
        [HttpPost]
        public async Task<ActionResult> Create(string firstName, string lastName)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call create employee endpoint
            await _payrollNzApi.CreateEmployeeAsync(xeroToken.AccessToken, xeroTenantId, ConstructEmployee(firstName, lastName));

            return RedirectToAction("Index", "NzEmployeesInfo");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper method to create a new employee object
        /// </summary>
        /// <param name="firstName">Firstname of employee object to instantiate</param>
        /// <param name="lastName">Lastname of employee object to instantiate</param>
        /// <returns></returns>
        private Employee ConstructEmployee(string firstName, string lastName)
        {
            Address homeAddress = new Address
            {
                AddressLine1 = "123 Mock Address",
                City = "Mock City",
                PostCode = "1234"
            };

            Employee employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = DateTime.Today.AddYears(-20),
                Address = homeAddress,
                Gender = Employee.GenderEnum.M,
                Title = "worker"
            };

            return employee;
        }
        #endregion
    }
}