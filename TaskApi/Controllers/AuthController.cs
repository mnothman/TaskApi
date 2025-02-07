using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TaskApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // return jwt token as cookie instead of response body
        // jwt sent automatically with requests, prevent XSS attack since JS can't access cookies
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "password")
            {
                var token = GenerateJwtToken(request.Username);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,  // Prevents client-side JS from accessing it
                    Secure = false,    // Ensures it's only sent over HTTPS (set to false for local testing)
                    SameSite = SameSiteMode.Strict, // Prevents CSRF attacks
                    Expires = DateTime.UtcNow.AddHours(1)
                };

                Response.Cookies.Append("AuthToken", token, cookieOptions); // Add token to cookie

                return Ok(new { token });
                // return Ok(new { message = "Login successful" });
            }

            return Unauthorized();
        }
        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken"); // Remove JWT token cookie
            return Ok(new { message = "Logged out" });
        }

        private string GenerateJwtToken(string username)
        {
            var key = _configuration["JwtSettings:Secret"];
            Console.WriteLine($"Using JWT Secret: {key}");
            if (string.IsNullOrEmpty(key) || key.Length < 32)
                throw new ArgumentException("JWT Secret must be at least 32 characters long!");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            
            // remove these two lines after testing
            // var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            // Console.WriteLine($"Generated Token: {tokenString}"); // Log the token string

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
