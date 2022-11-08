using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.PayrollAu;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following AU employee endpoints:
    /// </summary>
    public class AuEmployeesInfo : Controller
    {
        private readonly IOptions<XeroConfiguration> _xeroConfig;
        private readonly PayrollAuApi _payrollAuApi;

        public AuEmployeesInfo(IOptions<XeroConfiguration> xeroConfig)
        {
            _xeroConfig = xeroConfig;
            _payrollAuApi = new PayrollAuApi();
        }

        #region GET Endpoints

        /// <summary>
        /// GET: /AuEmployeesInfo#Index
        /// </summary>
        /// <returns>Returns a list of AU employees</returns>
        public async Task<ActionResult> Index()
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Call get employees AU endpoint
            var response = await _payrollAuApi.GetEmployeesAsync(xeroToken.AccessToken, xeroTenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._Employees);
        }

        /// <summary>
        /// GET: /AuEmployeesInfo#Create
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
        /// POST: /AuEmployeesInfo#Create
        /// </summary>
        /// <param name="firstName">Firstname of employee to create</param>
        /// <param name="lastName">Lastname of employee to create</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(string firstName, string lastName)
        {
            // Token and TenantId setup
            var xeroToken = await TokenUtilities.GetXeroOAuth2Token(_xeroConfig.Value);
            var xeroTenantId = TokenUtilities.GetXeroTenantId(xeroToken);

            // Construct employee object
            var employees = new List<Employee> { ConstructEmployee(firstName, lastName) };

            // Call create employee endpoint
            await _payrollAuApi.CreateEmployeeAsync(xeroToken.AccessToken, xeroTenantId, employees);

            return RedirectToAction("Index", "AuEmployeesInfo");
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
            Employee employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = DateTime.Today.AddYears(-20),
                Gender = Employee.GenderEnum.M,
                Title = "worker"
            };

            return employee;
        }

        #endregion

    }
}