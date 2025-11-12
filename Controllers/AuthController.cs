using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CheckMachAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

            if(count == 0)
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
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { Token = jwt });
        }
        [HttpOptions("send-token")]
        public async Task<IActionResult> SendResetToken([FromBody] string email)
        {
            try
            {
                var user  = await  _userManager.FindByNameAsync(email); 
                if(user == null)
                {
                    return NotFound("No encontrado");

                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailSender.SendEmailAsync(email, "Cambio contrase;a", token);
                return Ok();
            }
            catch ( Exception d)
            {
                return BadRequest();
            }
        }

        private async Task AssignRolesAsync(ApplicationUser user)
        {
            try
            {
                string[] rolName = ["admin", "manager", "user"];

                if( (await _userManager.Users.CountAsync()) == 1)
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
    
    }

   
    // Modelos para recibir datos
    public record RegisterModel(string Username, string Email, string Password);
    public record LoginModel(string Username, string Password);
}
