using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;

namespace XeroNetStandardApp.Controllers
{
    public class IdentityInfo : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;

    public IdentityInfo(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /IdentityInfo/
    public async Task<ActionResult> Index()
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);

      var IdentityApi = new IdentityApi();
      var response = await IdentityApi.GetConnectionsAsync(accessToken);
      
      foreach(var connection in response){
        ViewBag.jsonResponse += connection.ToJson();
      }

      var connections = response;

      return View(connections);
    }

    // GET: /Contacts#Delete
    [HttpGet]
    public async Task<ActionResult> Delete(string connectionId)
    {
      // Authentication   
      var client = new XeroClient(XeroConfig.Value);
      var accessToken = await TokenUtilities.GetCurrentAccessToken(client);

      Guid connectionIdGuid = Guid.Parse(connectionId);

      var IdentityApi = new IdentityApi();
      await IdentityApi.DeleteConnectionAsync(accessToken, connectionIdGuid);
      
      return RedirectToAction("Index", "IdentityInfo");
    }
  }
}