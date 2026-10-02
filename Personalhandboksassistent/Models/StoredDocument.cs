namespace Personalhandboksassistent.Models;

public sealed record StoredDocument(
    string Id,
    string Name,
    string Type,
    long Size,
    DateTimeOffset UpdatedAt)
{
    public bool IsPlainText => Type is "MD" or "TXT";

    public static string TypeFromFileName(string fileName) =>
        Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();
}
