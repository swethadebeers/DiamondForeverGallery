using System.ComponentModel.DataAnnotations;
using DiamondForeverGallery.Data;
using DiamondForeverGallery.Models;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiamondForeverGallery.Pages;

public class UploadModel(ApplicationDbContext db, PhotoStorageService  storage) : PageModel
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxFileBytes = 10 * 1024 * 1024;

    [BindProperty]
    public InputModel Input { get; set; } = new();

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

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
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

        if (!ModelState.IsValid || file is null)
        {
            return Page();
        }

        await using var stream = file.OpenReadStream();
        var blobName = await storage.UploadAsync(stream, file.ContentType, Path.GetExtension(file.FileName).ToLowerInvariant(), cancellationToken);

        db.GalleryPhotos.Add(new GalleryPhoto
        {
            Title = Input.Title,
            Description = Input.Story,
            Category = Input.Category,
            ImageUrl = blobName,
            DateAdded = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Index");
    }
}

