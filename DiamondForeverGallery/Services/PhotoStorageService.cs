using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace DiamondForeverGallery.Services;

public class PhotoStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _container;

    public PhotoStorageService(IConfiguration configuration)
    {
        var accountUri = configuration["BlobStorage:AccountUri"]
            ?? throw new InvalidOperationException("BlobStorage:AccountUri is not configured.");
        var containerName = configuration["BlobStorage:ContainerName"] ?? "photos";

        _serviceClient = new BlobServiceClient(new Uri(accountUri), new DefaultAzureCredential());
        _container = _serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string fileExtension, CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid():N}{fileExtension}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blobName;
    }

    public async Task<Uri> GetReadUriAsync(string blobName, TimeSpan validFor, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(blobName);
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expiresOn = DateTimeOffset.UtcNow.Add(validFor);

        var userDelegationKey = await _serviceClient.GetUserDelegationKeyAsync(startsOn, expiresOn, cancellationToken);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = startsOn,
            ExpiresOn = expiresOn
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, _serviceClient.AccountName).ToString();
        return new Uri($"{blob.Uri}?{sasToken}");
    }
}
