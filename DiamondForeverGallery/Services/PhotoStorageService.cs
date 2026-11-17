using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace DiamondForeverGallery.Services;

/// <summary>
/// Handles the photo files in Azure Blob Storage: uploading them and creating temporary, secure links to view them.
/// </summary>
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

    /// <summary>
    /// Saves a file to blob storage under a unique name and returns that name.
    /// </summary>
    public async Task<string> UploadAsync(
    {
        var blobName = $"{Guid.NewGuid():N}{fileExtension}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blobName;
    }

    /// <summary>
    /// Creates a link to a blob that works only for a limited time (a "SAS" link).
    /// </summary>
    public async Task<Uri> GetReadUriAsync(
    {
        var blob = _container.GetBlobClient(blobName);
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5);
        var expiresOn = DateTimeOffset.UtcNow.Add(validFor);

        // A key from Azure AD (via DefaultAzureCredential) used to sign the link without storing account keys.
        var userDelegationKey

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
