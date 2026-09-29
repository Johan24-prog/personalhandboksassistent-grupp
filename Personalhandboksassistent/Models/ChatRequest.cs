namespace Personalhandboksassistent.Models;

// Frågan som skickas från webbgränssnittet till chat-API:t.
public sealed record ChatRequest(string? Question);