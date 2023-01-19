using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XeroNetStandardApp.IO;

namespace XeroNetStandardApp.ViewComponents{
    public class SideBarViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View(LocalStorageTokenIO.Instance.TokenExists()));
        }
    }
}