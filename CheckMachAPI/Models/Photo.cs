using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a photo record stored in the system, including its file path, 
    /// description, reference to another entity, and type classification.
    /// </summary>
    public class Photo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the photo.
        /// </summary>
        [Key]
        public int PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the file path or location where the photo is stored.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the photo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the entity this photo is associated with 
        /// (for example, a machine, inspection, or maintenance record).
        /// </summary>
        public int ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the type or category of the photo (e.g., "Machine", "Inspection", "Maintenance").
        /// </summary>
        public string? PhotoType { get; set; }
    }

}
