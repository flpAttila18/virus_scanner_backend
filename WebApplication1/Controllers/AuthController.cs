using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.models;
using Microsoft.AspNetCore.Http;


namespace WebApplication1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AuthController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
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
                Role = "user",
                Profile_Pic = "default.jpg"


            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sikeres regisztálció" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { error = "Hibás felhasználónév vagy jelszó" });
            }



            var token = GenerateJwtToken(user);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            };
            Response.Cookies.Append("X-Auth-token", token, cookieOptions);


            return Ok(new { message = $"sikeres bejelentkezés!" });
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
                new Claim(ClaimTypes.Role , user.Role ?? "user"),
                new Claim("ProfilePicture" , user.Profile_Pic ?? "default.jpg")
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
        public async Task<IActionResult> whoami() // async lett és Task<IActionResult>!
        {
            try
            {
                // 1. Biztonságosan kiszedjük a bejelentkezett felhasználó ID-ját a tokenből
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return BadRequest(new { error = "Nem található felhasználói azonosító." });
                }

                // Átalakítjuk uint-re (vagy int-re, attól függően, mi a típusa a User modelledben)
                uint currentUserId = uint.Parse(userIdClaim);

                // 2. Megkeressük a felhasználót az adatbázisban (itt van a valós, friss Profile_Pic!)
                var dbUser = await _context.Users.FindAsync(currentUserId);
                if (dbUser == null)
                {
                    return NotFound(new { error = "A felhasználó nem található az adatbázisban." });
                }

                // 3. Visszaadjuk a friss, adatbázisból kiolvasott adatokat a frontendnek
                return Ok(new
                {
                    Id = dbUser.Id,
                    UserName = dbUser.UserName,
                    Email = dbUser.Email,
                    Role = dbUser.Role,
                    ProfilePicture = dbUser.Profile_Pic ?? "default.png" // Így frissítéskor mindig a jó kép jön vissza!
                });
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

                Response.Cookies.Delete("X-Auth-token", cookieOptons);
                return Ok(new { message = "sikeres kijelentkezés" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Váratlan szerverhiba történt." });
            }
        }
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetUserHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { error = "Nem található felhasználói azonosító a tokenben." });
                }
                int curreuntUserId = int.Parse(userIdClaim);

                var history = await _context.viruses
                    .Where(v => v.userId == curreuntUserId)
                    .Select(v => new
                    {
                        v.Id,
                        v.FileName,
                        v.VirusName,
                        v.virusType,
                        v.userId,
                    })
                    .ToListAsync();



                return Ok(history);


            }
            catch (Exception)
            {

                return StatusCode(500, new { error = $"Hiba a történet lekérése során   " });
            }
        }


        [Authorize]
        [HttpPut("updateUserName")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
                {
                    return BadRequest(new { error = "A felhasználónév nem lehet üres." });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { error = "Nem található felhasználói azonosító ." });
                }
                uint curreuntUserId = uint.Parse(userIdClaim);

                bool nameExists = await _context.Users.AnyAsync(u => u.UserName == dto.Username && u.Id != curreuntUserId);
                if (nameExists)
                {
                    return BadRequest(new { error = "A megadott felhasználónév már foglalt." });
                }

                var user = await _context.Users.FindAsync(curreuntUserId);
                if (user == null)
                {
                    return NotFound(new { error = "Felhasználó nem található." });
                }

                user.UserName = dto.Username;
                await _context.SaveChangesAsync();

                var newToken = GenerateJwtToken(user);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(14)
                };
                Response.Cookies.Append("X-Auth-token", newToken, cookieOptions);
                return Ok(new { message = "Felhasználónév sikeresen frissítve." });

            }
            catch (Exception ex)
            {


                return StatusCode(500, new { error = $"Szerverhiba: {ex.Message}", details = ex.InnerException?.Message });
            }
        }

        [Authorize]
        [HttpPost("uploadPfp")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file)
        {
            try
            {
                // 1. Validáció: Van-e egyáltalán fájl?
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "Nem választottál ki fájlt." });
                }

                // 2. Biztonság: Fájlméret korlátozása (pl. max 5 MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "A fájl mérete nem haladhatja meg az 5 MB-ot." });
                }

                // 3. Biztonság: Kiterjesztés ellenőrzése
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { error = "Csak .jpg, .jpeg, .png, .webp vagy .gif formátumú képek tölthetők fel." });
                }

                // 4. Felhasználó beazonosítása
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { error = "Nem található felhasználói azonosító." });
                }
                uint currentUserId = uint.Parse(userIdClaim);

                var user = await _context.Users.FindAsync(currentUserId);
                if (user == null)
                {
                    return NotFound(new { error = "A felhasználó nem található." });
                }

                // 5. Mappa kezelése: Biztosra megyünk és manuálisan lőjük be a wwwroot/uploads mappát
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(rootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 6. Biztonság: Egyedi név generálása Guid segítségével... (Ez a részed maradhat változatlan)
                string uniqueFileName = $"pfp_{currentUserId}_{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 7. Opcionális: Régi kép törlése, ha már volt neki (nem szemeteljük a szervert)
                // JAVÍTÁS: Ellenőrizzük, hogy a régi kép nem a default-e, mert azt NEM akarjuk letörölni a lemezről!
                if (!string.IsNullOrEmpty(user.Profile_Pic) && user.Profile_Pic != "default.jpg" && user.Profile_Pic != "default.png")
                {
                    string oldFilePath = Path.Combine(uploadsFolder, user.Profile_Pic);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 8. Fájl elmentése a lemezre
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 9. Új fájlnév mentése az adatbázisba a User entitásba
                // (Feltételezve, hogy a User.cs-ben létrehoztad a public string? ProfilePicture { get; set; } tulajdonságot)
                user.Profile_Pic = uniqueFileName;
                await _context.SaveChangesAsync();

                // 10. Visszatérési érték a frontendnek az új kép elérési útjával
                string relativeUrl = $"/uploads/{uniqueFileName}";
                return Ok(new { message = "Profilkép sikeresen frissítve.", profilePicture = relativeUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Szerverhiba a kép mentése során: {ex.Message}" });
            }

        }

    }
}

