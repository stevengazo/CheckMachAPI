using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents an inventory record, including its name, location, 
    /// last update information, and related inventory items.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the inventory.
        /// </summary>
        [Key]
        public int InventoryId { get; set; }

        /// <summary>
        /// Gets or sets the name of the inventory.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the location where the inventory is stored or managed.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the inventory was last updated.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the user who created the inventory record.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the user who last edited the inventory record.
        /// </summary>
        public string? LastEditor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the inventory record has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the collection of inventory items associated with this inventory.
        /// </summary>
        public ICollection<InventoryItem>? InventoryItems { get; set; }
    }

}
