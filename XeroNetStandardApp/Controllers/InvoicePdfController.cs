using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Linq;


namespace XeroNetStandardApp.Controllers
{
  public class InvoicePdf : Controller
  {

    private readonly ILogger<AuthorizationController> _logger;
    private readonly IOptions<XeroConfiguration> XeroConfig;
    public InvoicePdf(IOptions<XeroConfiguration> XeroConfig, ILogger<AuthorizationController> logger)
    {
      _logger = logger;
      this.XeroConfig = XeroConfig;
    }

    // GET: /InvoicePdf/
    public async Task<ActionResult> Index()
    {
      var xeroToken = TokenUtilities.GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
        var client = new XeroClient(XeroConfig.Value);
        xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
        TokenUtilities.StoreToken(xeroToken);
      }

      string accessToken = xeroToken.AccessToken;
      Guid tenantId = TokenUtilities.GetCurrentTenantId();
      string xeroTenantId;
      if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
      {
        xeroTenantId = tenantId.ToString();
      }
      else
      {
        var id = xeroToken.Tenants.First().TenantId;
        xeroTenantId = id.ToString();
        TokenUtilities.StoreTenantId(id);
      }

      Guid invoiceId = Guid.Parse("d5654fc7-be8e-42e5-802a-9f5ab9d2476a");

      var AccountingApi = new AccountingApi();

      var response = await AccountingApi.GetInvoiceAsPdfAsync(accessToken, xeroTenantId, invoiceId);
      ViewBag.jsonResponse = response.ToString();

      return new FileStreamResult(response, "application/pdf");

    }
  }
}