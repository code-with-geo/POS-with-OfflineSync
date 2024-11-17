using POS.Class;
using POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;  // For password hashing

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult Register([FromBody] User user)
        {
            // Validate input (basic example)
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Check if user already exists
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("User already exists.");
            }

            // Hash the password before saving
            user.Password = HashPassword(user.Password);

            // Save the new user
            _context.Users.Add(user);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Login), new { username = user.Username }, "User registered successfully.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            // Validate the input
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Find the user by username
            var dbUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (dbUser == null || !VerifyPassword(user.Password, dbUser.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            // Generate JWT Token
            var token = GenerateJwtToken(dbUser);
            return Ok(new { Token = token });
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            // You can use a more advanced hashing library like BCrypt or PBKDF2
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes("your-salt-here"), // Should be a unique salt for each user
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }

        // Helper method to verify the password
        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            var hashedInputPassword = HashPassword(inputPassword); // In real apps, use a proper hashing algorithm
            return hashedInputPassword == storedPasswordHash;
        }

        // Helper method to generate the JWT token
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
