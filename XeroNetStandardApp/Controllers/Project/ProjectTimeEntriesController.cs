using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Project;
using Task = Xero.NetStandard.OAuth2.Model.Project.Task;

namespace XeroNetStandardApp.Controllers.Project
{
    /// <summary>
    /// Controller implementing the following Project Api endpoints
    /// - GET: Time Entries 
    /// - POST: Create Time Entry
    /// - PUT: Update Time Entry
    /// - DELETE: Delete Time Entry
    /// </summary>
    public class ProjectTimeEntriesController : ApiAccessorController<ProjectApi>
    {
        public ProjectTimeEntriesController(IOptions<XeroConfiguration> xeroConfig) : base(xeroConfig) { }

        #region GET Endpoints

        /// <summary>
        /// Get time entries for a given project
        /// </summary>
        /// <param name="projectId">Project id of project to get time entries for</param>
        /// <returns>Returns a list of time entries</returns>
        public async Task<IActionResult> GetTimeEntries(Guid projectId)
        {
            var selectedProject = await Api.GetProjectAsync(XeroToken.AccessToken, TenantId, projectId);
            var timeEntries = await Api.GetTimeEntriesAsync(XeroToken.AccessToken, TenantId, projectId);

            var timeEntryWrappers = new List<TimeEntryWrapper>();
            foreach (var timeEntry in timeEntries.Items)
            {
                var task = await Api.GetTaskAsync(XeroToken.AccessToken, TenantId, projectId, (Guid)timeEntry.TaskId);
                var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);
                var userName = "";
                foreach (var user in users.Items)
                {
                    if (user.UserId == timeEntry.UserId)
                    {
                        userName = user.Name;
                        break;
                    }
                }

                timeEntryWrappers.Add(
                    new TimeEntryWrapper
                    {
                        Data = timeEntry,
                        UserName = userName,
                        TaskName = task.Name
                    }
                );
            }

            ViewBag.jsonResponse = timeEntries.ToJson();
            return View((selectedProject, timeEntryWrappers));
        }


        /// <summary>
        /// Pass time entry properties to update time entry view
        /// </summary>
        /// <param name="projectId">Project id of project time entry to update is associated with</param>
        /// <param name="timeEntryId">Id of time entry to update</param>
        /// <returns>Redirects user to update time entry view</returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTimeEntry(Guid projectId, Guid timeEntryId)
        {
            var selectedTimeEntry = await Api.GetTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId);
            var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);
            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);

            return View("UpdateTimeEntry", model: new UpdateTimeEntryModel { Users = users.Items, Tasks = tasks.Items, TimeEntry = selectedTimeEntry });
        }

        /// <summary>
        /// Delete time entry
        /// </summary>
        /// <param name="projectId">Project id of project time entry to update is associated with</param>
        /// <param name="timeEntryId">Id of time entry to update</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DeleteTimeEntry(Guid projectId, Guid timeEntryId)
        {
            await Api.DeleteTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId);

            // Delay to ensure change reflects on get projects page
            await System.Threading.Tasks.Task.Delay(300);

            return RedirectToAction("GetTimeEntries", new { projectId });
        }

        /// <summary>
        /// Pass project Id to create time entry view
        /// </summary>
        /// <param name="projectId">Project id created view will be associated with</param>
        /// <returns>Redirects user to create view page</returns>
        [HttpGet]
        public async Task<IActionResult> CreateTimeEntry(Guid projectId)
        {

            var tasks = await Api.GetTasksAsync(XeroToken.AccessToken, TenantId, projectId);
            var users = await Api.GetProjectUsersAsync(XeroToken.AccessToken, TenantId);

            return View("CreateTimeEntry", new CreateTimeEntryModel { ProjectId = projectId, Tasks = tasks.Items, Users = users.Items });
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Update an existing time entry
        /// </summary>
        /// <param name="projectId">Project id of project time entry to update is associated with</param>
        /// <param name="timeEntryId">Id of time entry to update</param>
        /// <param name="userId">New user id to update time entry</param>
        /// <param name="taskId">New task id to update time entry</param>
        /// <param name="duration">New duration to update time entry</param>
        /// <returns>Redirects user to view time entries page</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTimeEntry(Guid projectId, Guid timeEntryId, Guid userId, Guid taskId, int duration)
        {

            var updatedTimeEntry = new TimeEntryCreateOrUpdate
            {
                UserId = userId,
                Duration = duration,
                TaskId = taskId
            };

            // Delay to ensure change reflects on get projects page
            await System.Threading.Tasks.Task.Delay(300);

            await Api.UpdateTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, timeEntryId, updatedTimeEntry);

            return RedirectToAction("GetTimeEntries", new { projectId });
        }

        /// <summary>
        /// Create new time entry 
        /// </summary>
        /// <param name="projectId">Project id to associate new time entry with</param>
        /// <param name="userId">User id to associate new time entry with</param>
        /// <param name="taskId">Task id to associate new time entry with</param>
        /// <param name="duration">Duration of new time entry</param>
        /// <returns>Redirects user to get time entries page</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTimeEntry(Guid projectId, Guid userId, Guid taskId, int duration)
        {
            var newTimeEntry = new TimeEntryCreateOrUpdate
            {
                UserId = userId,
                Duration = duration,
                TaskId = taskId
            };

            await Api.CreateTimeEntryAsync(XeroToken.AccessToken, TenantId, projectId, newTimeEntry);

            // Delay to ensure change reflects on get projects page
            await System.Threading.Tasks.Task.Delay(300);

            return RedirectToAction("GetTimeEntries", new { projectId });
        }

        #endregion

    }

    #region Models

    /// <summary>
    /// Have to define model for PUT update time entry due to dynamic
    /// models not supporting anonymous type in Razor; returns
    /// RuntimeBinderException 
    /// </summary>
    public class CreateTimeEntryModel
    {
        public List<Task> Tasks { get; set; }
        public List<ProjectUser> Users { get; set; }
        public Guid ProjectId { get; set; }
    }

    /// <summary>
    /// Have to define model for PUT update time entry due to dynamic
    /// models not supporting anonymous type in Razor; returns
    /// RuntimeBinderException 
    /// </summary>
    public class UpdateTimeEntryModel
    {
        public List<ProjectUser> Users { get; set; }
        public List<Task> Tasks { get; set; }
        public TimeEntry TimeEntry { get; set; }
    }

    /// <summary>
    /// Custom wrapper to expose task name and user name in single object
    /// </summary>
    public class TimeEntryWrapper
    {
        public TimeEntry Data { get; set; }
        public string TaskName { get; set; }
        public string UserName { get; set; }
    }

    #endregion
}
