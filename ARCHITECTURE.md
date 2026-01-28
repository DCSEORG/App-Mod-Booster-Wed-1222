# Expense Management System - Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                           User / Browser                             │
└────────────────┬───────────────────────────┬────────────────────────┘
                 │                           │
                 │ HTTPS                     │ HTTPS
                 ▼                           ▼
┌─────────────────────────────┐   ┌──────────────────────────────────┐
│     Expense Management      │   │       Chat UI with GenAI         │
│       Web Application       │   │  (Azure OpenAI Integration)      │
│   (ASP.NET Core Razor Pages)│   │                                  │
└────────────┬────────────────┘   └────────────┬─────────────────────┘
             │                                  │
             │ Uses                            │ Uses
             ▼                                  ▼
┌────────────────────────────────────────────────────────────────────┐
│                     RESTful API Layer                              │
│                    (ASP.NET Core Web API)                          │
│  • Expense API  • Category API  • User API  • Statistics API      │
│                    • Chat API (Function Calling)                   │
└────────────┬──────────────────────────────┬───────────────────────┘
             │                               │
             │ Calls Stored Procedures      │ Calls & Authenticates
             ▼                               ▼
┌────────────────────────────┐   ┌──────────────────────────────────┐
│      Azure SQL Database    │   │      Azure OpenAI (GPT-4o)       │
│   • Expenses               │   │   • Chat Completions             │
│   • Users                  │   │   • Function Calling             │
│   • Categories             │   │   • Model: gpt-4o                │
│   • Statuses               │   │   Location: swedencentral        │
│   • Stored Procedures      │   └──────────────────────────────────┘
│   Location: uksouth        │              │
└────────────┬───────────────┘              │ Integrated with
             │                               ▼
             │                    ┌──────────────────────────────────┐
             │                    │      Azure AI Search             │
             │                    │   • Document Indexing            │
             │                    │   • Semantic Search              │
             │                    │   Location: uksouth              │
             │                    └──────────────────────────────────┘
             │
             │ Authenticated via
             ▼
┌────────────────────────────────────────────────────────────────────┐
│              User-Assigned Managed Identity                        │
│   • Authenticates to Azure SQL (Entra ID)                         │
│   • Authenticates to Azure OpenAI (Cognitive Services OpenAI User)│
│   • Authenticates to AI Search (Search Index Data Contributor)    │
└────────────────────────────────────────────────────────────────────┘
             ▲
             │ Assigned to
             │
┌────────────────────────────────────────────────────────────────────┐
│                     Azure App Service (Linux)                      │
│   • SKU: Standard S1                                               │
│   • Runtime: .NET 8.0                                              │
│   • HTTPS Only                                                     │
│   • Always On                                                      │
│   Location: uksouth                                                │
└────────────────────────────────────────────────────────────────────┘
```

## Authentication Flow

```
1. App Service → Uses Managed Identity
2. Managed Identity → Authenticates to Azure SQL (Entra ID Only)
3. Managed Identity → Authenticates to Azure OpenAI (RBAC Role)
4. Managed Identity → Authenticates to AI Search (RBAC Role)
```

## Data Flow

```
User Request → App Service → API Controller → Service Layer 
            → Stored Procedure → Azure SQL → Result
            → Format Response → Return to User
```

## Chat Flow with Function Calling

```
User Message → Chat UI → POST /api/chat/message
           → ChatService → Azure OpenAI (with function definitions)
           → LLM decides to call function (e.g., get_expenses)
           → Execute function via ExpenseService
           → Send function result back to LLM
           → LLM generates natural language response
           → Return formatted response to user
```

## Security

- **No SQL Authentication**: Azure AD (Entra ID) only
- **No API Keys**: Managed Identity for all Azure services
- **HTTPS Only**: TLS 1.2+ enforced
- **RBAC**: Least privilege access for managed identity
- **Stored Procedures**: Parameterized queries, no SQL injection risk
- **Input Validation**: All API endpoints validate input

## Scalability

- **App Service**: Can scale up (S1 → S2 → S3) or out (multiple instances)
- **Azure SQL**: Can scale from Basic to Premium tiers
- **Azure OpenAI**: Capacity of 8 (configurable)
- **AI Search**: Can scale from Basic to Standard tiers
