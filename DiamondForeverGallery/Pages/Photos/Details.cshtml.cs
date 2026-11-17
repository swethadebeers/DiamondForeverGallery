using DiamondForeverGallery.Data;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static DiamondForeverGallery.Pages.IndexModel;

namespace DiamondForeverGallery.Pages.Photos
{
    public class DetailsModel(ApplicationDbContext db, PhotoStorageService storage) : PageModel
    {

        public PhotoView Photo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var photo = await db.GalleryPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if(photo is null)
            {
                return NotFound();
            }
            Photo = new PhotoView
            {
                Id = photo.Id,
                Title = photo.Title,
                Description = photo.Description,
                Category = photo.Category,
                DateAdded = photo.DateAdded,
                ImageUri = await storage.GetReadUriAsync(photo.ImageUrl, TimeSpan.FromHours(1), cancellationToken)
            };  
            return Page();
        }
    }
}
