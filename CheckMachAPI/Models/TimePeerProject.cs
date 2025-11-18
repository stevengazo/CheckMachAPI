using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a record of time worked on a specific project, 
    /// including hours worked, date, and associated project and machine.
    /// </summary>
    public class TimePeerProject
    {
        /// <summary>
        /// Gets or sets the unique identifier for the time record.
        /// </summary>
        [Key]
        public int TimePeerProjectId { get; set; }

        /// <summary>
        /// Gets or sets the number of hours worked during this time entry.
        /// </summary>
        public double HoursWorked { get; set; }

        /// <summary>
        /// Gets or sets the date on which the work was performed.
        /// </summary>
        public DateTime DateWorked { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the associated project.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the project associated with this time entry.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the machine used during this time entry.
        /// </summary>
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the machine associated with this time entry.
        /// </summary>
        public Machine Machine { get; set; }
    }

}
