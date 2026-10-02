using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Hittar det dokumentunderlag som assistenten ska svara utifrån, oavsett om det kommer från lagringen eller ett sökindex.
public interface IDocumentRetriever
{
    Task<IReadOnlyList<DocumentSource>> FindSourcesAsync(string question, CancellationToken cancellationToken);

    bool CanRead(StoredDocument document);
}

public sealed record DocumentSource(string Name, string Text);
