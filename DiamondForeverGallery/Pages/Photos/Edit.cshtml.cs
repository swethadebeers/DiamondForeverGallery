using System.ComponentModel.DataAnnotations;
using Azure;
using DiamondForeverGallery.Data;
using DiamondForeverGallery.Models;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiamondForeverGallery.Pages.Photos;

public class EditModel(ApplicationDbContext db, PhotoStorageService storage, ILogger<EditModel> logger) : PageModel
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxFileBytes = 10 * 1024 * 1024;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Uri ImageUri { get; private set; } = null!;
    public string? StatusMessage { get; private set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public PhotoCategory Category { get; set; }

        public IFormFile? Photo { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var photo = await db.GalleryPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (photo is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = photo.Id,
            Title = photo.Title,
            Description = photo.Description,
            Category = photo.Category
        };
        await SetImageUriAsync(photo.ImageUrl, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        if (id != Input.Id)
        {
            return BadRequest();
        }

        var photo = await db.GalleryPhotos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (photo is null)
        {
            return NotFound();
        }

        var file = Input.Photo;
        var extension = file is null ? null : Path.GetExtension(file.FileName).ToLowerInvariant();
        if (file is not null)
        {
            if (!AllowedContentTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(extension!))
            {
                ModelState.AddModelError("Input.Photo", "Please upload a JPG, PNG, GIF, or WebP image.");
            }
            else if (file.Length > MaxFileBytes)
            {
                ModelState.AddModelError("Input.Photo", "The image must be 10 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            await SetImageUriAsync(photo.ImageUrl, cancellationToken);
            return Page();
        }

        string? replacementBlobName = null;
        if (file is not null)
        {
            await using var stream = file.OpenReadStream();
            replacementBlobName = await storage.UploadAsync(stream, file.ContentType, extension!, cancellationToken);
        }

        var previousBlobName = photo.ImageUrl;
        var previousTitle = photo.Title;
        var previousDescription = photo.Description;
        var previousCategory = photo.Category;
        photo.Title = Input.Title;
        photo.Description = Input.Description;
        photo.Category = Input.Category;
        if (replacementBlobName is not null)
        {
            photo.ImageUrl = replacementBlobName;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update gallery photo {PhotoId}.", id);
            var cleanupFailed = false;
            if (replacementBlobName is not null)
            {
                try
                {
                    await storage.DeleteAsync(replacementBlobName, cancellationToken);
                }
                catch (RequestFailedException cleanupException)
                {
                    cleanupFailed = true;
                    logger.LogError(cleanupException, "Failed to clean up replacement image for gallery photo {PhotoId}.", id);
                }
            }

            photo.Title = previousTitle;
            photo.Description = previousDescription;
            photo.Category = previousCategory;
            photo.ImageUrl = previousBlobName;
            db.Entry(photo).State = EntityState.Unchanged;
            ModelState.AddModelError(string.Empty, cleanupFailed
                ? "The jewelry item could not be updated, and the uploaded replacement image could not be removed from storage. Please contact support."
                : "The jewelry item could not be updated. Please try again.");
            await SetImageUriAsync(previousBlobName, cancellationToken);
            return Page();
        }

        if (replacementBlobName is not null)
        {
            try
            {
                await storage.DeleteAsync(previousBlobName, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                logger.LogError(ex, "Updated gallery photo {PhotoId}, but failed to remove its previous image.", id);
                StatusMessage = "Your changes were saved, but the previous image could not be removed from storage.";
                await SetImageUriAsync(photo.ImageUrl, cancellationToken);
                return Page();
            }
        }

        return RedirectToPage("/Index");
    }

    private async Task SetImageUriAsync(string blobName, CancellationToken cancellationToken)
    {
        ImageUri = await storage.GetReadUriAsync(blobName, TimeSpan.FromHours(1), cancellationToken);
    }
}
