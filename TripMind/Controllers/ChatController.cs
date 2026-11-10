using Microsoft.AspNetCore.Mvc;
using TripMind.Models;
using TripMind.Services;

namespace TripMind.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _gemini;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGeminiService gemini, ILogger<ChatController> logger)
    {
        _gemini = gemini;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 500)
            return BadRequest(new { error = "Съобщението е празно или твърде дълго." });

        try
        {
            var preview = req.Message.Length > 60 ? req.Message[..60] + "..." : req.Message;
            AppLogger.Instance.Log("Chat message received", $"User asked: \"{preview}\"");

            var reply = await _gemini.AskAsync(req.Message, req.History);

            AppLogger.Instance.Log("Chat reply sent", $"Assistant answered ({reply.Length} characters)");
            return Ok(new ChatResponse(reply));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat failed");
            AppLogger.Instance.Log("Chat error", ex.Message);
            return StatusCode(503, new { error = "Чатботът не е наличен в момента. Опитай пак по-късно." });
        }
    }
}