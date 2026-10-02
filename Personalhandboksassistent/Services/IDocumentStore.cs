using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Beskriver hur dokument hämtas och ändras utan att låsa API:t till en viss lagring.
public interface IDocumentStore
{
    Task<IReadOnlyCollection<StoredDocument>> GetAllAsync(CancellationToken cancellationToken);

    Task<StoredDocument?> GetAsync(string id, CancellationToken cancellationToken);

    Task<StoredDocument> SaveAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string id, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(string id, CancellationToken cancellationToken);
}
