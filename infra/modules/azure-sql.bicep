// Azure SQL Database with Entra ID Authentication

@description('Location for all resources')
param location string

@description('Environment name')
param environmentName string

@description('Unique suffix for resource names')
param uniqueSuffix string

@description('Resource prefix')
param resourcePrefix string

@description('Azure AD admin Object ID')
param adminObjectId string

@description('Azure AD admin login name')
param adminLogin string

@description('Managed Identity Principal ID for database access')
param managedIdentityPrincipalId string

@description('Managed Identity Name')
param managedIdentityName string

var sqlServerName = 'sql-${resourcePrefix}-${uniqueSuffix}'
var databaseName = 'ExpenseManagement'

// Create SQL Server with Entra ID authentication
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: 'sqladmin' // Required but not used with Entra ID only
    administratorLoginPassword: '${uniqueString(resourceGroup().id)}${uniqueString(subscription().id)}Aa1!' // Required but not used
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// Configure Entra ID Administrator
resource sqlAdministrator 'Microsoft.Sql/servers/administrators@2021-11-01' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: adminLogin
    sid: adminObjectId
    tenantId: subscription().tenantId
    azureADOnlyAuthentication: true
  }
}

// Create Database
resource database 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
  }
}

// Allow Azure services to access the server
resource firewallRuleAzureServices 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Outputs
output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
output managedIdentityName string = managedIdentityName
