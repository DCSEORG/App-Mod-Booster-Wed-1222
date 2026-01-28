// Main Bicep template for Expense Management System
// Orchestrates deployment of all Azure resources

targetScope = 'resourceGroup'

@description('Location for all resources')
param location string = 'uksouth'

@description('Environment name (e.g., dev, prod)')
param environmentName string = 'demo'

@description('Azure AD admin Object ID for SQL Server')
param adminObjectId string

@description('Azure AD admin login name for SQL Server')
param adminLogin string

@description('Deploy GenAI resources (Azure OpenAI, AI Search)')
param deployGenAI bool = false

// Generate unique suffix for resource names using resource group ID
var uniqueSuffix = uniqueString(resourceGroup().id)
var resourcePrefix = 'expensemgmt'

// Deploy App Service with managed identity
module appService 'modules/app-service.bicep' = {
  name: 'app-service-deployment'
  params: {
    location: location
    environmentName: environmentName
    uniqueSuffix: uniqueSuffix
    resourcePrefix: resourcePrefix
  }
}

// Deploy Azure SQL Database
module azureSQL 'modules/azure-sql.bicep' = {
  name: 'azure-sql-deployment'
  params: {
    location: location
    environmentName: environmentName
    uniqueSuffix: uniqueSuffix
    resourcePrefix: resourcePrefix
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
    managedIdentityName: appService.outputs.managedIdentityName
  }
}

// Deploy GenAI resources (conditional)
module genAI 'modules/genai.bicep' = if (deployGenAI) {
  name: 'genai-deployment'
  params: {
    location: location
    environmentName: environmentName
    uniqueSuffix: uniqueSuffix
    resourcePrefix: resourcePrefix
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

// Outputs
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output managedIdentityName string = appService.outputs.managedIdentityName
output managedIdentityClientId string = appService.outputs.managedIdentityClientId
output managedIdentityPrincipalId string = appService.outputs.managedIdentityPrincipalId

output sqlServerName string = azureSQL.outputs.sqlServerName
output sqlServerFqdn string = azureSQL.outputs.sqlServerFqdn
output sqlDatabaseName string = azureSQL.outputs.databaseName

output openAIEndpoint string = deployGenAI ? genAI.outputs.openAIEndpoint : ''
output openAIModelName string = deployGenAI ? genAI.outputs.openAIModelName : ''
output searchEndpoint string = deployGenAI ? genAI.outputs.searchEndpoint : ''
output openAIName string = deployGenAI ? genAI.outputs.openAIName : ''
output searchName string = deployGenAI ? genAI.outputs.searchName : ''
