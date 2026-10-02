using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

// Söker fram de mest relevanta dokumenten i Azure AI Search. Indexern knäcker PDF och Word åt oss.
public sealed class AzureSearchRetriever(SearchClient searchClient, IConfiguration configuration) : IDocumentRetriever
{
    private const int MaximumExcerptLength = 4_000;
    private const int MaximumSourceCount = 5;
    private const string ContentField = "content";
    private const string NameField = "metadata_storage_name";

    public async Task<IReadOnlyList<DocumentSource>> FindSourcesAsync(
        string question,
        CancellationToken cancellationToken)
    {
        var options = new SearchOptions
        {
            Size = MaximumSourceCount,
            HighlightPreTag = "<search-hit>",
            HighlightPostTag = "</search-hit>"
        };

        options.HighlightFields.Add(ContentField);
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

            var excerpt = GetRelevantExcerpt(result, content);

            if (string.IsNullOrWhiteSpace(excerpt))
            {
                continue;
            }

            sources.Add(new DocumentSource(name, excerpt));
        }

        return sources;
    }
    private static string? GetRelevantExcerpt(
    SearchResult<SearchDocument> result,
    string content)
    {
        if (result.Highlights is not null
            && result.Highlights.TryGetValue(ContentField, out var highlights))
        {
            var passages = highlights
                .Select(text => text
                    .Replace("<search-hit>", "")
                    .Replace("</search-hit>", "")
                    .Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct()
                .Take(5);

            // Markera att utdragen kan komma från olika delar av dokumentet.
            var excerpt = string.Join("\n\n[...]\n\n", passages);

            if (!string.IsNullOrWhiteSpace(excerpt))
            {
                return excerpt.Length > MaximumExcerptLength
                    ? excerpt[..MaximumExcerptLength]
                    : excerpt;
            }
        }

        // Ett kort dokument får plats i sin helhet.
        // För långa dokument behöver vi ett relevant utdrag.
        return content.Length <= MaximumExcerptLength
            ? content
            : null;
    }

    // Indexern hanterar alla uppladdningsbara format.
    public bool CanRead(StoredDocument document) => true;
}
