using System.Text;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Validerar uppladdningar och omvandlar läsbara filer till dokumentposter.
public sealed class DocumentService(IDocumentStore documentStore)
{
    private const long MaximumFileSize = 20 * 1024 * 1024;
    private const int MaximumTextLength = 50_000;

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

        // PDF och Word sparas som dokumentposter, men textutvinning är inte inkopplad ännu.
        var content = await ReadTextContentAsync(file, extension, cancellationToken);
        var id = replacementId ?? Guid.NewGuid().ToString("N");
        var document = new StoredDocument(
            id,
            fileName,
            extension.TrimStart('.').ToUpperInvariant(),
            file.Length,
            DateTimeOffset.UtcNow,
            content);

        documentStore.Save(document);
        return DocumentSaveResult.Success(document);
    }

    private static async Task<string?> ReadTextContentAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var isTextFile = extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".md", StringComparison.OrdinalIgnoreCase);
        if (!isTextFile)
        {
            return null;
        }

        using var reader = new StreamReader(
            file.OpenReadStream(),
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return text[..Math.Min(text.Length, MaximumTextLength)];
    }
}

// Antingen det sparade dokumentet eller ett begripligt valideringsfel.
public sealed record DocumentSaveResult(StoredDocument? Document, string? Error)
{
    public static DocumentSaveResult Success(StoredDocument document) => new(document, null);

    public static DocumentSaveResult Failure(string error) => new(null, error);
}