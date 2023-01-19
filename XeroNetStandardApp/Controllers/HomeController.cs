using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using XeroNetStandardApp.IO;
using XeroNetStandardApp.Models;

namespace XeroNetStandardApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromQuery] Guid? tenantId)
        {
            var tokenIO = LocalStorageTokenIO.Instance;
            if (tenantId != null) tokenIO.StoreTenantId(tenantId.ToString());
            
            return View(tokenIO.TokenExists());
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
