using CheckMachAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace CheckMachAPI.Models
{
    public class PasswordResetCode
    {
        [Key]
        public int Id { get; set; }

        // FK al usuario
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Código numérico temporal
        [Required]
        public string Code { get; set; }

        // Fecha de expiración
        [Required]
        public DateTime Expiration { get; set; }

        // Indica si ya fue usado
        public bool IsUsed { get; set; } = false;
    }
}
