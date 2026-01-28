using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseManagementApp.Pages;

public class NewExpenseModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<NewExpenseModel> _logger;

    public NewExpenseModel(IExpenseService expenseService, ICategoryService categoryService, ILogger<NewExpenseModel> logger)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _logger = logger;
    }

    public List<ExpenseCategory> Categories { get; set; } = new();

    [BindProperty]
    public int CategoryId { get; set; }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [BindProperty]
    public string? Description { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _categoryService.GetCategoriesAsync();

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors in the form";
            return Page();
        }

        try
        {
            int amountMinor = (int)(Amount * 100);
            var expenseId = await _expenseService.CreateExpenseAsync(
                userId: 1, // In a real app, get from authentication
                categoryId: CategoryId,
                amountMinor: amountMinor,
                currency: "GBP",
                expenseDate: ExpenseDate,
                description: Description,
                receiptFile: null
            );

            if (expenseId > 0)
            {
                return RedirectToPage("/Expenses");
            }
            else
            {
                ErrorMessage = "Failed to create expense";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            ErrorMessage = "An error occurred while creating the expense";
            return Page();
        }
    }
}
