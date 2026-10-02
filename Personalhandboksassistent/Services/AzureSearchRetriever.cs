using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Söker fram de mest relevanta dokumenten i Azure AI Search. Indexern knäcker PDF och Word åt oss.
public sealed class AzureSearchRetriever(SearchClient searchClient, IConfiguration configuration) : IDocumentRetriever
{
    private const int MaximumExcerptLength = 8_000;
    private const int MaximumSourceCount = 5;
    private const string ContentField = "content";
    private const string NameField = "metadata_storage_name";

    public async Task<IReadOnlyList<DocumentSource>> FindSourcesAsync(
        string question,
        CancellationToken cancellationToken)
    {
        var options = new SearchOptions { Size = MaximumSourceCount };
        options.Select.Add(ContentField);
        options.Select.Add(NameField);

        var semanticConfiguration = configuration["AZURE_SEARCH_SEMANTIC_CONFIG"];
        if (!string.IsNullOrWhiteSpace(semanticConfiguration))
        {
            options.QueryType = SearchQueryType.Semantic;
            options.SemanticSearch = new SemanticSearchOptions { SemanticConfigurationName = semanticConfiguration };
        }

        var response = await searchClient.SearchAsync<SearchDocument>(question, options, cancellationToken);

        var sources = new List<DocumentSource>();
        await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
        {
            var name = result.Document.TryGetValue(NameField, out var nameValue) ? nameValue?.ToString() : null;
            var content = result.Document.TryGetValue(ContentField, out var contentValue) ? contentValue?.ToString() : null;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var excerpt = content.Length > MaximumExcerptLength ? content[..MaximumExcerptLength] : content;
            sources.Add(new DocumentSource(name, excerpt));
        }

        return sources;
    }

    // Indexern hanterar alla uppladdningsbara format.
    public bool CanRead(StoredDocument document) => true;
}
