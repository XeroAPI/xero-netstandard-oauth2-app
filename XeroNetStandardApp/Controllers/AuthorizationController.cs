using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Models;
using System.Collections.Generic;
using System.Linq;

namespace XeroNetStandardApp.Controllers
{
  public class AuthorizationController : Controller
  {
    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    
    // GET /Authorization/
    public AuthorizationController(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;

    }

    public IActionResult Index()
    {
      var client = new XeroClient(XeroConfig.Value);

      var clientState = Guid.NewGuid().ToString(); 
      TokenUtilities.StoreState(clientState);

      return Redirect(client.BuildLoginUri(clientState));
    }

    // GET /Authorization/Callback
    public async Task<ActionResult> Callback(string code, string state)
    {
      var clientState = TokenUtilities.GetCurrentState();
      
      if (state != clientState) {
        return Content("Cross site forgery attack detected!");
      }
      

      var client = new XeroClient(XeroConfig.Value);
      var xeroToken = (XeroOAuth2Token)await client.RequestAccessTokenAsync(code);

      Console.WriteLine("xeroToken: {0}", xeroToken);

      List<Tenant> tenants = await client.GetConnectionsAsync(xeroToken);

      Tenant firstTenant = tenants[0];

      TokenUtilities.StoreToken(xeroToken);

      return RedirectToAction("Index", "OrganisationInfo");
    }

    // GET /Authorization/Disconnect
    public async Task<ActionResult> Disconnect()
    {      
      var client = new XeroClient(XeroConfig.Value);

      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Tenant xeroTenant = xeroToken.Tenants[0];

      await client.DeleteConnectionAsync(xeroToken, xeroTenant);

      TokenUtilities.DestroyToken();

      return RedirectToAction("Index", "Home");
    }

    //GET /Authorization/Revoke
    public async Task<ActionResult> Revoke()
    {      
      var client = new XeroClient(XeroConfig.Value);

      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;

      await client.RevokeAccessTokenAsync(xeroToken);

      TokenUtilities.DestroyToken();

      return RedirectToAction("Index", "Home");
    }
  }
}