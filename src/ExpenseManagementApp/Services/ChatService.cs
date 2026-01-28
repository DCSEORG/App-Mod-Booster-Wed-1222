using Azure.AI.OpenAI;
using Azure.Identity;
using ExpenseManagementApp.Models;
using System.Text.Json;
using System.Text;

namespace ExpenseManagementApp.Services;

public class ChatService : IChatService
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatService> _logger;
    private readonly OpenAIClient? _openAIClient;
    private readonly string? _deploymentName;

    public ChatService(
        IExpenseService expenseService,
        IUserService userService,
        ICategoryService categoryService,
        IConfiguration configuration,
        ILogger<ChatService> logger)
    {
        _expenseService = expenseService;
        _userService = userService;
        _categoryService = categoryService;
        _configuration = configuration;
        _logger = logger;

        var endpoint = _configuration["OpenAI:Endpoint"];
        _deploymentName = _configuration["OpenAI:DeploymentName"];
        var managedIdentityClientId = _configuration["ManagedIdentityClientId"];

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(_deploymentName))
        {
            try
            {
                var credential = string.IsNullOrEmpty(managedIdentityClientId)
                    ? new ManagedIdentityCredential()
                    : new ManagedIdentityCredential(managedIdentityClientId);

                _openAIClient = new OpenAIClient(new Uri(endpoint), credential);
                _logger.LogInformation("Azure OpenAI client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure OpenAI client");
            }
        }
        else
        {
            _logger.LogWarning("OpenAI configuration not found");
        }
    }

    public bool IsConfigured()
    {
        return _openAIClient != null && !string.IsNullOrEmpty(_deploymentName);
    }

    public async Task<string> ProcessChatMessageAsync(string message)
    {
        if (!IsConfigured())
        {
            return "GenAI services are not deployed. Please run deploy-with-chat.sh to enable AI features.";
        }

        try
        {
            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage("You are an AI assistant for an expense management system. You can help users manage their expenses using natural language. When listing expenses, format them nicely with line breaks and bullets."),
                    new ChatRequestUserMessage(message)
                },
                Tools = { GetExpensesTool(), GetExpenseByIdTool(), GetExpensesByStatusTool(), CreateExpenseTool(), SubmitExpenseTool(), ApproveExpenseTool(), RejectExpenseTool(), GetStatisticsTool() },
                Temperature = 0.7f,
                MaxTokens = 1500
            };

            var response = await _openAIClient!.GetChatCompletionsAsync(chatCompletionsOptions);
            
            var responseChoice = response.Value.Choices[0];
            
            // Function calling loop
            while (responseChoice.FinishReason == CompletionsFinishReason.ToolCalls)
            {
                var toolCalls = responseChoice.Message.ToolCalls;
                
                // Add assistant message with tool calls to conversation
                chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(responseChoice.Message));
                
                // Process each tool call
                foreach (var toolCall in toolCalls)
                {
                    if (toolCall is ChatCompletionsFunctionToolCall functionToolCall)
                    {
                        var functionResult = await ExecuteFunctionAsync(functionToolCall.Name, functionToolCall.Arguments);
                        
                        // Add tool response to conversation
                        chatCompletionsOptions.Messages.Add(new ChatRequestToolMessage(functionResult, functionToolCall.Id));
                    }
                }
                
                // Get next response from the model
                response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                responseChoice = response.Value.Choices[0];
            }

            return responseChoice.Message.Content ?? "I'm sorry, I couldn't process your request.";
        }
        catch (Azure.Identity.AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Authentication failed. Ensure Managed Identity is properly configured.");
            return "Authentication error: Unable to connect to Azure OpenAI. Please verify that Managed Identity is properly configured with access to the Azure OpenAI resource.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return "I'm sorry, an error occurred while processing your request. Please try again.";
        }
    }

    private async Task<string> ExecuteFunctionAsync(string functionName, string argumentsJson)
    {
        try
        {
            _logger.LogInformation($"Executing function: {functionName} with args: {argumentsJson}");

            switch (functionName)
            {
                case "get_expenses":
                    return await GetExpensesAsync();
                
                case "get_expense_by_id":
                    var getByIdArgs = JsonSerializer.Deserialize<GetExpenseByIdArgs>(argumentsJson);
                    return await GetExpenseByIdAsync(getByIdArgs?.ExpenseId ?? 0);
                
                case "get_expenses_by_status":
                    var getByStatusArgs = JsonSerializer.Deserialize<GetExpensesByStatusArgs>(argumentsJson);
                    return await GetExpensesByStatusAsync(getByStatusArgs?.Status ?? "");
                
                case "create_expense":
                    var createArgs = JsonSerializer.Deserialize<CreateExpenseArgs>(argumentsJson);
                    return await CreateExpenseAsync(createArgs);
                
                case "submit_expense":
                    var submitArgs = JsonSerializer.Deserialize<SubmitExpenseArgs>(argumentsJson);
                    return await SubmitExpenseAsync(submitArgs?.ExpenseId ?? 0);
                
                case "approve_expense":
                    var approveArgs = JsonSerializer.Deserialize<ApproveExpenseArgs>(argumentsJson);
                    return await ApproveExpenseAsync(approveArgs?.ExpenseId ?? 0, approveArgs?.ReviewerId ?? 0);
                
                case "reject_expense":
                    var rejectArgs = JsonSerializer.Deserialize<RejectExpenseArgs>(argumentsJson);
                    return await RejectExpenseAsync(rejectArgs?.ExpenseId ?? 0, rejectArgs?.ReviewerId ?? 0);
                
                case "get_statistics":
                    return await GetStatisticsAsync();
                
                default:
                    return JsonSerializer.Serialize(new { error = $"Unknown function: {functionName}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing function {functionName}");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> GetExpensesAsync()
    {
        var expenses = await _expenseService.GetExpensesAsync();
        return JsonSerializer.Serialize(expenses.Select(e => new
        {
            expenseId = e.ExpenseId,
            userName = e.UserName,
            categoryName = e.CategoryName,
            amount = $"£{e.AmountGBP:F2}",
            statusName = e.StatusName,
            expenseDate = e.ExpenseDate.ToString("yyyy-MM-dd"),
            description = e.Description
        }));
    }

    private async Task<string> GetExpenseByIdAsync(int expenseId)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(expenseId);
        if (expense == null)
        {
            return JsonSerializer.Serialize(new { error = "Expense not found" });
        }

        return JsonSerializer.Serialize(new
        {
            expenseId = expense.ExpenseId,
            userName = expense.UserName,
            email = expense.Email,
            categoryName = expense.CategoryName,
            amount = $"£{expense.AmountGBP:F2}",
            statusName = expense.StatusName,
            expenseDate = expense.ExpenseDate.ToString("yyyy-MM-dd"),
            description = expense.Description,
            submittedAt = expense.SubmittedAt?.ToString("yyyy-MM-dd HH:mm"),
            reviewerName = expense.ReviewerName,
            reviewedAt = expense.ReviewedAt?.ToString("yyyy-MM-dd HH:mm")
        });
    }

    private async Task<string> GetExpensesByStatusAsync(string status)
    {
        var expenses = await _expenseService.GetExpensesByStatusAsync(status);
        return JsonSerializer.Serialize(expenses.Select(e => new
        {
            expenseId = e.ExpenseId,
            userName = e.UserName,
            categoryName = e.CategoryName,
            amount = $"£{e.AmountGBP:F2}",
            statusName = e.StatusName,
            expenseDate = e.ExpenseDate.ToString("yyyy-MM-dd"),
            description = e.Description
        }));
    }

    private async Task<string> CreateExpenseAsync(CreateExpenseArgs? args)
    {
        if (args == null)
        {
            return JsonSerializer.Serialize(new { error = "Invalid arguments" });
        }

        var expenseId = await _expenseService.CreateExpenseAsync(
            args.UserId,
            args.CategoryId,
            args.AmountMinor,
            args.Currency ?? "GBP",
            args.ExpenseDate,
            args.Description,
            args.ReceiptFile
        );

        return JsonSerializer.Serialize(new
        {
            success = true,
            expenseId = expenseId,
            message = "Expense created successfully"
        });
    }

    private async Task<string> SubmitExpenseAsync(int expenseId)
    {
        var success = await _expenseService.SubmitExpenseAsync(expenseId);
        return JsonSerializer.Serialize(new
        {
            success = success,
            message = success ? "Expense submitted successfully" : "Failed to submit expense"
        });
    }

    private async Task<string> ApproveExpenseAsync(int expenseId, int reviewerId)
    {
        var success = await _expenseService.ApproveExpenseAsync(expenseId, reviewerId);
        return JsonSerializer.Serialize(new
        {
            success = success,
            message = success ? "Expense approved successfully" : "Failed to approve expense"
        });
    }

    private async Task<string> RejectExpenseAsync(int expenseId, int reviewerId)
    {
        var success = await _expenseService.RejectExpenseAsync(expenseId, reviewerId);
        return JsonSerializer.Serialize(new
        {
            success = success,
            message = success ? "Expense rejected successfully" : "Failed to reject expense"
        });
    }

    private async Task<string> GetStatisticsAsync()
    {
        var stats = await _expenseService.GetExpenseStatisticsAsync();
        return JsonSerializer.Serialize(new
        {
            totalExpenses = stats.TotalExpenses,
            pendingCount = stats.PendingCount,
            pendingAmount = $"£{stats.TotalPendingAmount:F2}",
            approvedCount = stats.ApprovedCount,
            approvedAmount = $"£{stats.TotalApprovedAmount:F2}",
            rejectedCount = stats.RejectedCount,
            averageAmount = $"£{stats.AverageExpenseAmount:F2}"
        });
    }

    private ChatCompletionsFunctionToolDefinition GetExpensesTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "get_expenses",
            Description = "Get a list of all expenses in the system",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{},\"required\":[]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition GetExpenseByIdTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "get_expense_by_id",
            Description = "Get details of a specific expense by ID",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"expenseId\":{\"type\":\"integer\",\"description\":\"The ID of the expense\"}},\"required\":[\"expenseId\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition GetExpensesByStatusTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "get_expenses_by_status",
            Description = "Get expenses filtered by status (Submitted, Approved, Rejected, or Draft)",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"status\":{\"type\":\"string\",\"description\":\"Status name: Submitted, Approved, Rejected, or Draft\",\"enum\":[\"Draft\",\"Submitted\",\"Approved\",\"Rejected\"]}},\"required\":[\"status\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition CreateExpenseTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "create_expense",
            Description = "Create a new expense",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"userId\":{\"type\":\"integer\",\"description\":\"The ID of the user creating the expense\"},\"categoryId\":{\"type\":\"integer\",\"description\":\"The category ID (1=Travel, 2=Meals, 3=Office Supplies, 4=Entertainment, 5=Other)\"},\"amountMinor\":{\"type\":\"integer\",\"description\":\"Amount in pence (e.g., 5000 for £50.00)\"},\"currency\":{\"type\":\"string\",\"description\":\"Currency code (default: GBP)\"},\"expenseDate\":{\"type\":\"string\",\"description\":\"Date of expense in ISO format (YYYY-MM-DD)\"},\"description\":{\"type\":\"string\",\"description\":\"Description of the expense\"},\"receiptFile\":{\"type\":\"string\",\"description\":\"Receipt file name (optional)\"}},\"required\":[\"userId\",\"categoryId\",\"amountMinor\",\"expenseDate\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition SubmitExpenseTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "submit_expense",
            Description = "Submit an expense for approval",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"expenseId\":{\"type\":\"integer\",\"description\":\"The ID of the expense to submit\"}},\"required\":[\"expenseId\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition ApproveExpenseTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "approve_expense",
            Description = "Approve an expense",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"expenseId\":{\"type\":\"integer\",\"description\":\"The ID of the expense to approve\"},\"reviewerId\":{\"type\":\"integer\",\"description\":\"The ID of the reviewer approving the expense\"}},\"required\":[\"expenseId\",\"reviewerId\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition RejectExpenseTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "reject_expense",
            Description = "Reject an expense",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{\"expenseId\":{\"type\":\"integer\",\"description\":\"The ID of the expense to reject\"},\"reviewerId\":{\"type\":\"integer\",\"description\":\"The ID of the reviewer rejecting the expense\"}},\"required\":[\"expenseId\",\"reviewerId\"]}")
        };
    }

    private ChatCompletionsFunctionToolDefinition GetStatisticsTool()
    {
        return new ChatCompletionsFunctionToolDefinition
        {
            Name = "get_statistics",
            Description = "Get expense statistics including total, pending, approved, and rejected expenses with amounts",
            Parameters = BinaryData.FromString("{\"type\":\"object\",\"properties\":{},\"required\":[]}")
        };
    }

    private class GetExpenseByIdArgs
    {
        public int ExpenseId { get; set; }
    }

    private class GetExpensesByStatusArgs
    {
        public string Status { get; set; } = string.Empty;
    }

    private class CreateExpenseArgs
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int AmountMinor { get; set; }
        public string? Currency { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string? ReceiptFile { get; set; }
    }

    private class SubmitExpenseArgs
    {
        public int ExpenseId { get; set; }
    }

    private class ApproveExpenseArgs
    {
        public int ExpenseId { get; set; }
        public int ReviewerId { get; set; }
    }

    private class RejectExpenseArgs
    {
        public int ExpenseId { get; set; }
        public int ReviewerId { get; set; }
    }
}
