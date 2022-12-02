using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.PayrollUk;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing methods demonstrating following PayrollUK endpoints:
    /// <para>- GET: /UkEmployeesInfo#Index</para>
    /// <para>- POST: /UkEmployeesInfo#Create</para>
    /// </summary>
    public class UkEmployeesInfo : ApiAccessorController<PayrollUkApi>
    {
        public UkEmployeesInfo(IOptions<XeroConfiguration> xeroConfig):base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// GET: /UkEmployeesInfo#Index
        /// </summary>
        /// <returns>Returns a list of employees</returns>
        public async Task<ActionResult> Index()
        {
            // Call get employees endpoint
            var response = await Api.GetEmployeesAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response._Employees);
        }

        /// <summary>
        /// GET: /UkEmployeesInfo#Create
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
        /// POST: /UkEmployeesInfo#Create
        /// </summary>
        /// <param name="firstName">Firstname of new employee to create</param>
        /// <param name="lastName">Lastname of new employee to create</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(string firstName, string lastName)
        {
            // Call create employee endpoint
            await Api.CreateEmployeeAsync(XeroToken.AccessToken, TenantId, ConstructEmployee(firstName, lastName));

            return RedirectToAction("Index", "UkEmployeesInfo");
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