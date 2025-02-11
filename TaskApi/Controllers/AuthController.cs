using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MimeKit;
using MailKit.Net.Smtp;
using TaskApi.Data;
using TaskApi.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace TaskApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthController(IConfiguration configuration, AppDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        // Register new user and send verification email
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email already registered" });

            var user = new UserModel
            {
                Username = request.Username,
                Email = request.Email,
                // PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hash password
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                EmailConfirmed = false, // Ensure it starts as false
                VerificationToken = Guid.NewGuid().ToString(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24) // Token valid for 24h
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Send verification email
            SendVerificationEmail(user.Email, user.VerificationToken);

            return Ok(new { message = "User registered. Please check your email to verify your account." });
        }

        // Verify email (User clicks link in email)
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null || (user.VerificationTokenExpiry.HasValue && user.VerificationTokenExpiry < DateTime.UtcNow))
            return BadRequest(new { message = "Invalid or expired verification token" });

            user.EmailConfirmed = true;
            user.VerificationToken = null;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Email verified successfully. You can now log in." });
        }

        // return jwt token as cookie instead of response body
        // jwt sent automatically with requests, prevent XSS attack since JS can't access cookies
        // [HttpPost("login")]
        // public async Task<IActionResult> Login([FromBody] LoginRequest request)
        // {
        //     var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        //     if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        //         return Unauthorized(new { message = "Invalid credentials" });

        //     if (!user.EmailConfirmed)
        //         return Unauthorized(new { message = "Email not verified. Please check your inbox." });

        //     var token = GenerateJwtToken(user.Id, user.Username);
        //     Response.Cookies.Append("AuthToken", token, new CookieOptions
        //     {
        //         HttpOnly = true,
        //         Secure = false, // Change to true in production (requires HTTPS)
        //         SameSite = SameSiteMode.Strict,
        //         Expires = DateTime.UtcNow.AddHours(1)
        //     });

        //     return Ok(new { message = "Login successful" });
        // }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            if (!user.EmailConfirmed)
                return Unauthorized(new { message = "Email not verified. Please check your inbox." });

            var token = GenerateJwtToken(user.Id, user.Username);
            
            Console.WriteLine($"🔑 Generated Token: {token}");
            Console.WriteLine($"✅ Storing JWT in Cookie: {token}");
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to `true` if using HTTPS
                SameSite = SameSiteMode.Lax, // Ensures it's sent with frontend requests
                Expires = DateTime.UtcNow.AddHours(1)
            });

            Console.WriteLine("✅ JWT Token Set in Cookie");

            return Ok(new { message = "Login successful" });
        }



        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken"); // Remove JWT token cookie
            return Ok(new { message = "Logged out" });
        }

        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            var token = Request.Cookies["AuthToken"]; // Read token from cookie
            Console.WriteLine($"📌 Debug: Received Token from Cookie: {token}");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ No token found in cookies.");
                return Unauthorized(new { isAuthenticated = false });
            }

            Console.WriteLine($"📌 Received token: {token}");

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (!string.IsNullOrEmpty(username))
                {
                    Console.WriteLine($"✅ Authenticated user: {username}");
                    Console.WriteLine($"✅ User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                    return Ok(new { isAuthenticated = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JWT validation error: {ex.Message}");
                return Unauthorized(new { isAuthenticated = false });
            }

            return Unauthorized(new { isAuthenticated = false });
        }


        private string GenerateJwtToken(int userId, string username)
        {
            var key = _configuration["JwtSettings:Secret"];
            // Console.WriteLine($"Using JWT Secret: {key}");
            if (string.IsNullOrEmpty(key) || key.Length < 32)
                throw new ArgumentException("JWT Secret must be at least 32 characters long!");

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"])),
                SecurityAlgorithms.HmacSha256 // <-- Ensure this is HS256!
            );

            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username),
                // new Claim(ClaimTypes.Name, username),
                new Claim("name", username),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var claim in claims)
            {
                Console.WriteLine($"📌 Adding Claim: Type={claim.Type}, Value={claim.Value}");
            }

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

    private void SendVerificationEmail(string email, string token)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPass"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = "Verify Your Email";
        message.Body = new TextPart("plain")
        {
            Text = $"Click the link to verify your email: http://localhost:5000/api/auth/verify?token={token}"
        };

        using var client = new SmtpClient();
        client.Connect(smtpHost, smtpPort, false);
        client.Authenticate(smtpUser, smtpPass);
        client.Send(message);
        client.Disconnect(true);
    }
    }
    // Request Models
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}