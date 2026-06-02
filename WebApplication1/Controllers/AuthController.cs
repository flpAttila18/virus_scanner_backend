using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.models;



namespace WebApplication1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email || u.UserName == dto.UserName))
            {
                return BadRequest(new { error = "A felhasználónév vagy az email cím már foglalt" });
            }

            string hashedPassowrd = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                Password = hashedPassowrd,
                Role = "user"
                
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sikeres regisztálció" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { error = "Hibás felhasználónév vagy jelszó" });
            }
            ;

            var token = GenerateJwtToken(user);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            };
            Response.Cookies.Append("X-Auth-token", token, cookieOptions);

            
            return Ok(new {message =$"sikeres bejelentkezés!"});
        }
        [NonAction]
        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["key"] ?? throw new InvalidOperationException("JWT Key is missing"));
            var symmetricKey = new SymmetricSecurityKey(keyBytes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name ,user.UserName),
                new Claim(ClaimTypes.Email , user.Email),
                new Claim(ClaimTypes.Role , user.Role ?? "user")
            };

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(14),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandel = new JwtSecurityTokenHandler();
            var token = tokenHandel.CreateToken(tokenDescription);

            return tokenHandel.WriteToken(token);

        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult whoami()
        {
            try
            {
                if (User != null)
                {
                    return Ok(new
                    {
                      Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                      UserName = User.Identity?.Name,
                      Email = User.FindFirstValue(ClaimTypes.Email),
                      Role = User.FindFirstValue(ClaimTypes.Role)
                    });
                }
                else
                {
                    return BadRequest(new { error ="nem található cookie"});
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Váratlan szerverhiba történt." });
            }
        }


        [HttpPost("logout")]
        public IActionResult logout()
        {
            try
            {

                var cookieOptons = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Delete("X-Auth-token");
                return Ok(new { message = "sikeres kijelentkezés" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Váratlan szerverhiba történt." });
            }
        }

    }
}

