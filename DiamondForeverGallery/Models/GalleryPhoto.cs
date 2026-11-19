using System.ComponentModel.DataAnnotations;

namespace DiamondForeverGallery.Models;

public class GalleryPhoto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; }
    
    [Required]
    public string ImageUrl { get; set; }

    [Required]
    public PhotoCategory Category { get; set; }
    public DateTime DateAdded { get; set; }
}


public enum PhotoCategory
{
    Engagement,
    Anniversary,
    Wedding,
    Birthday,
    Other
}

