using DiamondForeverGallery.Models;
using Microsoft.EntityFrameworkCore;

namespace DiamondForeverGallery.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<GalleryPhoto> GalleryPhotos => Set<GalleryPhoto>();
}
