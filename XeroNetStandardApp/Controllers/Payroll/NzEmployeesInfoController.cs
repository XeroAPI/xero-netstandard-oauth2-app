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
    /// <para>- GET: /NzEmployeesInfo#Index</para>
    /// <para>- POST: /NzEmployeesInfo#Create</para>
    /// </summary>
    public class NzEmployeesInfo : ApiAccessorController<PayrollNzApi>
    {
        public NzEmployeesInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /NzEmployeesInfo#Index
        /// </summary>
        /// <returns>Returns list of NZ employees</returns>
        public async Task<IActionResult> Index()
        {
            // Call get employees endpoint
            var employees = await Api.GetEmployeesAsync(XeroToken.AccessToken, TenantId);
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
        public async Task<IActionResult> Create(string firstName, string lastName)
        {
            // Call create employee endpoint
            await Api.CreateEmployeeAsync(XeroToken.AccessToken, TenantId, ConstructEmployee(firstName, lastName));

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
            var homeAddress = new Address
            {
                AddressLine1 = "123 Mock Address",
                City = "Mock City",
                PostCode = "1234"
            };

            var employee = new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = DateTime.Today.AddYears(-20),
                Address = homeAddress,
                Gender = Employee.GenderEnum.M,

            };

            return employee;
        }
        #endregion
    }
}