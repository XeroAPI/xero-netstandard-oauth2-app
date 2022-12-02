using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace XeroNetStandardApp.ViewComponents
{
    public class NavBarViewComponent : ViewComponent
    {
        public class TenantDetails
        {
            public string TenantName { get; set; }
            public Guid TenantId { get; set; }
        }

        public async Task<IViewComponentResult> InvokeAsync(int maxPriority, bool isDone)
        {
            var xeroToken = TokenUtilities.GetStoredToken();

            var tenantId = TokenUtilities.GetCurrentTenantId();
            try
            {
                ViewBag.OrgPickerCurrentTenantId = tenantId;
                ViewBag.OrgPickerTenantList = xeroToken.Tenants.Select(
                    (t) => new TenantDetails { TenantName = t.TenantName, TenantId = t.TenantId }
                ).ToList();
            }
            catch (Exception)
            {

            }

            return View(TokenUtilities.TokenExists());
        }

    }

}

