using System.ComponentModel.DataAnnotations;
using DiamondForeverGallery.Data;
using DiamondForeverGallery.Models;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiamondForeverGallery.Pages;

/// <summary>
/// Code behind the Upload page. Checks the uploaded photo, saves it to storage, then saves its details to the database.
/// </summary>
public class UploadModel(
{
    // Allowed file types. Checked against both the MIME type and the file extension.
    private static readonly string[] AllowedContentTypes
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    // 10 MB limit (10 x 1024 x 1024 bytes).
    private const long MaxFileBytes

    // Holds the form values. [BindProperty] copies the submitted form data into this object.
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// The form fields the user fills in. Validation attributes (like [Required]) are checked automatically.
    /// </summary>
    public class InputModel
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Story { get; set; } = string.Empty;

        [Required]
        public PhotoCategory Category { get; set; }

        [Required]
        public IFormFile? Photo { get; set; }
    }

    public void OnGet()
    {
    }

    /// <summary>
    /// Runs when the user clicks Upload (a POST request). Returns the form again if something is wrong.
    /// </summary>
    public async Task<IActionResult> OnPostAsync(
    {
        var file = Input.Photo;
        if (file is not null)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedContentTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Input.Photo", "Please upload a JPG, PNG, GIF, or WebP image.");
            }
            else if (file.Length > MaxFileBytes)
            {
                ModelState.AddModelError("Input.Photo", "The image must be 10 MB or smaller.");
            }
        }

        // If any validation error exists, redisplay the form so the user can fix it.
        if (!ModelState.IsValid || file is null)
        {
            return Page();
        }

        // Open the file and upload it to blob storage. "await using" closes the stream when done.
        await using var stream = file.OpenReadStream();
        var blobName = await storage.UploadAsync(stream, file.ContentType, Path.GetExtension(file.FileName).ToLowerInvariant(), cancellationToken);

        // Save the photo's details. The image file itself lives in storage; the database keeps its name.
        db.GalleryPhotos.Add(new GalleryPhoto
        {
            Title = Input.Title,
            Description = Input.Story,
            Category = Input.Category,
            ImageUrl = blobName,
            DateAdded = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        // Success: send the user back to the Gallery page (PRG pattern prevents duplicate uploads on refresh).
        return RedirectToPage("/Index");
    }
}
