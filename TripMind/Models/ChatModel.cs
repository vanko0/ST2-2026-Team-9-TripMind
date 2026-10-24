namespace TripMind.Models;

public record ChatTurn(string Role, string Text);
public record ChatRequest(string Message, List<ChatTurn>? History);
public record ChatResponse(string Reply);