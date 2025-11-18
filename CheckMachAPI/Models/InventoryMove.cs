using CheckMachAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a record of a movement (such as addition or removal) 
    /// of an item within an inventory, including quantity, type, date, 
    /// notes, and references to the user and inventory item involved.
    /// </summary>
    public class InventoryMove
    {
        /// <summary>
        /// Gets or sets the unique identifier for the inventory movement.
        /// </summary>
        [Key]
        public int InventoryMoveId { get; set; }

        /// <summary>
        /// Gets or sets the type of movement, such as "In", "Out", or "Adjustment".
        /// </summary>
        public string MovementType { get; set; }

        /// <summary>
        /// Gets or sets the quantity of items moved.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the movement occurred.
        /// </summary>
        public DateTime MovementDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes or comments about the movement.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the user who recorded the movement.
        /// </summary>
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who recorded the inventory movement.
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the associated inventory item.
        /// </summary>
        [ForeignKey(nameof(InventoryItem))]
        public int InventoryItemId { get; set; }

        /// <summary>
        /// Gets or sets the inventory item affected by this movement.
        /// </summary>
        public InventoryItem InventoryItem { get; set; }
    }

}
