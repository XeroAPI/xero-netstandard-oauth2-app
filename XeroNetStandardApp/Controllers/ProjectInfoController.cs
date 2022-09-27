using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
// using Xero.NetStandard.OAuth2.Model.Accounting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Project;
using Xero.NetStandard.OAuth2.Token;

namespace XeroNetStandardApp.Controllers
{
  // For the two example uses only
  public class projectTaskExampleClass
  {
    public List<string> projectNames = new List<string>();
    public List<string> taskNames = new List<string>();
    public List<Guid> projectIds = new List<Guid>();
    public Guid lastSelectedProjectId = new Guid();
  }

  public class ProjectInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public ProjectInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    public static projectTaskExampleClass projectTaskExampleObject = new projectTaskExampleClass();

    /*===============================================================================================\\
    ||                                   Below is for Project                                        ||
    \\===============================================================================================*/

    // GET: /Project/
    public async Task<ActionResult> ProjectIndex()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var response = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      var projects = response.Items;
      ViewBag.jsonResponse = response.ToJson();

      return View(projects);
    }

    // GET: /Project #Create
    [HttpGet]
    public async Task<IActionResult> ProjectCreate()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var accountingApi = new AccountingApi();
      var contacts = await accountingApi.GetContactsAsync(accessToken, xeroTenantId);

      List<string> contactIds = new List<string>();
      foreach(Xero.NetStandard.OAuth2.Model.Accounting.Contact contact in contacts._Contacts)
      {
        contactIds.Add(contact.ContactID.ToString());
      }
      return View(contactIds);
    }

    // POST: /Project #Create
    [HttpPost]
    public async Task<ActionResult> ProjectCreate(string contactId, string name, string estimateAmount)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;

      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      DateTime deadlineUtc = DateTime.Today.AddDays(10);

      Amount projectAmount = new Amount() {
        Currency = CurrencyCode.AUD,
        Value = Decimal.Parse(estimateAmount)
      };

      var project = new ProjectCreateOrUpdate() {
        Name = name,
        EstimateAmount = Decimal.Parse(estimateAmount),
        ContactId = Guid.Parse(contactId),
        DeadlineUtc = deadlineUtc
      };

      var ProjectApi = new ProjectApi();
      var response = await ProjectApi.CreateProjectAsync(accessToken, xeroTenantId, project);

      return RedirectToAction("ProjectIndex", "ProjectInfo");
    }
  

    // GET: /Project #Update
    [HttpGet]
    public async Task<IActionResult> ProjectUpdate()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      List<string> projectNames = new List<string>();
      foreach(Project project in projects.Items)
      {
        projectNames.Add(project.Name);
      }
      return View(projectNames);
    }

    // PUT: /Project #Update
    [HttpPost]
    public async Task<ActionResult> ProjectUpdate(string projectOldName, string newName, DateTime newDeadline, string newEstimateAmount)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      Amount projectAmount = new Amount() {
        Currency = CurrencyCode.AUD,
        Value = Decimal.Parse(newEstimateAmount)
      };
      
      // First find which project was selected and retrieve its Guid
      var ProjectApi = new ProjectApi();
      var projectsList = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      Project projectToBeUpdate = new Project();
      Guid oldProjectId = new Guid();

      foreach(Project oldProject in projectsList.Items)
      {
        if(Equals(projectOldName, oldProject.Name))
        {
          projectToBeUpdate = oldProject;
          oldProjectId = projectToBeUpdate.ProjectId.Value;
          break;
        }
      }

      var project = new ProjectCreateOrUpdate() {
        Name = newName,
        EstimateAmount = Decimal.Parse(newEstimateAmount),
        ContactId = projectToBeUpdate.ContactId,
        DeadlineUtc = newDeadline
      };

      await ProjectApi.UpdateProjectAsync(accessToken, xeroTenantId, oldProjectId, project);

      // Wait a second for the update made to be registered
      System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Run( () => System.Threading.Thread.Sleep(1000));

      return RedirectToAction("ProjectIndex", "ProjectInfo");
    }


    // GET: /Project #Patch
    [HttpGet]
    public async Task<IActionResult> ProjectPatch()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);
      List<string> projectNames = new List<string>();
      foreach(Project project in projects.Items)
      {
        projectNames.Add(project.Name);
      }

      return View(projectNames);
    }

    // PUT: /Project #Patch
    [HttpPost]
    public async Task<ActionResult> ProjectPatch(string projectName, string projectStatusChoice)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projectsList = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      Project projectToBeUpdate = new Project();
      Guid projectId = new Guid();
      var projectPatch = new ProjectPatch();
      Boolean stateChanged = false;

      foreach(Project project in projectsList.Items)
      {
        if(Equals(projectName, project.Name))
        {
          projectToBeUpdate = project;
          projectId = projectToBeUpdate.ProjectId.Value;

          ProjectStatus status;

          if (Enum.TryParse(projectStatusChoice, out status))
          {
            switch (status)
            {
              // Inprogress status was selected
              case ProjectStatus.INPROGRESS:
                if(projectToBeUpdate.Status != ProjectStatus.INPROGRESS)
                {
                  projectPatch.Status = ProjectStatus.INPROGRESS;
                  stateChanged = true;
                }
              break;

              case ProjectStatus.CLOSED:
                if(projectToBeUpdate.Status != ProjectStatus.CLOSED)
                {
                  projectPatch.Status = ProjectStatus.CLOSED;
                  stateChanged = true;
                }
              break;

              default:
              break;
            }
          }
          else 
          {
              /* invalid enum value, handle */
          }
          break;
        }
      }

      if(stateChanged)
      {
        await ProjectApi.PatchProjectAsync(accessToken, xeroTenantId, projectId, projectPatch);
        // Wait 3 seconds for the update made to be registered
        System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Run( () => System.Threading.Thread.Sleep(3000));
        stateChanged = false;
      }

      return RedirectToAction("ProjectIndex", "ProjectInfo");
    }


    /*===============================================================================================\\
    ||                                  Below is for Project Tasks                                   ||
    \\===============================================================================================*/

    // GET: /TaskInfo/
    [HttpGet]
    public async Task<ActionResult> TaskIndex()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      List<string> projectNames = new List<string>();
      foreach(Project project in projects.Items)
      {
        projectNames.Add(project.Name);
      }
      return View(projectNames);
    }

    [HttpPost]
    public async Task<ActionResult> TaskIndex(string projectName)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      // First find which project was selected and retrieve its Guid
      var ProjectApi = new ProjectApi();
      var projectsList = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      Project projectToShowTask = new Project();
      Guid examinedProjectId = new Guid();
      List<string> projectNames = new List<string>();

      foreach(Project project in projectsList.Items)
      {
        projectNames.Add(project.Name);

        // Look for the project ID of the project selected
        if(Equals(projectName, project.Name))
        {
          projectToShowTask = project;
          examinedProjectId = projectToShowTask.ProjectId.Value;
        }
      }

      var response = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, examinedProjectId);

      var tasks = response.Items;
      ViewBag.jsonResponse = response.ToJson();

      return View("TaskIndex", projectNames);
    }


    // GET: /Project Task #Create
    [HttpGet]
    public async Task<IActionResult> TaskCreate()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      List<string> projectNames = new List<string>();
      foreach(Project project in projects.Items)
      {
        projectNames.Add(project.Name);
      }
      return View(projectNames);
    }


    // POST: /Project Task #Create
    [HttpPost]
    public async Task<ActionResult> TaskCreate(string projectName, string taskName, string taskRate, string estimateMinute, string taskChargeType)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      // First find which project was selected and retrieve its Guid
      var ProjectApi = new ProjectApi();
      var projectsList = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);
      List<string> projectNames = new List<string>();

      Guid projectId = new Guid();

      foreach(Project project in projectsList.Items)
      {
        projectNames.Add(project.Name);

        if(Equals(projectName, project.Name))
        {
          projectId = project.ProjectId.Value;
        }
      }

      Amount task_rate = new Amount() {
        Currency = CurrencyCode.AUD,
        Value = Decimal.Parse(taskRate)
      };

      var task = new TaskCreateOrUpdate() {
        Name = taskName,
        Rate = task_rate,
        EstimateMinutes = int.Parse(estimateMinute),
        ChargeType = (ChargeType) Enum.Parse(typeof(ChargeType), taskChargeType)
      };

      await ProjectApi.CreateTaskAsync(accessToken, xeroTenantId, projectId, task);

      return RedirectToAction("TaskIndex", "ProjectInfo");
    }

    
    // GET: /Project Task #Delete
    [HttpGet]
    public async Task<IActionResult> TaskDelete()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      // Reset the saved static values
      projectTaskExampleObject.projectNames.Clear();
      projectTaskExampleObject.taskNames.Clear();
      projectTaskExampleObject.projectIds.Clear();
      projectTaskExampleObject.lastSelectedProjectId = Guid.Empty;

      // Populate the list of projects retrieved
      foreach(Project project in projects.Items)
      {
        projectTaskExampleObject.projectNames.Add(project.Name);
        projectTaskExampleObject.projectIds.Add(project.ProjectId.Value);
      }

      // Check if there is any project
      if(projectTaskExampleObject.projectNames.Count == 0)
      {
        projectTaskExampleObject.projectNames.Add("No Projects");
        projectTaskExampleObject.taskNames.Add("No Tasks");
        return View(projectTaskExampleObject);
      }

      // The initial project ID is the last selected project ID
      projectTaskExampleObject.lastSelectedProjectId = projectTaskExampleObject.projectIds[0];
      
      // Get tasks for this initial project
      var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);
        
      // Populate the list of projects retrieved
      foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
        projectTaskExampleObject.taskNames.Add(task.Name);

      if(projectTaskExampleObject.taskNames.Count == 0)
        projectTaskExampleObject.taskNames.Add("No Tasks");

      return View(projectTaskExampleObject);
    }


    // POST: /Project Task #Delete project
    [HttpPost]
    public async Task<ActionResult> TaskDelete(string projectName, string taskName)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }


      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      int numberOfProjects = 0;
      int selectedProjectPosition = 0;

      // Project selected to refresh tasks
      if(taskName == null)
      {
        // Reset the UI dropdown list for project
        projectTaskExampleObject.projectNames.Clear();

        foreach(Project project in projects.Items)
        {
          numberOfProjects++;

          projectTaskExampleObject.projectNames.Add(project.Name);

          if(Equals(projectName, project.Name))
          {
            selectedProjectPosition = numberOfProjects;

            // Save this selected project ID
            projectTaskExampleObject.lastSelectedProjectId = project.ProjectId.Value;

            var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, project.ProjectId.Value);

            // Reset the UI dropdown list for task
            projectTaskExampleObject.taskNames.Clear();

            // Populate the list of tasks retrieved
            foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
              projectTaskExampleObject.taskNames.Add(task.Name);

            if(projectTaskExampleObject.taskNames.Count == 0)
              projectTaskExampleObject.taskNames.Add("No Tasks");
          }
        }

        if(selectedProjectPosition == 1)
          // First project in the list was selected, no need for reorder
          return View(projectTaskExampleObject);
        else
        {
          // Reorder the project list and bring the selected one forward
          for(int i = selectedProjectPosition; i > 1; i--)
          {
            projectTaskExampleObject.projectNames[i-1] = projectTaskExampleObject.projectNames[i-2];
          }
          projectTaskExampleObject.projectNames[0] = projectName;

          return View(projectTaskExampleObject);
        }
      }

      // Task selected to delete
      if(projectName == null)
      {
        // Reset the UI dropdown list for task
        projectTaskExampleObject.taskNames.Clear();

        var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);

        foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
        {
          if(Equals(taskName, task.Name))
          {
            await ProjectApi.DeleteTaskAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId, task.TaskId.Value);
            
            // After deleting task, reset the saved static list
            projectTaskExampleObject.taskNames.Clear();
            break;
          }
        }
        
        // Wait 3 seconds for the update made to be registered
        System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Run( () => System.Threading.Thread.Sleep(3000));

        // Retrieve new list of tasks
        tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);

        // Populate new list of tasks retrieved
        foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
          projectTaskExampleObject.taskNames.Add(task.Name);

        return View(projectTaskExampleObject);
      }

      return View(projectTaskExampleObject);
    }













    // GET: /Project Task #Update
    [HttpGet]
    public async Task<IActionResult> TaskUpdate()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      // Reset the saved static values
      projectTaskExampleObject.projectNames.Clear();
      projectTaskExampleObject.taskNames.Clear();
      projectTaskExampleObject.projectIds.Clear();
      projectTaskExampleObject.lastSelectedProjectId = Guid.Empty;

      // Populate the list of projects retrieved
      foreach(Project project in projects.Items)
      {
        projectTaskExampleObject.projectNames.Add(project.Name);
        projectTaskExampleObject.projectIds.Add(project.ProjectId.Value);
      }

      // Check if there is any project
      if(projectTaskExampleObject.projectNames.Count == 0)
      {
        projectTaskExampleObject.projectNames.Add("No Projects");
        projectTaskExampleObject.taskNames.Add("No Tasks Can't Update");
        return View(projectTaskExampleObject);
      }

      // The initial project ID is the last selected project ID
      projectTaskExampleObject.lastSelectedProjectId = projectTaskExampleObject.projectIds[0];
      
      // Get tasks for this initial project
      var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);
        
      // Populate the list of projects retrieved
      foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
        projectTaskExampleObject.taskNames.Add(task.Name);

      if(projectTaskExampleObject.taskNames.Count == 0)
        projectTaskExampleObject.taskNames.Add("No Tasks Can't Update");

      return View(projectTaskExampleObject);
    }


    // POST: /Project Task #Delete project
    [HttpPost]
    public async Task<ActionResult> TaskUpdate(string projectName, string taskName, string newTaskName, string taskRate, string estimateMinute, string taskChargeType)
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }


      var ProjectApi = new ProjectApi();
      var projects = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      int numberOfProjects = 0;
      int selectedProjectPosition = 0;

      // Project selected to refresh tasks
      if(newTaskName == null)
      {
        // Reset the UI dropdown list for project
        projectTaskExampleObject.projectNames.Clear();

        foreach(Project project in projects.Items)
        {
          numberOfProjects++;

          projectTaskExampleObject.projectNames.Add(project.Name);

          if(Equals(projectName, project.Name))
          {
            selectedProjectPosition = numberOfProjects;

            // Save this selected project ID
            projectTaskExampleObject.lastSelectedProjectId = project.ProjectId.Value;

            var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, project.ProjectId.Value);

            // Reset the UI dropdown list for task
            projectTaskExampleObject.taskNames.Clear();

            // Populate the list of tasks retrieved
            foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
              projectTaskExampleObject.taskNames.Add(task.Name);

            if(projectTaskExampleObject.taskNames.Count == 0)
              projectTaskExampleObject.taskNames.Add("No Tasks Can't Update");
          }
        }

        if(selectedProjectPosition == 1)
          // First project in the list was selected, no need for reorder
          return View(projectTaskExampleObject);
        else
        {
          // Reorder the project list and bring the selected one forward
          for(int i = selectedProjectPosition; i > 1; i--)
          {
            projectTaskExampleObject.projectNames[i-1] = projectTaskExampleObject.projectNames[i-2];
          }
          projectTaskExampleObject.projectNames[0] = projectName;

          return View(projectTaskExampleObject);
        }       
      }

      // Task selected to delete
      if(projectName == null)
      {
        // Reset the UI dropdown list for task
        projectTaskExampleObject.taskNames.Clear();

        var tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);

        foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
        {
          if(Equals(taskName, task.Name))
          {
            Amount task_rate = new Amount()
            {
              Currency = CurrencyCode.AUD,
              Value = Decimal.Parse(taskRate)
            };

            var newTask = new TaskCreateOrUpdate()
            {
              Name = newTaskName,
              Rate = task_rate,
              EstimateMinutes = int.Parse(estimateMinute),
              ChargeType = (ChargeType) Enum.Parse(typeof(ChargeType), taskChargeType)
            };
            
            await ProjectApi.UpdateTaskAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId, task.TaskId.Value, newTask);
            
            // After deleting task, reset the saved static list
            projectTaskExampleObject.taskNames.Clear();
            break;
          }
        }
        
        // Wait 3 seconds for the update made to be registered
        System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Run( () => System.Threading.Thread.Sleep(3000));

        // Retrieve new list of tasks
        tasks = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, projectTaskExampleObject.lastSelectedProjectId);

        // Populate new list of tasks retrieved
        foreach(Xero.NetStandard.OAuth2.Model.Project.Task task in tasks.Items)
          projectTaskExampleObject.taskNames.Add(task.Name);

        return View(projectTaskExampleObject);
      }

      return View(projectTaskExampleObject);
    }
  }
}