using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public sealed class DocumentService(IDocumentStore documentStore)
{
    private const long MaximumFileSize = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".md", ".txt"
    };

    public async Task<DocumentSaveResult> SaveAsync(
        IFormFile? file,
        string? replacementId,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return DocumentSaveResult.Failure("Välj en fil att ladda upp.");
        }

        if (file.Length > MaximumFileSize)
        {
            return DocumentSaveResult.Failure("Filen får vara högst 20 MB.");
        }

        var fileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return DocumentSaveResult.Failure("Tillåtna format är PDF, Word, Markdown och ren text.");
        }

        var isSameDocument = string.Equals(fileName, replacementId, StringComparison.OrdinalIgnoreCase);
        if (!isSameDocument && await documentStore.GetAsync(fileName, cancellationToken) is not null)
        {
            return DocumentSaveResult.Failure(
                $"Det finns redan ett dokument med namnet {fileName}. Använd Ersätt på det dokumentet i stället.",
                StatusCodes.Status409Conflict);
        }

        await using var content = file.OpenReadStream();
        var document = await documentStore.SaveAsync(fileName, content, cancellationToken);

        if (replacementId is not null && !isSameDocument)
        {
            await documentStore.RemoveAsync(replacementId, cancellationToken);
        }

        return DocumentSaveResult.Success(document);
    }
}

public sealed record DocumentSaveResult(StoredDocument? Document, string? Error, int StatusCode)
{
    public static DocumentSaveResult Success(StoredDocument document) =>
        new(document, null, StatusCodes.Status200OK);

    public static DocumentSaveResult Failure(string error, int statusCode = StatusCodes.Status400BadRequest) =>
        new(null, error, statusCode);
}
