using System.ComponentModel.DataAnnotations;

namespace DiamondForeverGallery.Models;

/// <summary>
/// Represents one photo row in the database.
/// [Required] means the field cannot be empty. [StringLength(n)] sets the maximum number of characters.
/// </summary>
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
    /// <summary>
        /// The file name of the image in Azure Blob Storage (not a full web link).
        /// </summary>
        public string ImageUrl { get; set; }

    [Required]
    public PhotoCategory Category { get; set; }
    public DateTime DateAdded { get; set; }
}


/// <summary>
/// The kinds of occasions a photo can belong to. Each value is stored as a choice in dropdown lists.
/// </summary>
public enum PhotoCategory
{
    Engagement,
    Anniversary,
    Wedding,
    Birthday,
    Other
}

