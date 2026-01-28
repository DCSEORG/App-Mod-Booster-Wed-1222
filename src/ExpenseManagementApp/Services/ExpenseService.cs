using ExpenseManagementApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpenseManagementApp.Services;

public class ExpenseService : IExpenseService
{
    private readonly DatabaseService _dbService;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(DatabaseService dbService, ILogger<ExpenseService> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    public async Task<List<Expense>> GetExpensesAsync()
    {
        var expenses = new List<Expense>();
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_GetExpenses", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpenseFromReader(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expenses");
            return GetDummyExpenses();
        }
        
        return expenses;
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_GetExpenseById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapExpenseFromReader(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expense {ExpenseId}", expenseId);
        }
        
        return null;
    }

    public async Task<List<Expense>> GetExpensesByStatusAsync(string statusName)
    {
        var expenses = new List<Expense>();
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_GetExpensesByStatus", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@StatusName", statusName);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                expenses.Add(MapExpenseFromReader(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expenses by status {StatusName}", statusName);
            return GetDummyExpenses();
        }
        
        return expenses;
    }

    public async Task<int> CreateExpenseAsync(int userId, int categoryId, int amountMinor, string currency, DateTime expenseDate, string? description, string? receiptFile)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_CreateExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            command.Parameters.AddWithValue("@AmountMinor", amountMinor);
            command.Parameters.AddWithValue("@Currency", currency);
            command.Parameters.AddWithValue("@ExpenseDate", expenseDate);
            command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReceiptFile", (object?)receiptFile ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return 0;
        }
    }

    public async Task<bool> UpdateExpenseAsync(int expenseId, int categoryId, int amountMinor, DateTime expenseDate, string? description, string? receiptFile)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_UpdateExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@CategoryId", categoryId);
            command.Parameters.AddWithValue("@AmountMinor", amountMinor);
            command.Parameters.AddWithValue("@ExpenseDate", expenseDate);
            command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReceiptFile", (object?)receiptFile ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", expenseId);
            return false;
        }
    }

    public async Task<bool> SubmitExpenseAsync(int expenseId)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_SubmitExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense {ExpenseId}", expenseId);
            return false;
        }
    }

    public async Task<bool> ApproveExpenseAsync(int expenseId, int reviewerId)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_ApproveExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@ReviewerId", reviewerId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense {ExpenseId}", expenseId);
            return false;
        }
    }

    public async Task<bool> RejectExpenseAsync(int expenseId, int reviewerId)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_RejectExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@ReviewerId", reviewerId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense {ExpenseId}", expenseId);
            return false;
        }
    }

    public async Task<bool> DeleteExpenseAsync(int expenseId)
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_DeleteExpense", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ExpenseId", expenseId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", expenseId);
            return false;
        }
    }

    public async Task<ExpenseStatistics> GetExpenseStatisticsAsync()
    {
        try
        {
            using var connection = _dbService.GetConnection();
            await connection.OpenAsync();
            
            using var command = new SqlCommand("dbo.sp_GetExpenseStatistics", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ExpenseStatistics
                {
                    TotalExpenses = reader.GetInt32(reader.GetOrdinal("TotalExpenses")),
                    PendingCount = reader.GetInt32(reader.GetOrdinal("PendingCount")),
                    ApprovedCount = reader.GetInt32(reader.GetOrdinal("ApprovedCount")),
                    RejectedCount = reader.GetInt32(reader.GetOrdinal("RejectedCount")),
                    TotalApprovedAmount = reader.GetDecimal(reader.GetOrdinal("TotalApprovedAmount")),
                    TotalPendingAmount = reader.GetDecimal(reader.GetOrdinal("TotalPendingAmount")),
                    AverageExpenseAmount = reader.GetDecimal(reader.GetOrdinal("AverageExpenseAmount"))
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expense statistics");
        }
        
        return new ExpenseStatistics();
    }

    private static Expense MapExpenseFromReader(SqlDataReader reader)
    {
        return new Expense
        {
            ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
            StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
            AmountMinor = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
            AmountGBP = reader.GetDecimal(reader.GetOrdinal("AmountGBP")),
            Currency = reader.GetString(reader.GetOrdinal("Currency")),
            ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            ReceiptFile = reader.IsDBNull(reader.GetOrdinal("ReceiptFile")) ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
            SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
            ReviewedBy = reader.IsDBNull(reader.GetOrdinal("ReviewedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ReviewedBy")),
            ReviewerName = reader.IsDBNull(reader.GetOrdinal("ReviewerName")) ? null : reader.GetString(reader.GetOrdinal("ReviewerName")),
            ReviewedAt = reader.IsDBNull(reader.GetOrdinal("ReviewedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }

    private static List<Expense> GetDummyExpenses()
    {
        return new List<Expense>
        {
            new Expense
            {
                ExpenseId = 1,
                UserId = 1,
                UserName = "Alice Example",
                Email = "alice@example.co.uk",
                CategoryId = 1,
                CategoryName = "Travel",
                StatusId = 2,
                StatusName = "Submitted",
                AmountMinor = 2540,
                AmountGBP = 25.40m,
                Currency = "GBP",
                ExpenseDate = DateTime.Now.AddDays(-5),
                Description = "Taxi from airport to client site",
                ReceiptFile = null,
                SubmittedAt = DateTime.Now.AddDays(-5),
                CreatedAt = DateTime.Now.AddDays(-5)
            }
        };
    }
}
