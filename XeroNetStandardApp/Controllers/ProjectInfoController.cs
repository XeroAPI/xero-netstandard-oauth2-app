using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Model.Project;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
// using Xero.NetStandard.OAuth2.Model.Accounting;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace XeroNetStandardApp.Controllers
{
  public class ProjectInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public ProjectInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }


    /*===============================================================================================\\
    ||                                   Below is for Project                                        ||
    \\===============================================================================================*/

    // GET: /ProjectInfo/
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

    // GET: /ProjectInfo#Create
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

    // POST: /ProjectInfo#Create
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
  

    // GET: /ProjectInfo#Update
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

    // PUT: /ProjectInfo#Update
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


    // GET: /ProjectInfo#Patch
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

    // PUT: /ProjectInfo#Patch
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
        // Wait a second for the update made to be registered
        System.Threading.Tasks.Task taskA = System.Threading.Tasks.Task.Run( () => System.Threading.Thread.Sleep(1000));
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

      var ProjectApi = new ProjectApi();
      var projectsList = await ProjectApi.GetProjectsAsync(accessToken, xeroTenantId);

      Project projectToShowTask = new Project();
      Guid examinedProjectId = new Guid();

      foreach(Project project in projectsList.Items)
      {
        // Look for the project ID of the project selected
        if(Equals(projectName, project.Name))
        {
          projectToShowTask = project;
          examinedProjectId = projectToShowTask.ProjectId.Value;
          break;
        }
      }

      var response = await ProjectApi.GetTasksAsync(accessToken, xeroTenantId, examinedProjectId);

      var tasks = response.Items;
      ViewBag.jsonResponse = response.ToJson();

      return View(tasks);
    }
  }
}