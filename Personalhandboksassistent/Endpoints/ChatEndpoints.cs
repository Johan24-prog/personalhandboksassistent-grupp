using Personalhandboksassistent.Models;
using Personalhandboksassistent.Services;

namespace Personalhandboksassistent.Endpoints;

// Tar emot personalfrågor och lämnar AI-arbetet till AzureOpenAiService.
public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/chat", async (
            ChatRequest request,
            AzureOpenAiService assistant,
            CancellationToken cancellationToken) =>
        {
            var question = request.Question?.Trim();
            if (string.IsNullOrWhiteSpace(question) || question.Length > 2000)
            {
                return Results.BadRequest(new { error = "Skriv en fråga på högst 2 000 tecken." });
            }

            var result = await assistant.GetAnswerAsync(question, cancellationToken);
            if (result.Error is not null)
            {
                return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
            }

            return Results.Ok(new
            {
                answer = result.Answer,
                sources = result.Sources
            });
        });

        return endpoints;
    }
}