using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class OrgAccountingController : Controller
{
    AccountingApi AccountingApi = new AccountingApi();

    public OrgAccountingController(IOptions<Configuration> xeroConfig) : base() { }

    /// <summary>
    /// GET: /GetAccounts
    /// </summary>
    /// <returns>Returns list of accounts</returns>
    public async Task<IActionResult> Index()
    {
        var xeroToken = "Bearer_Token";
        var TenantId = "Tenant_id";  // string | Xero identifier for Tenant

        Configuration config = new Configuration();
        config.BasePath = "https://api.xero.com/api.xro/2.0";
        config.AccessToken = xeroToken;

        AccountingApi = new AccountingApi(config);
        var response = await AccountingApi.GetAccountsAsync(TenantId);

        return Json(response);
    }
}
