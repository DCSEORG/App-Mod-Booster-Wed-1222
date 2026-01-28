#!/bin/bash
set -e

echo "=========================================="
echo "Expense Management System - Deployment"
echo "=========================================="
echo ""

# Configuration
RESOURCE_GROUP="rg-expensemgmt-demo"
LOCATION="uksouth"
DEPLOYMENT_NAME="expensemgmt-$(date +%Y%m%d-%H%M%S)"

# Get current user info for SQL Server admin
CURRENT_USER=$(az account show --query user.name -o tsv)
CURRENT_USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)

echo "Deployment Configuration:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  SQL Admin: $CURRENT_USER"
echo "  Admin Object ID: $CURRENT_USER_OBJECT_ID"
echo ""

# Create resource group
echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION --output none
echo "✓ Resource group created"
echo ""

# Deploy infrastructure (without GenAI)
echo "Deploying infrastructure..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/main.bicep \
  --parameters location=$LOCATION \
               environmentName=demo \
               adminObjectId=$CURRENT_USER_OBJECT_ID \
               adminLogin=$CURRENT_USER \
               deployGenAI=false \
  --query properties.outputs \
  --output json)

echo "✓ Infrastructure deployed"
echo ""

# Extract outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
APP_SERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceUrl.value')
MANAGED_IDENTITY_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityName.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')
SQL_SERVER_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerName.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
SQL_DATABASE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlDatabaseName.value')

echo "Deployment Outputs:"
echo "  App Service: $APP_SERVICE_NAME"
echo "  App URL: $APP_SERVICE_URL"
echo "  Managed Identity: $MANAGED_IDENTITY_NAME"
echo "  SQL Server: $SQL_SERVER_NAME"
echo "  Database: $SQL_DATABASE_NAME"
echo ""

# Configure App Service settings
echo "Configuring App Service settings..."
CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN};Database=${SQL_DATABASE_NAME};Authentication=Active Directory Managed Identity;User Id=${MANAGED_IDENTITY_CLIENT_ID};"

az webapp config appsettings set \
  --name $APP_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    "AZURE_CLIENT_ID=$MANAGED_IDENTITY_CLIENT_ID" \
    "ManagedIdentityClientId=$MANAGED_IDENTITY_CLIENT_ID" \
  --output none

echo "✓ App Service settings configured"
echo ""

# Wait for SQL Server to be ready
echo "Waiting 30 seconds for SQL Server to be fully ready..."
sleep 30
echo ""

# Add current IP to SQL firewall
echo "Adding current IP to SQL firewall..."
MY_IP=$(curl -s https://api.ipify.org)
echo "  Current IP: $MY_IP"

# Allow Azure services access
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name "AllowAllAzureIPs" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

# Add deployment IP
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name "AllowDeploymentIP" \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP \
  --output none

echo "✓ Firewall rules configured"
echo ""

echo "Waiting 15 seconds for firewall rules to propagate..."
sleep 15
echo ""

# Install Python dependencies
echo "Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity
echo "✓ Python dependencies installed"
echo ""

# Update Python scripts with actual server details
echo "Updating Python scripts..."
sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/${SQL_SERVER_FQDN}/g" run-sql.py && rm -f run-sql.py.bak
sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/${SQL_SERVER_FQDN}/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak
sed -i.bak "s/sql-expensemgmt-placeholder.database.windows.net/${SQL_SERVER_FQDN}/g" run-sql-stored-procs.py && rm -f run-sql-stored-procs.py.bak
sed -i.bak "s/MANAGED-IDENTITY/${MANAGED_IDENTITY_NAME}/g" script.sql && rm -f script.sql.bak
echo "✓ Python scripts updated"
echo ""

# Import database schema
echo "Importing database schema..."
python3 run-sql.py
echo "✓ Database schema imported"
echo ""

# Configure database roles for managed identity
echo "Configuring database roles for managed identity..."
python3 run-sql-dbrole.py
echo "✓ Database roles configured"
echo ""

# Deploy stored procedures
echo "Deploying stored procedures..."
python3 run-sql-stored-procs.py
echo "✓ Stored procedures deployed"
echo ""

# Build and package the application
echo "Building application..."
cd src/ExpenseManagementApp
dotnet publish -c Release -o ../../publish
cd ../..
echo "✓ Application built"
echo ""

# Create deployment package
echo "Creating deployment package..."
cd publish
zip -r ../app.zip * > /dev/null
cd ..
echo "✓ Deployment package created"
echo ""

# Deploy application to App Service
echo "Deploying application to App Service..."
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --src-path app.zip \
  --type zip \
  --output none
echo "✓ Application deployed"
echo ""

echo "=========================================="
echo "Deployment Complete!"
echo "=========================================="
echo ""
echo "Application URL: ${APP_SERVICE_URL}/Index"
echo "API Documentation: ${APP_SERVICE_URL}/swagger"
echo "Chat UI: ${APP_SERVICE_URL}/chatui"
echo ""
echo "Note: The Chat UI will show a message that GenAI services are not deployed."
echo "To deploy with GenAI services, run: ./deploy-with-chat.sh"
echo ""
