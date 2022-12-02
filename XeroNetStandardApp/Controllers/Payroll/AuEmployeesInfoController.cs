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
    /// <para>- GET: /AuEmployeesInfo#Index</para>
    /// <para>- POST: /AuEmployeesInfo#Create</para>
    /// </summary>
    public class AuEmployeesInfo : ApiAccessorController<PayrollAuApi>
    {
        public AuEmployeesInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /AuEmployeesInfo#Index
        /// </summary>
        /// <returns>Returns a list of AU employees</returns>
        public async Task<ActionResult> Index()
        {
            // Call get employees AU endpoint
            var response = await Api.GetEmployeesAsync(XeroToken.AccessToken, TenantId);

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
            // Construct employee object
            var employees = new List<Employee> { ConstructEmployee(firstName, lastName) };

            // Call create employee endpoint
            await Api.CreateEmployeeAsync(XeroToken.AccessToken, TenantId, employees);

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