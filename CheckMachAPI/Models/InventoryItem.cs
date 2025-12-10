using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents an item entry within an inventory, including quantity, 
    /// minimum stock level, creation date, and references to the related 
    /// item, inventory, and inventory movements.
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the inventory item.
        /// </summary>
        [Key]
        public int InventoryItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the item currently in stock.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the minimum stock level required for this item.
        /// </summary>
        public int MinStock { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this inventory item record was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the associated item.
        /// </summary>
        [ForeignKey(nameof(Item))]
        public int ItemId { get; set; }

        /// <summary>
        /// Gets or sets the item details associated with this inventory record.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the associated inventory.
        /// </summary>
        [ForeignKey(nameof(Inventory))]
        public int InventoryId { get; set; }

        /// <summary>
        /// Gets or sets the inventory to which this item belongs.
        /// </summary>
        public Inventory? Inventory { get; set; }

        /// <summary>
        /// Gets or sets the collection of inventory movements associated with this item.
        /// </summary>
        public ICollection<InventoryMove>? InventoryMoves { get; set; }
    }

}
