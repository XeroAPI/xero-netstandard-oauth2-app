using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace XeroNetStandardApp.ViewComponents{
    public class SideBarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int maxPriority, bool isDone)
        {

            return View(TokenUtilities.TokenExists());
        }
    }
}