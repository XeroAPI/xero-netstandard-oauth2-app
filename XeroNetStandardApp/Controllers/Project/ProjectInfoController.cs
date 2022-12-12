using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using System.Collections.Generic;
using System;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = System.Threading.Tasks.Task;

namespace XeroNetStandardApp.Controllers
{
    /// <summary>
    /// Controller implementing following ProjectApi endpoints:
    /// <para>- GET: /Projects/</para>
    /// </summary>
    public class ProjectInfoController : ApiAccessorController<ProjectApi>
    {
        public ProjectInfoController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig){}

        #region GET Endpoints

        /// <summary>
        /// Get a list of projects
        /// <para>GET: /Projects/</para>
        /// </summary>
        /// <returns>Returns a list of projects</returns>
        public async Task<IActionResult> GetProjects()
        {
            var response = await Api.GetProjectsAsync(XeroToken.AccessToken, TenantId);

            ViewBag.jsonResponse = response.ToJson();
            return View(response.Items);
        }

        /// <summary>
        /// Get list of contact ids for create project page
        /// </summary>
        /// <returns>Returns a list of tuples containing contact names and contact ids</returns>
        [HttpGet]
        public async Task<IActionResult> CreateProject()
        {
            var accountingApi = new AccountingApi();
            var contacts = await accountingApi.GetContactsAsync(XeroToken.AccessToken, TenantId);

            var contactInfo = new List<(string, string)>();
            contacts._Contacts.ForEach(contact => contactInfo.Add(
                (
                    contact.Name,
                    contact.ContactID.ToString()
                )
            ));

            return View(contactInfo);
        }

        /// <summary>
        /// Get list of project names for update project page
        /// </summary>
        /// <returns>Returns a list of project names</returns>
        [HttpGet]
        public async Task<IActionResult> UpdateProject(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            return View(selectedProject);
        }

        #endregion


        /// <summary>
        /// Create a new project
        /// <para>POST: /Project #Create</para>
        /// </summary>
        /// <param name="contactId">Contact id to associate project with</param>
        /// <param name="name">Name of the project</param>
        /// <param name="estimateAmount">Estimated cost of project</param>
        /// <returns>Redirects user to get projects page</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject(string contactId, string name, string estimateAmount)
        {
            var newProject = new ProjectCreateOrUpdate
            {
                Name = name,
                EstimateAmount = decimal.Parse(estimateAmount),
                ContactId = Guid.Parse(contactId),
                DeadlineUtc = DateTime.Today.AddDays(10)
            };

            await Api.CreateProjectAsync(XeroToken.AccessToken, TenantId, newProject);
            
            // Delay to ensure change reflects on get projects page
            await Task.Delay(300);

            return RedirectToAction("GetProjects");
        }

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="projectId">Project id of project to update</param>
        /// <param name="name">New name for project</param>
        /// <param name="estimateAmount">New estimate amount for project</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProject(string projectId, string name, string estimateAmount)
        {
            var updatedProject = new ProjectCreateOrUpdate
            {
                Name = name,
                EstimateAmount = decimal.Parse(estimateAmount)
            };

            await Api.UpdateProjectAsync(XeroToken.AccessToken, TenantId, Guid.Parse(projectId), updatedProject);

            // Delay to ensure change reflects on get projects page
            await Task.Delay(300);

            return RedirectToAction("GetProjects");
        }
    }
}
