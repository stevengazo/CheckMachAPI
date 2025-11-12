using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a machine within the system, including its identifying details, 
    /// model information, location, purchase date, availability status, and 
    /// related maintenance, inspection, and time tracking records.
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// Gets or sets the unique identifier for the machine.
        /// </summary>
        [Key]
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the model designation of the machine.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the location where the machine is installed or used.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the machine.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the machine was purchased.
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the machine has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the machine is currently available for use.
        /// </summary>
        public bool Avariable { get; set; }

        /// <summary>
        /// Gets or sets the collection of time records associated with this machine.
        /// </summary>
        public ICollection<TimePeerProject> Times { get; set; }

        /// <summary>
        /// Gets or sets the collection of maintenance records associated with this machine.
        /// </summary>
        public ICollection<Maintenance> Maintenances { get; set; }

        /// <summary>
        /// Gets or sets the collection of inspection records associated with this machine.
        /// </summary>
        public ICollection<Inspection> Inspections { get; set; }
    }

}
