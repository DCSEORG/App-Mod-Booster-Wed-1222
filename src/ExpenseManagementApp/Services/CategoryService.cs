using ExpenseManagementApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpenseManagementApp.Services;

public class CategoryService : ICategoryService
{
    private readonly DatabaseService _dbService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(DatabaseService dbService, ILogger<CategoryService> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        var categories = new List<ExpenseCategory>();
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_GetCategories", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new ExpenseCategory
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return GetDummyCategories();
        }
        
        return categories;
    }

    private static List<ExpenseCategory> GetDummyCategories()
    {
        return new List<ExpenseCategory>
        {
            new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
            new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
            new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
            new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
            new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
        };
    }
}
