using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserApiController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserApiController> _logger;

    public UserApiController(IUserService userService, ILogger<UserApiController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUsers");
            return StatusCode(500, new { error = "An error occurred while fetching users", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = $"User {id} not found" });
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUser for id {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the user", details = ex.Message });
        }
    }
}
