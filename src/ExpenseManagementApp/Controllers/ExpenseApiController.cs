using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseApiController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpenseApiController> _logger;

    public ExpenseApiController(IExpenseService expenseService, ILogger<ExpenseApiController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetExpenses([FromQuery] string? status = null)
    {
        try
        {
            var expenses = string.IsNullOrEmpty(status) 
                ? await _expenseService.GetExpensesAsync()
                : await _expenseService.GetExpensesByStatusAsync(status);
            
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetExpenses");
            return StatusCode(500, new { error = "An error occurred while fetching expenses", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        try
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
            {
                return NotFound(new { error = $"Expense {id} not found" });
            }
            
            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the expense", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        try
        {
            var expenseId = await _expenseService.CreateExpenseAsync(
                request.UserId,
                request.CategoryId,
                request.AmountMinor,
                request.Currency ?? "GBP",
                request.ExpenseDate,
                request.Description,
                request.ReceiptFile
            );

            if (expenseId > 0)
            {
                return CreatedAtAction(nameof(GetExpense), new { id = expenseId }, new { expenseId });
            }
            
            return BadRequest(new { error = "Failed to create expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateExpense");
            return StatusCode(500, new { error = "An error occurred while creating the expense", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        try
        {
            var success = await _expenseService.UpdateExpenseAsync(
                id,
                request.CategoryId,
                request.AmountMinor,
                request.ExpenseDate,
                request.Description,
                request.ReceiptFile
            );

            if (success)
            {
                return NoContent();
            }
            
            return BadRequest(new { error = "Failed to update expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the expense", details = ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult> SubmitExpense(int id)
    {
        try
        {
            var success = await _expenseService.SubmitExpenseAsync(id);
            if (success)
            {
                return Ok(new { message = "Expense submitted successfully" });
            }
            
            return BadRequest(new { error = "Failed to submit expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubmitExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while submitting the expense", details = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveExpense(int id, [FromBody] ReviewRequest request)
    {
        try
        {
            var success = await _expenseService.ApproveExpenseAsync(id, request.ReviewerId);
            if (success)
            {
                return Ok(new { message = "Expense approved successfully" });
            }
            
            return BadRequest(new { error = "Failed to approve expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ApproveExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while approving the expense", details = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectExpense(int id, [FromBody] ReviewRequest request)
    {
        try
        {
            var success = await _expenseService.RejectExpenseAsync(id, request.ReviewerId);
            if (success)
            {
                return Ok(new { message = "Expense rejected successfully" });
            }
            
            return BadRequest(new { error = "Failed to reject expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RejectExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while rejecting the expense", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExpense(int id)
    {
        try
        {
            var success = await _expenseService.DeleteExpenseAsync(id);
            if (success)
            {
                return NoContent();
            }
            
            return BadRequest(new { error = "Failed to delete expense" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteExpense for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the expense", details = ex.Message });
        }
    }
}

public record CreateExpenseRequest(
    int UserId,
    int CategoryId,
    int AmountMinor,
    string? Currency,
    DateTime ExpenseDate,
    string? Description,
    string? ReceiptFile
);

public record UpdateExpenseRequest(
    int CategoryId,
    int AmountMinor,
    DateTime ExpenseDate,
    string? Description,
    string? ReceiptFile
);

public record ReviewRequest(int ReviewerId);
