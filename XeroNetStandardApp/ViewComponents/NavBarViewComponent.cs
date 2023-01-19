using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using XeroNetStandardApp.IO;

namespace XeroNetStandardApp.ViewComponents
{
    public class NavBarViewComponent : ViewComponent
    {
        public class TenantDetails
        {
            public string TenantName { get; set; }
            public Guid TenantId { get; set; }
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            var tokenIO = LocalStorageTokenIO.Instance;

            var xeroToken = tokenIO.GetToken();
            var tenantId = Guid.Parse(tokenIO.GetTenantId());

            ViewBag.OrgPickerCurrentTenantId = tenantId;
            ViewBag.OrgPickerTenantList = xeroToken?.Tenants.Select(
                t => new TenantDetails { TenantName = t.TenantName, TenantId = t.TenantId })
                .ToList();
            

            return Task.FromResult<IViewComponentResult>(View(tokenIO.TokenExists()));
        }

    }

}

