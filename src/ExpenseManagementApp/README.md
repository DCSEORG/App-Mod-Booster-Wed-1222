# Expense Management Application

A modern ASP.NET Core 8.0 web application for managing employee expenses with approval workflows.

## Overview

This application provides a complete expense management system with:
- Dashboard with real-time statistics
- Expense submission and tracking
- Manager approval workflows
- RESTful API with Swagger documentation
- Modern, responsive UI with clean design
- Azure Managed Identity support for secure database access

## Architecture

### Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with stored procedures
- **Authentication**: Azure Managed Identity
- **API Documentation**: Swagger/OpenAPI
- **Frontend**: Razor Pages with modern CSS

### Project Structure

```
ExpenseManagementApp/
├── Models/                      # Data models
│   ├── Expense.cs              # Expense entity with all properties
│   ├── ExpenseCategory.cs      # Category lookup
│   ├── ExpenseStatus.cs        # Status lookup
│   ├── User.cs                 # User entity
│   ├── Role.cs                 # Role entity
│   ├── ExpenseStatistics.cs    # Dashboard statistics
│   └── ErrorViewModel.cs       # Error page model
├── Services/                    # Business logic layer
│   ├── DatabaseService.cs      # Database connection with Managed Identity
│   ├── IExpenseService.cs      # Expense service interface
│   ├── ExpenseService.cs       # Expense operations (uses stored procedures)
│   ├── ICategoryService.cs     # Category service interface
│   ├── CategoryService.cs      # Category operations
│   ├── IUserService.cs         # User service interface
│   └── UserService.cs          # User operations
├── Controllers/                 # API Controllers
│   ├── ExpenseApiController.cs  # Expense CRUD endpoints
│   ├── CategoryApiController.cs # Category endpoints
│   ├── UserApiController.cs     # User endpoints
│   └── StatisticsApiController.cs # Statistics endpoint
├── Pages/                       # Razor Pages (UI)
│   ├── Index.cshtml/.cs        # Dashboard with statistics
│   ├── Expenses.cshtml/.cs     # All expenses list with filters
│   ├── NewExpense.cshtml/.cs   # Create new expense form
│   ├── Approvals.cshtml/.cs    # Manager approval page
│   ├── Error.cshtml/.cs        # Error page
│   ├── Shared/
│   │   ├── _Layout.cshtml      # Main layout with navigation
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/                     # Static files
│   ├── css/
│   │   └── site.css            # Modern styling (blue/purple theme)
│   └── js/
│       └── site.js             # Client-side JavaScript
├── Program.cs                   # Application configuration
├── appsettings.json            # Configuration
└── ExpenseManagementApp.csproj # Project file
```

## Features

### 1. Dashboard (/)
- Statistics cards showing:
  - Total expenses count
  - Pending approvals count and amount
  - Approved expenses count and amount
  - Rejected expenses count
- Recent expenses table
- Quick navigation to all features

### 2. Expenses (/Expenses)
- View all expenses
- Filter by status (Draft, Submitted, Approved, Rejected)
- Sortable table with expense details
- Status badges with color coding

### 3. New Expense (/NewExpense)
- Create new expense form
- Category selection dropdown
- Amount input (stored as pence in DB, displayed as GBP)
- Date picker for expense date
- Description text area
- Form validation

### 4. Approvals (/Approvals)
- Manager view of pending expenses
- Approve/Reject actions
- User and expense details
- Real-time updates

### 5. RESTful API (/swagger)
Complete REST API with Swagger documentation:

**Expenses**
- `GET /api/ExpenseApi` - Get all expenses (optional ?status filter)
- `GET /api/ExpenseApi/{id}` - Get expense by ID
- `POST /api/ExpenseApi` - Create expense
- `PUT /api/ExpenseApi/{id}` - Update expense
- `POST /api/ExpenseApi/{id}/submit` - Submit for approval
- `POST /api/ExpenseApi/{id}/approve` - Approve expense
- `POST /api/ExpenseApi/{id}/reject` - Reject expense
- `DELETE /api/ExpenseApi/{id}` - Delete expense

**Categories**
- `GET /api/CategoryApi` - Get all categories

**Users**
- `GET /api/UserApi` - Get all users
- `GET /api/UserApi/{id}` - Get user by ID

**Statistics**
- `GET /api/StatisticsApi` - Get dashboard statistics

## Database Integration

All database access uses stored procedures (no inline SQL):

### Stored Procedures Used
- `sp_GetExpenses` - Get all expenses with joins
- `sp_GetExpenseById` - Get single expense
- `sp_GetExpensesByStatus` - Filter by status
- `sp_CreateExpense` - Create new expense
- `sp_UpdateExpense` - Update expense details
- `sp_SubmitExpense` - Submit for approval
- `sp_ApproveExpense` - Approve expense
- `sp_RejectExpense` - Reject expense
- `sp_DeleteExpense` - Delete expense
- `sp_GetExpenseStatistics` - Dashboard statistics
- `sp_GetCategories` - Get all categories
- `sp_GetUsers` - Get all users
- `sp_GetUserById` - Get single user

### Currency Handling
- Amounts stored as `AmountMinor` (integer, in pence) to avoid floating-point issues
- Example: £25.40 stored as 2540
- Converted to decimal GBP for display in UI
- Stored procedures return both `AmountMinor` and `AmountGBP`

## Configuration

### Connection String
Configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net;Database=ExpenseManagement;Authentication=Active Directory Managed Identity;User Id=your-client-id;"
  }
}
```

### Managed Identity
The application supports Azure Managed Identity for secure database access:
- No passwords in connection strings
- Automatic token acquisition
- Falls back gracefully if not in Azure environment
- Tokens are cached and refreshed automatically

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- SQL Server database with expense management schema
- Stored procedures deployed (see `stored-procedures.sql`)

### Build and Run

```bash
cd src/ExpenseManagementApp
dotnet restore
dotnet build
dotnet run
```

Access the application:
- **Web UI**: https://localhost:5001
- **API Documentation**: https://localhost:5001/swagger

### Development Mode
In development, the app includes:
- Detailed error pages
- Swagger UI enabled
- Hot reload support

## Error Handling

The application implements graceful error handling:
- Database errors return dummy data with error details in logs
- API errors return structured JSON responses with error messages
- UI shows user-friendly error messages
- All errors logged to configured logging providers

## Design

### UI/UX
- Clean, modern design with card-based layouts
- Blue/purple color scheme matching style guide
- Responsive design for mobile/tablet/desktop
- Intuitive navigation
- Status badges with color coding
- Hover effects and smooth transitions

### Color Scheme
- Primary: #4F46E5 (Indigo)
- Success: #10B981 (Green)
- Warning: #F59E0B (Amber)
- Danger: #EF4444 (Red)
- Light background: #F9FAFB

## Security Considerations

1. **Managed Identity**: Uses Azure Managed Identity instead of SQL credentials
2. **Input Validation**: Form validation on client and server side
3. **SQL Injection Prevention**: All data access via stored procedures with parameters
4. **CORS**: Configurable CORS policy
5. **HTTPS**: Enforced in production

## Future Enhancements

Potential improvements:
- User authentication and authorization (Azure AD, ASP.NET Identity)
- Receipt file upload and storage (Azure Blob Storage)
- Email notifications for approvals
- Advanced reporting and analytics
- Export to Excel/PDF
- Audit logging
- Multi-currency support
- Mobile app

## API Examples

### Create Expense
```bash
curl -X POST https://localhost:5001/api/ExpenseApi \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "categoryId": 1,
    "amountMinor": 2540,
    "currency": "GBP",
    "expenseDate": "2024-01-15",
    "description": "Client meeting lunch"
  }'
```

### Get Statistics
```bash
curl https://localhost:5001/api/StatisticsApi
```

### Approve Expense
```bash
curl -X POST https://localhost:5001/api/ExpenseApi/5/approve \
  -H "Content-Type: application/json" \
  -d '{"reviewerId": 2}'
```

## Support

For issues or questions:
1. Check the Swagger documentation at `/swagger`
2. Review application logs
3. Verify database connectivity and stored procedures
4. Ensure Managed Identity is configured correctly in Azure

## License

Copyright © 2024 - Expense Management System
