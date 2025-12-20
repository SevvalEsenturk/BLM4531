using LicenseTracker.Data;
using LicenseTracker.DTOs;
using LicenseTracker.Models;
using LicenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LicenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already in use" });
            }

            // 2. Handle Company
            Company? company = null;
            if (!string.IsNullOrEmpty(dto.CompanyName))
            {
                company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == dto.CompanyName);
                if (company == null)
                {
                    company = new Company { Name = dto.CompanyName };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                }
            }

            // 3. Create user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true,
                CompanyId = company?.Id
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Registration failed", errors = result.Errors });
            }

            // 4. Generate token
            var token = _jwtService.GenerateToken(user);
            
            return Ok(new AuthResponseDto
            {
                Token = token,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CompanyName = company?.Name
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var audit = new LoginAudit
            {
                UserName = dto.Email,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            try 
            {
                // 1. Find user
                var user = await _userManager.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    audit.IsSuccess = false;
                    audit.FailureReason = "User not found";
                    _context.LoginAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    return Unauthorized(new { message = "Invalid email or password" });
                }
                
                audit.UserId = user.Id;

                // 2. Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                
                if (!result.Succeeded)
                {
                    audit.IsSuccess = false;
                    audit.FailureReason = "Invalid password";
                    _context.LoginAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // 3. Success
                audit.IsSuccess = true;
                user.LastLoginDate = DateTime.UtcNow;
                
                _context.LoginAudits.Add(audit);
                await _userManager.UpdateAsync(user); // Save LastLoginDate
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CompanyName = user.Company?.Name
                });
            }
            catch(Exception ex)
            {
                // Fallback logging
                audit.IsSuccess = false;
                audit.FailureReason = $"Exception: {ex.Message}";
                _context.LoginAudits.Add(audit);
                await _context.SaveChangesAsync();
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return Ok(new 
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Id,
                CompanyName = user.Company?.Name
            });
        }
    }
}
