using CheckMachAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents an inspection record for a specific machine, 
    /// including details such as date, description, notes, result, 
    /// and references to the machine and user who performed the inspection.
    /// </summary>
    public class Inspection
    {
        /// <summary>
        /// Gets or sets the unique identifier for the inspection.
        /// </summary>
        [Key]
        public int InspectionId { get; set; }

        /// <summary>
        /// Gets or sets the date when the inspection was performed.
        /// </summary>
        public DateTime InspectionDate { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the inspection.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets additional notes or remarks about the inspection.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the inspection passed.
        /// </summary>
        public bool Pass { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the associated machine.
        /// </summary>
        [ForeignKey(nameof(Machine))]
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the machine that was inspected.
        /// </summary>
        public Machine Machine { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the user who performed the inspection.
        /// </summary>
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the inspection.
        /// </summary>
        public ApplicationUser User { get; set; }
    }

}
