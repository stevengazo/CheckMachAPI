

using Microsoft.AspNetCore.Identity;

namespace CheckMachAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = "";
        public string LastName { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}