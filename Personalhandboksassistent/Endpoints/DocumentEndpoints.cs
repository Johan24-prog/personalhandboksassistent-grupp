using Personalhandboksassistent.Models;
using Personalhandboksassistent.Services;

namespace Personalhandboksassistent.Endpoints;

// HTTP-rutter för att lista, lägga till, ersätta och ta bort dokument.
public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var documents = endpoints.MapGroup("/api/documents");

        documents.MapGet("", async (
            IDocumentStore store,
            IDocumentRetriever retriever,
            CancellationToken cancellationToken) =>
            Results.Ok((await store.GetAllAsync(cancellationToken))
                .OrderByDescending(document => document.UpdatedAt)
                .Select(document => DocumentResponse.From(document, retriever.CanRead(document)))));

        documents.MapPost("", async (
            HttpRequest request,
            DocumentService service,
            IDocumentRetriever retriever,
            CancellationToken cancellationToken) =>
        {
            var formResult = await ReadFormAsync(request, cancellationToken);
            if (formResult.Error is not null)
            {
                return Results.BadRequest(new { error = formResult.Error });
            }

            var result = await service.SaveAsync(
                formResult.Form!.Files.GetFile("file"),
                replacementId: null,
                cancellationToken);
            return ToHttpResult(result, retriever);
        });

        documents.MapPut("/{id}", async (
            string id,
            HttpRequest request,
            IDocumentStore store,
            DocumentService service,
            IDocumentRetriever retriever,
            CancellationToken cancellationToken) =>
        {
            if (await store.GetAsync(id, cancellationToken) is null)
            {
                return Results.NotFound(new { error = "Dokumentet finns inte längre." });
            }

            var formResult = await ReadFormAsync(request, cancellationToken);
            if (formResult.Error is not null)
            {
                return Results.BadRequest(new { error = formResult.Error });
            }

            var result = await service.SaveAsync(
                formResult.Form!.Files.GetFile("file"),
                id,
                cancellationToken);
            return ToHttpResult(result, retriever);
        });

        documents.MapDelete("/{id}", async (string id, DocumentService service, CancellationToken cancellationToken) =>
            await service.RemoveAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound());

        return endpoints;
    }

    private static async Task<FormReadResult> ReadFormAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return new FormReadResult(null, "Skicka dokumentet som multipart/form-data.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        return new FormReadResult(form, null);
    }

    private static IResult ToHttpResult(DocumentSaveResult result, IDocumentRetriever retriever)
    {
        if (result.Error is not null)
        {
            return Results.Json(new { error = result.Error }, statusCode: result.StatusCode);
        }

        return Results.Ok(DocumentResponse.From(result.Document!, retriever.CanRead(result.Document!)));
    }

    private sealed record FormReadResult(IFormCollection? Form, string? Error);
}