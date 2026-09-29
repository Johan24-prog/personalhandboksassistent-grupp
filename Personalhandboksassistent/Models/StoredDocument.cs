namespace Personalhandboksassistent.Models;

// Metadata och läsbar text för ett dokument i prototypens minneslager.
public sealed record StoredDocument(
    string Id,
    string Name,
    string Type,
    long Size,
    DateTimeOffset UpdatedAt,
    string? Content);