using CheckMachAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CheckMachAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; } = "";
        public string? LastName { get; set; } = "";
        public ICollection<Maintenance> Maintenances { get; set; }
        public ICollection<Inspection> Inspections { get; set; }

        public ICollection<InventoryMove> InventoryMoves { get; set; }  
    }
}
