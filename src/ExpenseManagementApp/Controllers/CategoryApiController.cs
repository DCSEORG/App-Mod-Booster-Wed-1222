using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryApiController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryApiController> _logger;

    public CategoryApiController(ICategoryService categoryService, ILogger<CategoryApiController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseCategory>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCategories");
            return StatusCode(500, new { error = "An error occurred while fetching categories", details = ex.Message });
        }
    }
}
