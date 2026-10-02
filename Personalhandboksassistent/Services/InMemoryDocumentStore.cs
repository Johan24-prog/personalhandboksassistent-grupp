using System.Collections.Concurrent;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public sealed class InMemoryDocumentStore : IDocumentStore
{
    private readonly ConcurrentDictionary<string, (StoredDocument Document, byte[] Content)> _documents =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyCollection<StoredDocument>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<StoredDocument>>(
            _documents.Values.Select(entry => entry.Document).ToArray());

    public Task<StoredDocument?> GetAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_documents.TryGetValue(id, out var entry) ? entry.Document : null);

    public async Task<StoredDocument> SaveAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        var document = new StoredDocument(
            fileName,
            fileName,
            StoredDocument.TypeFromFileName(fileName),
            buffer.Length,
            DateTimeOffset.UtcNow);
        _documents[fileName] = (document, buffer.ToArray());
        return document;
    }

    public Task<Stream?> OpenReadAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(
            _documents.TryGetValue(id, out var entry) ? new MemoryStream(entry.Content, writable: false) : null);

    public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_documents.TryRemove(id, out _));
}
