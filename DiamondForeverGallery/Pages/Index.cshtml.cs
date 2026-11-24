using Azure;
using DiamondForeverGallery.Data;
using DiamondForeverGallery.Models;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiamondForeverGallery.Pages;

/// <summary>
/// Code behind the Gallery (home) page. Loads the list of photos, optionally filtered by category.
/// </summary>
public class IndexModel(ApplicationDbContext db, PhotoStorageService storage, ILogger<IndexModel> logger) : PageModel
{
    /// <summary>
    /// The photos shown on the page, newest first.
    /// </summary>
    public List<PhotoView> Photos { get; set; } = [];

    // Reads the "Category" value from the URL query string (for example ?Category=Wedding).
    [BindProperty(SupportsGet = true)]
    public PhotoCategory? Category { get; set; }

    /// <summary>
    /// Runs when the Gallery page is opened. Loads photos and builds their image links.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var query = db.GalleryPhotos.AsNoTracking();
        // Only filter when the user picked a category; otherwise show all photos.
        if(Category is not null)
        {
            query = query.Where(p => p.Category == Category);
        }
        var photos = await query
            .OrderByDescending(p => p.DateAdded)
            .ToListAsync(cancellationToken);
        var views = new List<PhotoView>();
        foreach (var p in photos)
        {
            views.Add(new PhotoView
            {
                Id = p.Id,
                Title = p.Title,
                Category = p.Category,
                DateAdded = p.DateAdded,
                ImageUri = await storage.GetReadUriAsync(p.ImageUrl, TimeSpan.FromHours(1), cancellationToken)
            });
        }

        Photos = views;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var photo = await db.GalleryPhotos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (photo is null)
        {
            return NotFound();
        }

        try
        {
            await storage.DeleteAsync(photo.ImageUrl, cancellationToken);
            db.GalleryPhotos.Remove(photo);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to delete the stored image for gallery photo {PhotoId}.", id);
            ModelState.AddModelError(string.Empty, "The image could not be deleted from storage, so the jewelry item was not removed.");
            await OnGetAsync(cancellationToken);
            return Page();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to delete gallery photo {PhotoId} from the database after deleting its image.", id);
            ModelState.AddModelError(string.Empty, "The image was deleted from storage, but the jewelry item could not be removed from the collection. Submit the delete action again to finish.");
            await OnGetAsync(cancellationToken);
            return Page();
        }

        return RedirectToPage(new { Category });
    }

    /// <summary>
    /// A simplified photo object for the page. It includes the ready-to-use image link (ImageUri).
    /// </summary>
    public class PhotoView
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PhotoCategory Category { get; set; }
        public DateTime DateAdded { get; set; }
        public Uri ImageUri { get; set; } = null!;
    }
}