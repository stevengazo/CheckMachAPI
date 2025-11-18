using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    /// <summary>
    /// Represents a project within the system, including its name, description, 
    /// start and end dates, author information, deletion status, and associated time records.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the unique identifier for the project.
        /// </summary>
        [Key]
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the project.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the project starts.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the project ends.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the project has been marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is the author of the project.
        /// </summary>
        public bool Author { get; set; }

        /// <summary>
        /// Gets or sets the collection of time records associated with this project.
        /// </summary>
        public ICollection<TimePeerProject> Times { get; set; }
    }
}
