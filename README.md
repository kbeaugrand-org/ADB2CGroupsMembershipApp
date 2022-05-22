# Azure AD B2C Group Membership Claims Provider

This project has the objective to provide a way to add group membership claims to the access token issued by Azure AD B2C.

## How it works

The Azure AD B2C Group Membership Claims Provider is a function that is called by the Azure AD B2C token validation pipeline.

The function reads the token from the request and extracts the group membership claims. These claims are added to the token.

The group membership claims are retrieved from the AD Graph API.

## References

* [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory/b2c/overview)
* [MS Graph API](https://docs.microsoft.com/en-us/graph/api/resources)

## Quick start

### Create Azure AD B2C application

To create an Azure AD B2C application, follow these steps:

```sh
az ad sp create-for-rbac -n "ADB2CGroupsMembershipApp"
```

This command creates a new service principal for the ADB2CGroupsMembershipApp application.

It returns the application ID and password for the application.

### Add Application permissions to AD Graph APIs

We need to grant permissions to MS Graph. 

On Azure Portal AD B2C Administration, go to **Applications** and select **ADB2CGroupsMembershipApp**.
  * Go to API Permissons 
  * Select ``+ Add a Permission``
  * Under API selection, select ``Microsoft Graph``
  * Under Application Permissions select **Group.Read.All** and **User.Read.All**
  * Finally, click ``Add Permissions`` and ``Grant admin consent for...``

### Deploy the application

First you need to create a resource group and a Storage account to run your function app.

```ps1
az group create --name ADB2CGroupsMembershipApp --location westeurope
az storage account create --name adb2cgroupsmembershipapp --resource-group ADB2CGroupsMembershipApp --location westeurope --sku Standard_LRS
az appservice plan create --name ADB2CGroupsMembershipAppPlan --resource-group ADB2CGroupsMembershipApp --sku B1 --is-linux
```

Then you can deploy your function app.

```ps1
az functionapp create --name ADB2CGroupsMembershipApp --resource-group ADB2CGroupsMembershipApp --plan ADB2CGroupsMembershipAppPlan --storage-account adb2cgroupsmembershipapp --os-type Linux --runtime dotnet --deployment-container-image-name ghcr.io/kbeaugrand-org/adb2cgroupsmembership:latest 
```

This command creates a new Azure Function app, ADB2CGroupsMembershipApp, in the resource group ADB2CGroupsMembershipApp, with the following configuration:

* Runtime: dotnet
* Storage account: ADB2CGroupsMembershipAppStorage
* Consumption plan
* Deployment container image: ghcr.io/kbeaugrand-org/adb2cgroupsmembership:latest
* OS type: Linux

### Configure the function

One the Function App is created you mus configure it to use the Azure AD B2C Application credentials that were created.

```ps1
az functionapp config appsettings set --resource-group ADB2CGroupsMembershipApp --name ADB2CGroupsMembershipApp --settings ClientId=${ADB2CGroupsMembershipAppClientId}
az functionapp config appsettings set --resource-group ADB2CGroupsMembershipApp --name ADB2CGroupsMembershipApp --settings ClientSecret=${ADB2CGroupsMembershipAppClientSecret}
az functionapp config appsettings set --resource-group ADB2CGroupsMembershipApp --name ADB2CGroupsMembershipApp --settings TenantId=${ADB2CGroupsMembershipAppTenantId}
```