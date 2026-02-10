using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Hoard.Core.Services;

public interface IBlobStorageService
{
    Task<string> UploadContractNoteAsync(string reference, Stream fileStream, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType)> DownloadContractNoteAsync(string reference, CancellationToken ct = default);
    Task<bool> DeleteContractNoteAsync(string reference, CancellationToken ct = default);
    Task<bool> ContractNoteExistsAsync(string reference, CancellationToken ct = default);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private const string ContractNotesFolder = "contractnotes";

    public BlobStorageService(string connectionString, string containerName)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadContractNoteAsync(string reference, Stream fileStream, CancellationToken ct = default)
    {
        var blobName = GetBlobName(reference);
        var blobClient = _containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, ct);

        return blobClient.Uri.ToString();
    }

    public async Task<(Stream Stream, string ContentType)> DownloadContractNoteAsync(string reference, CancellationToken ct = default)
    {
        var blobName = GetBlobName(reference);
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync(ct);
        return (response.Value.Content, response.Value.ContentType);
    }

    public async Task<bool> DeleteContractNoteAsync(string reference, CancellationToken ct = default)
    {
        var blobName = GetBlobName(reference);
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        return response.Value;
    }

    public async Task<bool> ContractNoteExistsAsync(string reference, CancellationToken ct = default)
    {
        var blobName = GetBlobName(reference);
        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(ct);
    }

    private static string GetBlobName(string reference) => $"{ContractNotesFolder}/{reference}.pdf";
}
