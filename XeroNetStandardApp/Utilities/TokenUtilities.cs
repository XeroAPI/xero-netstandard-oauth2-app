using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.DependencyInjection;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;

public static class TokenUtilities
{
  [Serializable]
  public struct State
  {  
    public string state {get; set;}
    public State(string state){
      this.state = state;
    }
  }


  public async static Task<XeroOAuth2Token> GetXeroOAuth2Token(XeroConfiguration xeroConfig)
  {
      var xeroToken = GetStoredToken();
      var utcTimeNow = DateTime.UtcNow;

      if (utcTimeNow > xeroToken.ExpiresAtUtc)
      {
          var client = new XeroClient(xeroConfig);
          xeroToken = (XeroOAuth2Token) await client.RefreshAccessTokenAsync(xeroToken);
          StoreToken(xeroToken);
      }

      return xeroToken;
  }

  public static void StoreToken(XeroOAuth2Token xeroToken)
  {
    string serializedXeroToken = JsonSerializer.Serialize(xeroToken);
    System.IO.File.WriteAllText("./xerotoken.json", serializedXeroToken);
  }

  public static XeroOAuth2Token GetStoredToken()
  {
    var xeroToken = new XeroOAuth2Token();
    
    try {
      string serializedXeroToken = System.IO.File.ReadAllText("./xerotoken.json");
      xeroToken = JsonSerializer.Deserialize<XeroOAuth2Token>(serializedXeroToken);

      return xeroToken;
    } catch (Exception) {
      
    }
    
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

  public static void StoreState(string state)
  {
    State currentState = new State(state);
    string serializedState = JsonSerializer.Serialize(currentState);
    System.IO.File.WriteAllText("./state.json", serializedState);
  }

  public static string GetCurrentState()
  {
    string state;
    try
    {
      string serializedIndexFile = System.IO.File.ReadAllText("./state.json");
      state = JsonSerializer.Deserialize<State>(serializedIndexFile).state;
    }
    catch (IOException)
    {
      state = null;
    }

    return state;
  }

    public static string GetXeroTenantId(XeroOAuth2Token xeroToken)
    {
        Guid tenantId = GetCurrentTenantId();
        string xeroTenantId;

        if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
        {
            xeroTenantId = tenantId.ToString();
        }
        else
        {
            var id = xeroToken.Tenants.First().TenantId;
            xeroTenantId = id.ToString();
            StoreTenantId(id);
        }

        return xeroTenantId;
    }
}
