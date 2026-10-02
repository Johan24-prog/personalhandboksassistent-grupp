using System.Text;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Reserv utan Azure AI Search: läser Markdown- och textfiler direkt från lagringen.
public sealed class StoredTextRetriever(IDocumentStore documentStore) : IDocumentRetriever
{
    private const int MaximumExcerptLength = 8_000;
    private const int MaximumSourceCount = 12;

    public async Task<IReadOnlyList<DocumentSource>> FindSourcesAsync(
        string question,
        CancellationToken cancellationToken)
    {
        var documents = (await documentStore.GetAllAsync(cancellationToken))
            .Where(CanRead)
            .OrderByDescending(document => document.UpdatedAt)
            .Take(MaximumSourceCount);

        var sources = new List<DocumentSource>();
        foreach (var document in documents)
        {
            await using var stream = await documentStore.OpenReadAsync(document.Id, cancellationToken);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var buffer = new char[MaximumExcerptLength];
            var length = await reader.ReadBlockAsync(buffer, cancellationToken);
            sources.Add(new DocumentSource(document.Name, new string(buffer, 0, length)));
        }

        return sources;
    }

    public bool CanRead(StoredDocument document) => document.IsPlainText;
}
