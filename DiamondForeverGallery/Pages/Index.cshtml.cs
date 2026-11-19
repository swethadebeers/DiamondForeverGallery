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
public class IndexModel(ApplicationDbContext db, PhotoStorageService storage) : PageModel
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