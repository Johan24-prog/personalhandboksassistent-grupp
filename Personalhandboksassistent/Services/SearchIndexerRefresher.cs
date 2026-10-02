using Azure;
using Azure.Search.Documents.Indexes;

namespace Personalhandboksassistent.Services;

// Startar Azure AI Search-indexern direkt så att ändringar syns utan att någon kör om indexet manuellt.
public sealed class SearchIndexerRefresher(
    SearchIndexerClient indexerClient,
    string indexerName,
    ILogger<SearchIndexerRefresher> logger) : IIndexRefresher
{
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            await indexerClient.RunIndexerAsync(indexerName, cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == StatusCodes.Status409Conflict)
        {
            // Indexern kör redan; ändringen plockas upp av pågående eller schemalagd körning.
            logger.LogInformation("Indexern {IndexerName} körs redan.", indexerName);
        }
        catch (RequestFailedException exception)
        {
            // Dokumentet är redan sparat, så ett indexeringsfel ska inte fälla anropet.
            logger.LogWarning(exception, "Kunde inte starta indexern {IndexerName}.", indexerName);
        }
    }
}
