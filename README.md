# Xero NetStandard OAuth 2.0 Starter App
This is a companion app built with .NET Core 3.1 MVC to demonstrate Xero OAuth 2.0 Client Authentication & OAuth 2.0 APIs.

__IMPORTANT!__ This application is for demo only. We recommend setting up a secure token storage for your production app.

Its functions include:

- connect & reconnect to Xero
- storing Xero token in a .json file
- refresh Xero access token on expiry
- allow user to switch between tenants/organisations
- allow user to disconnect a tenant or revoke token
- allow manual testing of many Xero API endpoints
- display API call responses
- display code snippets responsible for the call

You can connect this companion app to an actual Xero organisation and make real API calls. Please use your Demo Company organisation for your testing. [Here](https://central.xero.com/s/article/Use-the-demo-company) is how to turn it on. 

### Create a Xero app
You will need your Xero app credentials created to run this demo app.

To obtain your API keys, follow these steps:

* Create a [free Xero user account](https://www.xero.com/us/signup/api/) (if you don't have one)
* Login to [Xero developer center](https://developer.xero.com/myapps)
* Click "New App" link
* Enter your App name, company url, privacy policy url.
* Enter the redirect URI (your callback url - i.e. `https://localhost:5001/Authorization/Callback`)
* Agree to terms and condition and click "Create App".
* Click "Generate a secret" button.
* Copy your client id and client secret and save for use later.
* Click the "Save" button. You secret is now hidden.

### Download the code
Clone this repo to your local drive or open with GitHub desktop client.

### Configure your API Keys
In /NetStandardApp/appsettings.json, you should populate your XeroConfiguration as such: 

```
  "XeroConfiguration": {
    "ClientId": "YOUR_XERO_APP_CLIENT_ID",
    "ClientSecret": "YOUR_XERO_APP_CLIENT_SECRET",
    "CallbackUri": "https://localhost:5001/Authorization/Callback",
    "Scope": "openid offline_access profile ... ",
    "State": "YOUR_STATE"
  }
```

Note that you will have to have a state. The CallbackUri has to be exactly the same as redirect URI you put in Xero developer portal letter by letter. 

## Getting started with _dotnet_  & command line 
You can run this application with [dotnet SDK](https://code.visualstudio.com/download) from command line. 
### Install dotnet SDK
[Download](https://code.visualstudio.com/download) and install dotnet SDK on your machine. 

Verify in command line by:
```
$ dotnet --version
3.1.102
```
### Build the project
Change directory to NetStandardApp directory where you can see XeroNetStandardApp.csproj, build the project by: 

```
$ dotnet build
Microsoft (R) Build Engine version 16.4.0+e901037fe for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.
.
.
.

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.22
```
### Run the project 
In /NetStandardApp, run the project by:

```
$ dotnet run
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/.../Xero-NetStandard-App/XeroNetStandardApp
```
### Test the project
Open your browser, type in https://localhost:5001

## Getting started with Visual Studio Code
You can also run it using an IDE such as [VS Code](https://code.visualstudio.com/download) with [C# extension](https://code.visualstudio.com/docs/languages/csharp) installed. 
 
### Install VS Code
[Download](url) an install VS Code and open the project root folder with it. 

![image](https://user-images.githubusercontent.com/41350731/76296821-ec176380-630a-11ea-8a61-5b6ba1336862.png)

Go to Extensions, install C# extension by OmniSharp. 

![image](https://user-images.githubusercontent.com/41350731/76296935-19fca800-630b-11ea-8684-916c78254618.png)

### Build the project
Go back to Explorer and press F5 (or go to _Debug_ > _Start Debugging_). It will ask you for environment for launch configuration. Select _.NET Core_.

![image](https://user-images.githubusercontent.com/41350731/76297100-5d571680-630b-11ea-8ba2-c47105931ff4.png)

Save the launch.json, then press F5 again to run.

![image](https://user-images.githubusercontent.com/41350731/76299273-e3c12780-630e-11ea-9efc-c6460f0fb2ac.png)

You should see following in the DEBUG CONSOLE and be directed to your default browser with https://localhost:5001 already open. 

![image](https://user-images.githubusercontent.com/41350731/76297350-ca6aac00-630b-11ea-8cd3-05f098c3226a.png)

Start your Testing. 

## Some explanation of the code

**HomeController**
- checks if there is a xerotoken.json, and 
- passes a boolean firstTimeConnection to view to control the display of buttons. 

**AuthorizationController**
- reads XeroConfiguration &  make httpClientFactory available via dependency injection
- on /Authorization/, redirects user to Xero OAuth for authentication & authorization
- receives callback on /Authorization/Callback request Xero token
- gets connected tenants (organisations)
- store token via a public static method TokenUtilities.StoreToken(xeroToken);

**AssetsInfoController**
- makes API call to assets endpoint (Asset API)
- displays all current Fixed Assets (GET)
- allows for creation of Fixed Asset (PUT)

**AuEmployeesInfoController**
- makes API call to employees endpoint (PayrollAu API)
- displays all current AU employees (GET)
- allows for creation of a new Employee (PUT)

**BankfeedConnectionsController**
- makes API call to feed connections endpoint (BankFeeds API)
- displays all current feed connections (GET), allows for deletion (POST)
- allows for creation of new feed connection (PUT)

**BankfeedStatementsController**
- makes API call to statements endpoint (BankFeeds API)
- displays all current statements (GET)
- allows for creation of new statement (PUT)

**BankTransactionsInfoController**
- makes API call to bank transactions endpoint (Accounting API)
- displays all current bank transactions (GET)

**ContactInfoController** 
- makes api call to contacts endpoint
- displays in view
- static view Create.cshtml creates a web form and POST contact info to Create() action, and
- makes an create operation to contacts endpoint 

**IdentityInfoController**
- gets the list of tenant connections
- displays tenant information (GET /connections)
- allows user to disconnect a specific tenant (DELETE /connections/{id})

**InvoiceSyncController**
- gets invoices in the last 7 days and displays them in view (GET /invoices)
- allows user to upload attachments to a specific invoice (POST {id}/attachments)

**NzEmployeesInfoController**
- gets a list of employees in NZ Payroll (GET)
- displays them in view
- allows user to create new employees (POST)

**OrganisationInfoController**
- gets the current organisation information (GET)
- displays in view

**ProjectInfoController**
- gets the list of projects in Xero projects (GET)
- displays in view

**PurchaseOrderSyncController**
- gets a list of purchase orders (GET)
- displays in view
- allows user to create a new purchase order (POST)

**UkEmployeeInfoController**
- gets a list of employees in Uk Payroll (GET)
- displays them in view
- allows user to create new employees (POST)

Xero token is stored in a JSON file in the root of the project "./xerotoken.json". The app serialise and deserialise with the static class functions in /Utilities/TokenUtilities.cs. Most controllers will get and refresh token before calling API methods.

## License

This software is published under the [MIT License](http://en.wikipedia.org/wiki/MIT_License).

	Copyright (c) 2020 Xero Limited

	Permission is hereby granted, free of charge, to any person
	obtaining a copy of this software and associated documentation
	files (the "Software"), to deal in the Software without
	restriction, including without limitation the rights to use,
	copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the
	Software is furnished to do so, subject to the following
	conditions:

	The above copyright notice and this permission notice shall be
	included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
	EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
	OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
	NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
	HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
	WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
	OTHER DEALINGS IN THE SOFTWARE.
