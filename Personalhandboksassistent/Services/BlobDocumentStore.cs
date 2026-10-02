using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.StaticFiles;
using Personalhandboksassistent.Models;

namespace Personalhandboksassistent.Services;

public sealed class BlobDocumentStore(BlobContainerClient container) : IDocumentStore
{
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public async Task<IReadOnlyCollection<StoredDocument>> GetAllAsync(CancellationToken cancellationToken)
    {
        var documents = new List<StoredDocument>();
        await foreach (var blob in container.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            documents.Add(ToDocument(
                blob.Name,
                blob.Properties.ContentLength ?? 0,
                blob.Properties.LastModified));
        }

        return documents;
    }

    public async Task<StoredDocument?> GetAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var properties = await container.GetBlobClient(id).GetPropertiesAsync(cancellationToken: cancellationToken);
            return ToDocument(id, properties.Value.ContentLength, properties.Value.LastModified);
        }
        catch (RequestFailedException exception) when (exception.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<StoredDocument> SaveAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        if (!ContentTypes.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var blob = container.GetBlobClient(fileName);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken);

        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);
        return ToDocument(fileName, properties.Value.ContentLength, properties.Value.LastModified);
    }

    public async Task<Stream?> OpenReadAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            return await container.GetBlobClient(id).OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<bool> RemoveAsync(string id, CancellationToken cancellationToken)
    {
        var response = await container.GetBlobClient(id).DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    private static StoredDocument ToDocument(string name, long size, DateTimeOffset? updatedAt) => new(
        name,
        Path.GetFileName(name),
        StoredDocument.TypeFromFileName(name),
        size,
        updatedAt ?? DateTimeOffset.UtcNow);
}
