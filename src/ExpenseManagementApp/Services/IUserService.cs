using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Services;

public interface IUserService
{
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
}
