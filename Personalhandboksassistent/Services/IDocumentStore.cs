using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Beskriver hur dokument hämtas och ändras utan att låsa API:t till en viss lagring.
public interface IDocumentStore
{
    IReadOnlyCollection<StoredDocument> GetAll();

    bool TryGet(string id, out StoredDocument? document);

    void Save(StoredDocument document);

    bool Remove(string id);
}