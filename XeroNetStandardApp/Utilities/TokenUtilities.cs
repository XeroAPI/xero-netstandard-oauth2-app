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
  [Serializable]
  public struct State
  {  
    public string state {get; set;}
    public State(string state){
      this.state = state;
    }
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
}
