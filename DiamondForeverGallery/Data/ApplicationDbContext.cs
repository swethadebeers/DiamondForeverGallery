using DiamondForeverGallery.Models;
using Microsoft.EntityFrameworkCore;

namespace DiamondForeverGallery.Data;

/// <summary>
/// The database "gateway" for the app. Entity Framework Core uses it to read and write data.
/// It inherits from DbContext, which does the database work for us.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// A table of photos in the database. Use db.GalleryPhotos to query or add photos.
    /// </summary>
    public DbSet<GalleryPhoto> GalleryPhotos => Set<GalleryPhoto>();
}
