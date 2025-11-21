using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents an individual item that can be stored in an inventory, 
    /// including its identifying information, description, creation details, 
    /// and related inventory associations.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets the unique identifier for the item.
        /// </summary>
        [Key]
        public int ItemId { get; set; }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the item.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the item was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the user who created the item record.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the collection of inventory items that reference this item.
        /// </summary>
        public ICollection<InventoryItem> InventoryItems { get; set; }
    }

}
