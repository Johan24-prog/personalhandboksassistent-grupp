using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Personalhandboksassistent.Configuration;
using Personalhandboksassistent.Endpoints;
using Personalhandboksassistent.Models;
using Personalhandboksassistent.Services;

// .NET läser miljövariabler automatiskt, men inte lokala .env-filer.
DotEnvLoader.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

// Tjänsterna gör arbetet åt API-rutterna och kan bytas ut eller testas separat.
var storageConnectionString = builder.Configuration["AZURE_STORAGE_CONNECTION_STRING"];
var storageContainerName = builder.Configuration["AZURE_STORAGE_CONTAINER_NAME"];
if (!string.IsNullOrWhiteSpace(storageConnectionString) && !string.IsNullOrWhiteSpace(storageContainerName))
{
    builder.Services.AddSingleton(new BlobContainerClient(storageConnectionString, storageContainerName));
    builder.Services.AddSingleton<IDocumentStore, BlobDocumentStore>();
}
else
{
    builder.Services.AddSingleton<IDocumentStore, InMemoryDocumentStore>();
}

// Med Azure AI Search hämtas underlaget från sökindexet, annars läses MD/TXT direkt ur lagringen.
var searchEndpoint = builder.Configuration["AZURE_SEARCH_ENDPOINT"];
var searchApiKey = builder.Configuration["AZURE_SEARCH_API_KEY"];
var searchIndexName = builder.Configuration["AZURE_SEARCH_INDEX_NAME"];
var searchIndexerName = builder.Configuration["AZURE_SEARCH_INDEXER_NAME"];
if (!string.IsNullOrWhiteSpace(searchEndpoint)
    && !string.IsNullOrWhiteSpace(searchApiKey)
    && !string.IsNullOrWhiteSpace(searchIndexName))
{
    var searchCredential = new AzureKeyCredential(searchApiKey);
    builder.Services.AddSingleton(new SearchClient(new Uri(searchEndpoint), searchIndexName, searchCredential));
    builder.Services.AddSingleton<IDocumentRetriever, AzureSearchRetriever>();

    if (!string.IsNullOrWhiteSpace(searchIndexerName))
    {
        builder.Services.AddSingleton(new SearchIndexerClient(new Uri(searchEndpoint), searchCredential));
        builder.Services.AddSingleton<IIndexRefresher>(services => new SearchIndexerRefresher(
            services.GetRequiredService<SearchIndexerClient>(),
            searchIndexerName,
            services.GetRequiredService<ILogger<SearchIndexerRefresher>>()));
    }
    else
    {
        builder.Services.AddSingleton<IIndexRefresher, NoOpIndexRefresher>();
    }
}
else
{
    builder.Services.AddSingleton<IDocumentRetriever, StoredTextRetriever>();
    builder.Services.AddSingleton<IIndexRefresher, NoOpIndexRefresher>();
}
builder.Services.AddScoped<DocumentService>();
builder.Services.AddHttpClient<AzureOpenAiService>();

var app = builder.Build();

// Servera webbgränssnittet från wwwroot.
app.UseDefaultFiles();
if (app.Environment.IsDevelopment())
{
    // Undviker att webbläsaren cachar gamla HTML/CSS/JS-versioner under utveckling.
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = context =>
        {
            context.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Context.Response.Headers.Pragma = "no-cache";
            context.Context.Response.Headers.Expires = "0";
        },
    });
}
else
{
    app.UseStaticFiles();
}

// Varje grupp samlar API-rutter för ett eget område.
app.MapStatusEndpoints();
app.MapDocumentEndpoints();
app.MapChatEndpoints();

app.Run();

