using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagementApp.Pages;

public class ApprovalsModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ApprovalsModel> _logger;

    public ApprovalsModel(IExpenseService expenseService, ILogger<ApprovalsModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public List<Expense> PendingExpenses { get; set; } = new();
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            PendingExpenses = await _expenseService.GetExpensesByStatusAsync("Submitted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending expenses");
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(int expenseId)
    {
        try
        {
            var success = await _expenseService.ApproveExpenseAsync(expenseId, reviewerId: 2);
            Message = success ? "Expense approved successfully" : "Failed to approve expense";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense {ExpenseId}", expenseId);
            Message = "An error occurred while approving the expense";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int expenseId)
    {
        try
        {
            var success = await _expenseService.RejectExpenseAsync(expenseId, reviewerId: 2);
            Message = success ? "Expense rejected successfully" : "Failed to reject expense";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense {ExpenseId}", expenseId);
            Message = "An error occurred while rejecting the expense";
        }

        return RedirectToPage();
    }
}
