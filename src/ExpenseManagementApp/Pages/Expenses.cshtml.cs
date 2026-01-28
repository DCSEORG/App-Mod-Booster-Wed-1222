using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagementApp.Pages;

public class ExpensesModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesModel> _logger;

    public ExpensesModel(IExpenseService expenseService, ILogger<ExpensesModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public List<Expense> Expenses { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                Expenses = await _expenseService.GetExpensesByStatusAsync(StatusFilter);
            }
            else
            {
                Expenses = await _expenseService.GetExpensesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
        }
    }
}
