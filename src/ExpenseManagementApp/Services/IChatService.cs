namespace ExpenseManagementApp.Services;

public interface IChatService
{
    Task<string> ProcessChatMessageAsync(string message);
    bool IsConfigured();
}
