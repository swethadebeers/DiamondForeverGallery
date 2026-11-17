using DiamondForeverGallery.Data;
using DiamondForeverGallery.Models;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiamondForeverGallery.Pages;

public class IndexModel(ApplicationDbContext db, PhotoStorageService storage) : PageModel
{
    public List<PhotoView> Photos { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public PhotoCategory? Category { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var query = db.GalleryPhotos.AsNoTracking();
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