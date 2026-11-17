using DiamondForeverGallery.Data;
using DiamondForeverGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static DiamondForeverGallery.Pages.IndexModel;

namespace DiamondForeverGallery.Pages.Photos
{
    /// <summary>
    /// The code behind the Details page. It loads one photo from the database
    /// and gets a temporary link to its image so the page can display it.
    /// </summary>
    /// <param name="db">The database connection. ASP.NET Core provides it automatically (dependency injection).</param>
    /// <param name="storage">A helper service that creates links to images stored in the cloud.</param>
    public class DetailsModel(ApplicationDbContext db, PhotoStorageService storage) : PageModel
    {
        /// <summary>
        /// The photo shown on the page. The .cshtml file reads this with Model.Photo.
        /// </summary>
        public PhotoView Photo { get; set; } = null!;

        /// <summary>
        /// Runs when the page is requested with a GET request (when someone opens the page).
        /// </summary>
        /// <param name="id">The photo ID taken from the URL, for example /Photos/Details/5 gives 5.</param>
        /// <param name="cancellationToken">Lets ASP.NET Core cancel the work if the user leaves the page.</param>
        /// <returns>NotFound (404) if no photo has that ID; otherwise the page itself.</returns>
        public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
        {
            // Look for the photo with this ID. AsNoTracking means we only read the data and do not
            // need Entity Framework to track changes, which is faster for read-only pages.
            var photo = await db.GalleryPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if(photo is null)
            {
                // No photo with this ID exists, so show a "Not Found" response.
                return NotFound();
            }

            // Copy the database fields into a PhotoView, the simpler object the page uses.
            Photo = new PhotoView
            {
                Id = photo.Id,
                Title = photo.Title,
                Description = photo.Description,
                Category = photo.Category,
                DateAdded = photo.DateAdded,
                // The database stores only the image's file name (ImageUrl). This creates a link
                // that is valid for 1 hour so the browser can load the image.
                ImageUri = await storage.GetReadUriAsync(photo.ImageUrl, TimeSpan.FromHours(1), cancellationToken)
            };  
            // Show the page, using the Photo data set above.
            return Page();
        }
    }
}
