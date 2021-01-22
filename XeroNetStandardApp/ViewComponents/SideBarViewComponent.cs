using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace XeroNetStandardApp.ViewComponents{
  public class SideBarViewComponent : ViewComponent
  {
#pragma warning disable CS1998 // This async method lacks 'await' operators
    public async Task<IViewComponentResult> InvokeAsync(int maxPriority, bool isDone)
    {
    
      return View(TokenUtilities.TokenExists());
    }
#pragma warning restore CS1998 // This async method lacks 'await' operators
  }
}