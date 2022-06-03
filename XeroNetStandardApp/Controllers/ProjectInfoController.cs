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

    // GET: /ProjectInfo/
    public async Task<ActionResult> Index()
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
    public async Task<IActionResult> Create()
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
    public async Task<ActionResult> Create(string contactId, string name, string estimateAmount)
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

      return RedirectToAction("Index", "ProjectInfo");
    }
  
    // GET: /ProjectInfo#Update
    [HttpGet]
    public async Task<IActionResult> Update()
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
      List<string> projectIds = new List<string>();
      foreach(Xero.NetStandard.OAuth2.Model.Project.Project project in projects)
      {
        projectIds.Add(project.ProjectId.ToString());
      }
      return View(projectIds);
    }

    // PUT: /ProjectInfo#Update
    [HttpPost]
    public async Task<ActionResult> Update(string contactId, string projectId, string newName, DateTime newDeadline, string newEstimateAmount)
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

      Guid ProjectId = Guid.Parse(projectId);

      var project = new ProjectCreateOrUpdate() {
        Name = newName,
        EstimateAmount = Decimal.Parse(newEstimateAmount),
        ContactId = Guid.Parse(contactId),
        DeadlineUtc = newDeadline
      };

      var ProjectApi = new ProjectApi();
      var response = await ProjectApi.UpdateProjectAsync(accessToken, xeroTenantId, ProjectId, project);

      return RedirectToAction("Index", "ProjectInfo");
    }
  }
}