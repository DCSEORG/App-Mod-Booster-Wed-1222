using ExpenseManagementApp.Models;
using ExpenseManagementApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatApiController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatApiController> _logger;

    public ChatApiController(IChatService chatService, ILogger<ChatApiController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ChatResponse { Response = "Message cannot be empty" });
        }

        try
        {
            var response = await _chatService.ProcessChatMessageAsync(request.Message);
            return Ok(new ChatResponse { Response = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new ChatResponse { Response = "An error occurred while processing your message. Please try again." });
        }
    }

    [HttpGet("status")]
    public ActionResult<object> GetStatus()
    {
        var isConfigured = _chatService.IsConfigured();
        return Ok(new
        {
            configured = isConfigured,
            message = isConfigured
                ? "GenAI services are configured and ready"
                : "GenAI services are not deployed. Please run deploy-with-chat.sh to enable AI features."
        });
    }
}
