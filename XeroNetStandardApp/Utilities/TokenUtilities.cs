using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.DependencyInjection;

public static class TokenUtilities
{
  public static void StoreToken(XeroOAuth2Token xeroToken)
  {
    string serializedXeroToken = JsonSerializer.Serialize(xeroToken);
    System.IO.File.WriteAllText("./xerotoken.json", serializedXeroToken);
  }

  public static XeroOAuth2Token GetStoredToken()
  {
    string serializedXeroToken = System.IO.File.ReadAllText("./xerotoken.json");
    var xeroToken = JsonSerializer.Deserialize<XeroOAuth2Token>(serializedXeroToken);

    return xeroToken;
  }

  public static bool TokenExists()
  {
    string serializedXeroTokenPath = "./xerotoken.json";
    bool fileExist = File.Exists(serializedXeroTokenPath);

    return fileExist;
  }

  public static void DestroyToken()
  {
    string serializedXeroTokenPath = "./xerotoken.json";
    File.Delete(serializedXeroTokenPath);

    return;
  }

  private class TenantId
  {
    public Guid CurrentTenantId { get; set; }
  }

  public static void StoreTenantId(Guid tenantId)
  {
    string serializedXeroToken = JsonSerializer.Serialize(
      new TenantId { CurrentTenantId = tenantId }
    );
    System.IO.File.WriteAllText("./tenantid.json", serializedXeroToken);
  }

  public static Guid GetCurrentTenantId()
  {
    Guid id;
    try
    {
      string serializedIndexFile = System.IO.File.ReadAllText("./tenantid.json");
      id = JsonSerializer.Deserialize<TenantId>(serializedIndexFile).CurrentTenantId;
    }
    catch (IOException)
    {
      id = Guid.Empty;
    }

    return id;
  }
}