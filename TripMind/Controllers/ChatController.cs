using Microsoft.AspNetCore.Mvc;
using TripMind.Models;
using TripMind.Services;

namespace TripMind.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _gemini;

    public ChatController(IGeminiService gemini)
    {
        _gemini = gemini;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 500)
            return BadRequest(new { error = "Съобщението е празно или твърде дълго." });

        try
        {
            AppLogger.Instance.Log("Chat message received"); // без текста: /Trips/Log е публична страница
            var reply = await _gemini.AskAsync(req.Message, req.History);
            return Ok(new ChatResponse(reply));
        }
        catch (Exception ex)
        {
            AppLogger.Instance.Log($"Chat error: {ex.Message}");
            return StatusCode(503, new { error = "Чатботът не е наличен в момента. Опитай пак по-късно." });
        }
    }
}