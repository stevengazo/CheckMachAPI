using CheckMachAPI.Data;
using CheckMachAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CheckMachAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, EmailSender emailSender, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _db = applicationDbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var count = _db.Users.Count();

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            if (count == 0)
            {
                user.EmailConfirmed = true;
            }
            var result = await _userManager.CreateAsync(user, model.Password);

            if (count == 0)
            {
                await AssignRolesAsync(user);
            }

            if (!result.Succeeded)

                return BadRequest(result.Errors);

            return Ok(new { Message = "Usuario registrado correctamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Usuario o contraseña incorrectos");

            // Generar token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.Now.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { Token = jwt });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound("Usuario no encontrado");



            // Buscar el código en la DB
            var resetCode = await _db.PasswordResetCodes
                .Where(x => x.UserId == user.Id &&
            x.Code == model.Code &&
            !x.IsUsed &&
            x.Expiration > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (resetCode == null)
                return BadRequest("Código inválido o expirado");

            // Generar token de Identity para resetear contraseña
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Marcar el código como usado
            resetCode.IsUsed = true;
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Contraseña cambiada correctamente" });
        }

        [HttpOptions("send-token")]
        public async Task<IActionResult> SendResetToken([FromBody] string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound("No encontrado");

                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);


                PasswordResetCode ob = new()
                {
                    Expiration = DateTime.UtcNow.AddHours(2),
                    Code = GenerateNumericCode(),
                    UserId = user.Id,
                    IsUsed = false
                };
                SaveToken(ob);

                var emailBody = $@"
                    <!DOCTYPE html>
                    <html lang='es'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Restablecer Contraseña</title>
                        <style>
                            body {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #f4f4f7;
                                margin: 0;
                                padding: 0;
                                -webkit-text-size-adjust: 100%;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 40px auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 10px;
                                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            }}
                            .logo {{
                                text-align: center;
                                margin-bottom: 20px;
                            }}
                            h1 {{
                                color: #2a5298;
                                font-size: 1.8em;
                                margin-bottom: 20px;
                                text-align: center;
                            }}
                            p {{
                                font-size: 1em;
                                line-height: 1.6;
                                margin-bottom: 20px;
                                color: #333333;
                            }}
                            .btn {{
                                display: inline-block;
                                background-color: #00b4d8;
                                color: #ffffff !important;
                                padding: 12px 25px;
                                border-radius: 6px;
                                text-decoration: none;
                                font-weight: bold;
                                transition: background 0.3s ease;
                            }}
                            .btn:hover {{
                                background-color: #0096c7;
                            }}
                            footer {{
                                font-size: 0.85em;
                                color: #888888;
                                margin-top: 30px;
                                text-align: center;
                            }}
                            @media only screen and (max-width: 600px) {{
                                .container {{
                                    padding: 20px;
                                    margin: 20px;
                                }}
                                h1 {{
                                    font-size: 1.5em;
                                }}
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='logo'>
                                <img src='https://tu-dominio.com/logo.png' alt='CheckMach Logo' width='120'>
                            </div>
                            <h1>Restablecer Contraseña</h1>
                            <p>Hemos recibido una solicitud para restablecer tu contraseña en <strong>CheckMach API</strong>.</p>
                            <p style='text-align:center;'>
                                {ob.Code}
                            </p>
                            <p>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña seguirá siendo segura.</p>
                            <footer>© 2025 CheckMach API. Todos los derechos reservados.</footer>
                        </div>
                    </body>
                    </html>";

                await _emailSender.SendEmailAsync(email, "Cambio contrase;a", emailBody);
                return Ok();
            }
            catch (Exception d)
            {
                return BadRequest();
            }
        }

        private async Task AssignRolesAsync(ApplicationUser user)
        {
            try
            {
                string[] rolName = ["admin", "manager", "user"];

                if ((await _userManager.Users.CountAsync()) == 1)
                {
                    foreach (var item in rolName)
                    {
                        await _userManager.AddToRoleAsync(user, item);
                    }
                }
            }
            catch (Exception d)
            {
                throw d;
            }
        }

        public static string GenerateNumericCode(int length = 8)
        {
            if (length <= 0) throw new ArgumentException("Length must be positive", nameof(length));

            var maxValue = (int)Math.Pow(10, length) - 1;
            var minValue = (int)Math.Pow(10, length - 1);

            // Usamos RandomNumberGenerator para seguridad criptográfica
            int code;
            do
            {
                var bytes = new byte[4];
                RandomNumberGenerator.Fill(bytes);
                code = BitConverter.ToInt32(bytes, 0) % (maxValue + 1);
                if (code < 0) code = -code;
            } while (code < minValue); // Garantiza que tenga la longitud deseada

            return code.ToString();
        }

        private async Task SaveToken(PasswordResetCode obj)
        {
            try
            {
                _db.PasswordResetCodes.Add(obj);
                _db.SaveChanges();

            }
            catch (Exception f)
            {
                throw f;
            }
        }

    }


    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string Code { get; set; } // Código de 8 dígitos enviado por email
        public string NewPassword { get; set; }
    }


    // Modelos para recibir datos
    public record RegisterModel(string Username, string Email, string Password);
    public record LoginModel(string Username, string Password);
}
