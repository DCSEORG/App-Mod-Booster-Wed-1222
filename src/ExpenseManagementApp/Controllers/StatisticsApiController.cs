using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsApiController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<StatisticsApiController> _logger;

    public StatisticsApiController(IExpenseService expenseService, ILogger<StatisticsApiController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ExpenseStatistics>> GetStatistics()
    {
        try
        {
            var statistics = await _expenseService.GetExpenseStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetStatistics");
            return StatusCode(500, new { error = "An error occurred while fetching statistics", details = ex.Message });
        }
    }
}
