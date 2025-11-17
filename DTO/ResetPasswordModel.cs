namespace CheckMachAPI.DTO
{
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string Code { get; set; } // Código de 8 dígitos enviado por email
        public string NewPassword { get; set; }
    }

}
