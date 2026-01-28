using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Services;

public interface ICategoryService
{
    Task<List<ExpenseCategory>> GetCategoriesAsync();
}
