using CheckMachAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a maintenance record performed on a machine, 
    /// including details such as date, type, status, author, and 
    /// references to the related machine and user.
    /// </summary>
    public class Maintenance
    {
        /// <summary>
        /// Gets or sets the unique identifier for the maintenance record.
        /// </summary>
        [Key]
        public int MaintenanceId { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the maintenance work performed.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the maintenance was performed.
        /// </summary>
        public DateTime DatePerformed { get; set; }

        /// <summary>
        /// Gets or sets the type or category of maintenance (e.g., preventive, corrective).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the current status of the maintenance record (e.g., completed, pending).
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the maintenance record has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the user who created the maintenance record.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the maintenance record was created.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the identifier of the user who performed or logged the maintenance.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who performed or logged the maintenance.
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the machine associated with the maintenance.
        /// </summary>
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the machine on which the maintenance was performed.
        /// </summary>
        public Machine Machine { get; set; }
    }

}
