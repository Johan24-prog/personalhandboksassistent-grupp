using System.Collections.Concurrent;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// En enkel tillfällig lagring som ersätts med Blob Storage senare.
public sealed class InMemoryDocumentStore : IDocumentStore
{
    private readonly ConcurrentDictionary<string, StoredDocument> _documents = new();

    public IReadOnlyCollection<StoredDocument> GetAll() => _documents.Values.ToArray();

    public bool TryGet(string id, out StoredDocument? document) =>
        _documents.TryGetValue(id, out document);

    public void Save(StoredDocument document) => _documents[document.Id] = document;

    public bool Remove(string id) => _documents.TryRemove(id, out _);
}