using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagementApp.Pages;

public class IndexModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpenseService expenseService, ILogger<IndexModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public ExpenseStatistics Statistics { get; set; } = new();
    public List<Expense> RecentExpenses { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            Statistics = await _expenseService.GetExpenseStatisticsAsync();
            var allExpenses = await _expenseService.GetExpensesAsync();
            RecentExpenses = allExpenses.OrderByDescending(e => e.CreatedAt).Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
        }
    }
}
