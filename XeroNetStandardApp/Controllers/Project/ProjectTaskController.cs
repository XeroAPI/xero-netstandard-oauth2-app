using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = System.Threading.Tasks.Task;

namespace XeroNetStandardApp.Controllers.Project
{
    /// <summary>
    /// Controller implementing the following Project Api endpoints
    /// - GET: Tasks
    /// - POST: Create Task
    /// - PUT: Update Task
    /// - DELETE: Delete Task
    /// </summary>
    public class ProjectTaskController : ApiAccessorController<ProjectApi>
    {
        public ProjectTaskController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig) { }

        /// <summary>
        /// Get list of tasks for a particular project
        /// </summary>
        /// <param name="projectId">Id of project to retrieve tasks for</param>
        /// <returns>Returns a list of tasks</returns>
        public async Task<IActionResult> GetTasks(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);

            return View((selectedProject.Name, tasks)); 
        }

        /// <summary>
        /// Delete task associated with a particular project
        /// </summary>
        /// <param name="projectId">The project the task to be deleted is associated with</param>
        /// <param name="taskId">Specific task to be deleted</param>
        /// <returns>Redirects user to get tasks page</returns>
        [HttpGet]
        public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
        {
            await Api.DeleteTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId);

            // Delay to ensure change reflects on get projects page
            await Task.Delay(300);

            return RedirectToAction("GetTasks", new { projectId });
        }

        [HttpGet]
        public IActionResult CreateTask(Guid projectId, CurrencyCode currencyCode)
        {
            return View("CreateTask",new CreateTaskGetModel { ProjectId = projectId, CurrencyCode = currencyCode });
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        /// <param name="projectId">Project id of project task will be associated with</param>
        /// <param name="currencyCode">Currency code of task</param>
        /// <param name="taskName">Name of new task</param>
        /// <param name="taskRate">Task rate of new task</param>
        /// <param name="estimateMinute">Estimated minute of new task</param>
        /// <param name="taskChargeType">Charge type of new task</param>
        /// <returns>Redirects user to get tasks page</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTask(Guid projectId, CurrencyCode currencyCode, string taskName, string taskRate, string estimateMinute, string taskChargeType)
        {
            var newTask = new TaskCreateOrUpdate
            {
                Name = taskName,
                Rate = new Amount{Currency = currencyCode, Value = decimal.Parse(taskRate)},
                EstimateMinutes = int.Parse(estimateMinute),
                ChargeType = (ChargeType)Enum.Parse(typeof(ChargeType), taskChargeType)
            };

            await Api.CreateTaskAsync(XeroToken.AccessToken, TenantId, projectId, newTask);

            return RedirectToAction("GetTasks", new { projectId });
        }

        /// <summary>
        /// Returns task object to be consumed by update task view
        /// </summary>
        /// <param name="projectId">Project id task is associated with</param>
        /// <param name="taskId">Id of task to update</param>
        /// <returns>Redirects user to update task view</returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId)
        {
            var selectedTask = await Api.GetTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId);

            return View("UpdateTask", selectedTask);
        }

        /// <summary>
        /// Update properties of specified task
        /// </summary>
        /// <param name="projectId">Project task is associated with</param>
        /// <param name="taskId">Id of task to modify</param>
        /// <param name="taskName">New name for task</param>
        /// <param name="taskRate">New rate for task</param>
        /// <param name="estimateMinute">New estimated minute for task</param>
        /// <param name="taskChargeType">New charge type of task</param>
        /// <returns>Redirects user to get tasks page</returns>
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, string taskName, string taskRate, string estimateMinute, string taskChargeType)
        {
            // Get associated project so we can retrieve the estimate currency (not task don't have to have the same currency as project)
            var associatedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);

            var updatedTask = new TaskCreateOrUpdate
            {
                Name = taskName,
                Rate = new Amount { Currency = associatedProject.Estimate.Currency, Value = decimal.Parse(taskRate) },
                EstimateMinutes = int.Parse(estimateMinute),
                ChargeType = (ChargeType)Enum.Parse(typeof(ChargeType), taskChargeType)
            };

            await Api.UpdateTaskAsync(XeroToken.AccessToken, TenantId, projectId, taskId, updatedTask);

            return RedirectToAction("GetTasks", new { projectId });
        }

    }

    /// <summary>
    /// Have to define model for GET create task due to dynamic
    /// models not supporting anonymous type in Razor; returns
    /// RuntimeBinderException 
    /// </summary>
    public class CreateTaskGetModel
    {
        public Guid ProjectId { get; set; }
        public CurrencyCode CurrencyCode { get; set; }
    }
}
