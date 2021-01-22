using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using XeroNetStandardApp.Models;

namespace XeroNetStandardApp.Controllers
{
  public class HomeController : Controller
  {
    public IActionResult Index([FromQuery] Guid? tenantId)
    {
      bool firstTimeConnection = false;

      if (TokenUtilities.TokenExists())
      {
        firstTimeConnection = true;
      }

      if (tenantId is Guid tenantIdValue)
      {
        TokenUtilities.StoreTenantId(tenantIdValue);
      }

      return View(firstTimeConnection);
    }
    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}


