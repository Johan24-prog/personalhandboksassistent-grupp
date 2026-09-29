namespace Personalhandboksassistent.Models;

// Den del av dokumentinformationen som API:t visar för webbläsaren.
public sealed record DocumentResponse(
    string Id,
    string Name,
    string Type,
    long Size,
    DateTimeOffset UpdatedAt,
    bool Searchable)
{
    public static DocumentResponse From(StoredDocument document) => new(
        document.Id,
        document.Name,
        document.Type,
        document.Size,
        document.UpdatedAt,
        document.Content is not null);
}