# Expense Management System - Modern Azure Application

A modern, cloud-native expense management application built with ASP.NET Core 8.0, Azure SQL Database, and Azure OpenAI for natural language interactions.

![Expense Management](repo-header-booster.png)

## 🚀 Features

### Core Application
- **Modern Web UI**: Clean, responsive interface with dashboard, expense tracking, and approvals
- **RESTful API**: Full CRUD operations with Swagger documentation
- **Role-Based Access**: Employee and Manager roles with different permissions
- **Expense Management**: Create, submit, approve/reject expenses with categories and receipts
- **Real-time Statistics**: Dashboard with pending approvals, approved amounts, and expense counts
- **Currency Support**: GBP with proper handling of amounts (stored as pence, displayed as pounds)

### AI-Powered Chat Interface
- **Natural Language Queries**: "Show me all pending expenses" or "Create a travel expense for £50"
- **Function Calling**: Azure OpenAI GPT-4o with automatic function execution
- **Smart Responses**: Formatted lists, tables, and summaries
- **Context-Aware**: Understands expense management domain

### Security & Best Practices
- **Managed Identity**: No passwords or API keys - uses Azure AD authentication
- **Entra ID Only**: Azure SQL with Azure AD authentication only (no SQL auth)
- **Stored Procedures**: All data access through parameterized stored procedures
- **HTTPS Only**: TLS 1.2+ enforced
- **RBAC**: Least privilege access for all Azure services

## 📋 Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture diagram and data flows.

**Key Components:**
- **Azure App Service** (Linux, .NET 8.0, Standard S1)
- **Azure SQL Database** (Entra ID authentication, Basic tier)
- **Azure OpenAI** (GPT-4o model, Sweden Central)
- **Azure AI Search** (Basic tier)
- **User-Assigned Managed Identity** (Authentication for all services)

## 🛠️ Prerequisites

- Azure subscription
- Azure CLI installed and logged in (`az login`)
- .NET 8.0 SDK (for local development)
- Python 3.x with pip (for database deployment scripts)
- jq (for JSON parsing in deployment scripts)
- ODBC Driver 18 for SQL Server

### Install ODBC Driver (Ubuntu/Codespaces)
```bash
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18
```

## 🚀 Quick Start - Deploy to Azure

### Option 1: Deploy Without GenAI (Faster, Lower Cost)

Deploy the core application without Azure OpenAI chat features:

```bash
chmod +x deploy.sh
./deploy.sh
```

This deploys:
- Azure App Service with web application
- Azure SQL Database with schema and sample data
- RESTful APIs with Swagger documentation
- Chat UI (shows message that GenAI is not deployed)

**Estimated deployment time**: 5-8 minutes  
**Estimated monthly cost**: ~£30-50

### Option 2: Deploy With GenAI Chat (Full Features)

Deploy the complete application with AI-powered chat:

```bash
chmod +x deploy-with-chat.sh
./deploy-with-chat.sh
```

This deploys everything from Option 1 PLUS:
- Azure OpenAI (GPT-4o model)
- Azure AI Search
- Fully functional AI chat assistant

**Estimated deployment time**: 8-12 minutes  
**Estimated monthly cost**: ~£80-120

## 📱 Using the Application

After deployment, the script will output URLs:

### Web Application
```
https://<your-app>.azurewebsites.net/Index
```
- **Dashboard**: View statistics and recent expenses
- **Expenses**: List and filter all expenses
- **New Expense**: Create and submit new expenses
- **Approvals**: Managers can approve/reject pending expenses

### API Documentation
```
https://<your-app>.azurewebsites.net/swagger
```
Interactive Swagger UI for testing all API endpoints:
- `GET /api/expenses` - List all expenses
- `POST /api/expenses` - Create new expense
- `PUT /api/expenses/{id}` - Update expense
- `DELETE /api/expenses/{id}` - Delete draft expense
- `POST /api/expenses/{id}/submit` - Submit for approval
- `POST /api/expenses/{id}/approve` - Approve expense
- `POST /api/expenses/{id}/reject` - Reject expense
- `GET /api/statistics` - Get dashboard statistics
- `GET /api/categories` - List expense categories
- `GET /api/users` - List users

### AI Chat Interface
```
https://<your-app>.azurewebsites.net/chatui
```
Natural language interface for expense management:
- "Show me all my expenses"
- "What are the pending expenses?"
- "Create a travel expense for £50 dated today with description 'Taxi to client site'"
- "Approve expense ID 5"
- "Show me statistics"

## 💻 Local Development

### 1. Clone and Setup
```bash
git clone <repository-url>
cd App-Mod-Booster-Wed-1222
```

### 2. Configure Connection String

Update `src/ExpenseManagementApp/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:<your-sql-server>.database.windows.net;Database=ExpenseManagement;Authentication=Active Directory Default;"
  }
}
```

Note: Use `Active Directory Default` for local development (requires `az login`)

### 3. Run Locally
```bash
cd src/ExpenseManagementApp
dotnet restore
dotnet run
```

Visit: `https://localhost:5001/Index`

### 4. Run with Hot Reload
```bash
dotnet watch run
```

## 📊 Database Schema

The application uses a SQL Server database with:

- **Roles**: Employee, Manager
- **Users**: With manager relationships
- **ExpenseCategories**: Travel, Meals, Supplies, Accommodation, Other
- **ExpenseStatus**: Draft, Submitted, Approved, Rejected
- **Expenses**: Main expense records with amounts in pence

All data access goes through **stored procedures** (see `stored-procedures.sql`).

## 🔧 Project Structure

```
.
├── infra/                          # Infrastructure as Code
│   ├── main.bicep                  # Main orchestration template
│   └── modules/
│       ├── app-service.bicep       # App Service + Managed Identity
│       ├── azure-sql.bicep         # SQL Database (Entra ID)
│       └── genai.bicep             # Azure OpenAI + AI Search
├── src/ExpenseManagementApp/       # ASP.NET Core Application
│   ├── Controllers/                # API Controllers
│   ├── Models/                     # Data models
│   ├── Pages/                      # Razor Pages (UI)
│   ├── Services/                   # Business logic
│   └── wwwroot/                    # Static files + Chat UI
├── Database-Schema/
│   └── database_schema.sql         # SQL schema with sample data
├── stored-procedures.sql           # All stored procedures
├── run-sql.py                      # Deploy schema script
├── run-sql-dbrole.py              # Configure managed identity
├── run-sql-stored-procs.py        # Deploy stored procedures
├── deploy.sh                       # Deploy without GenAI
└── deploy-with-chat.sh            # Deploy with GenAI
```

## 🔐 Security Considerations

### Managed Identity
The application uses a **User-Assigned Managed Identity** to:
- Connect to Azure SQL Database (Entra ID authentication)
- Access Azure OpenAI (Cognitive Services OpenAI User role)
- Access Azure AI Search (Search Index Data Contributor role)

**No passwords or API keys** are stored in the application.

### SQL Security
- **Entra ID Only**: SQL authentication is disabled by policy
- **Stored Procedures**: All data access through parameterized procedures
- **Least Privilege**: Managed identity has only db_datareader, db_datawriter, and EXECUTE permissions

### Network Security
- **HTTPS Only**: TLS 1.2+ enforced on App Service
- **Firewall Rules**: SQL Server allows Azure services + deployment IP
- **Public Access**: Enabled for PoC (restrict for production)

## 📈 Scaling Considerations

### Performance Optimization
1. **App Service**: Scale up to S2/S3 or out to multiple instances
2. **SQL Database**: Upgrade from Basic to Standard/Premium for better performance
3. **Azure OpenAI**: Increase capacity from 8 to higher for more throughput
4. **Caching**: Add Azure Redis Cache for frequently accessed data

### High Availability
1. Enable **App Service deployment slots** for zero-downtime deployments
2. Use **SQL Database geo-replication** for disaster recovery
3. Configure **Application Insights** for monitoring and alerting

## 🧪 Testing

### Test the API
```bash
# Get all expenses
curl https://<your-app>.azurewebsites.net/api/expenses

# Create new expense
curl -X POST https://<your-app>.azurewebsites.net/api/expenses \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "categoryId": 1,
    "amountMinor": 5000,
    "currency": "GBP",
    "expenseDate": "2025-01-28",
    "description": "Taxi to client meeting"
  }'
```

### Test the Chat API
```bash
curl -X POST https://<your-app>.azurewebsites.net/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{"message": "Show me all expenses"}'
```

## 🐛 Troubleshooting

### Database Connection Issues
If you see "Unable to connect to database":
1. Check firewall rules: `az sql server firewall-rule list --resource-group <rg> --server <server>`
2. Verify managed identity has database permissions: Run `run-sql-dbrole.py` again
3. Check App Service configuration: Ensure `ManagedIdentityClientId` is set

### GenAI Not Working
If chat returns "GenAI services are not deployed":
1. Verify OpenAI endpoint: `az cognitiveservices account show --name <openai-name> --resource-group <rg>`
2. Check App Service settings: Ensure `OpenAI__Endpoint` and `OpenAI__DeploymentName` are set
3. Verify RBAC: Managed identity needs "Cognitive Services OpenAI User" role

### Build Errors
```bash
cd src/ExpenseManagementApp
dotnet clean
dotnet restore
dotnet build
```

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

This is a proof-of-concept application. For production use:
1. Add authentication/authorization (Azure AD B2C, Auth0, etc.)
2. Implement proper error handling and logging (Application Insights)
3. Add unit and integration tests
4. Implement CI/CD pipelines (GitHub Actions, Azure DevOps)
5. Add rate limiting and DDoS protection
6. Implement proper secret management (Azure Key Vault)
7. Add data encryption at rest and in transit
8. Implement audit logging for compliance

## 📞 Support

For issues or questions:
1. Check the [ARCHITECTURE.md](ARCHITECTURE.md) for system design
2. Review deployment logs for specific error messages
3. Check Azure Portal for resource status and logs

---

**Built with ❤️ using Azure App Modernization Booster**
