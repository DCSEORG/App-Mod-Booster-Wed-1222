using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Services;

public interface IExpenseService
{
    Task<List<Expense>> GetExpensesAsync();
    Task<Expense?> GetExpenseByIdAsync(int expenseId);
    Task<List<Expense>> GetExpensesByStatusAsync(string statusName);
    Task<int> CreateExpenseAsync(int userId, int categoryId, int amountMinor, string currency, DateTime expenseDate, string? description, string? receiptFile);
    Task<bool> UpdateExpenseAsync(int expenseId, int categoryId, int amountMinor, DateTime expenseDate, string? description, string? receiptFile);
    Task<bool> SubmitExpenseAsync(int expenseId);
    Task<bool> ApproveExpenseAsync(int expenseId, int reviewerId);
    Task<bool> RejectExpenseAsync(int expenseId, int reviewerId);
    Task<bool> DeleteExpenseAsync(int expenseId);
    Task<ExpenseStatistics> GetExpenseStatisticsAsync();
}
