namespace Personalhandboksassistent.Endpoints;

// Rutter som visar om nödvändiga Azure-inställningar är ifyllda.
public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/status", (IConfiguration configuration) => Results.Ok(new
        {
            storageConfigured = HasValues(
                configuration,
                "AZURE_STORAGE_CONNECTION_STRING",
                "AZURE_STORAGE_CONTAINER_NAME"),
            searchConfigured = HasValues(
                configuration,
                "AZURE_SEARCH_ENDPOINT",
                "AZURE_SEARCH_API_KEY",
                "AZURE_SEARCH_INDEX_NAME"),
            openAiConfigured = HasValues(
                configuration,
                "AZURE_OPENAI_ENDPOINT",
                "AZURE_OPENAI_API_KEY",
                "AZURE_OPENAI_CHAT_DEPLOYMENT"),
            chatDeployment = configuration["AZURE_OPENAI_CHAT_DEPLOYMENT"] ?? ""
        }));

        return endpoints;
    }

    private static bool HasValues(IConfiguration configuration, params string[] keys) =>
        keys.All(key => !string.IsNullOrWhiteSpace(configuration[key]));
}