using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.DTOs;
using System.Security.Claims;

namespace LicenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "Bearer")]
    public class LicensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LicensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Licenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LicenseDto>>> GetLicenses()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var licenses = await _context.Licenses.Where(l => l.UserId == userId).ToListAsync();
            var licenseDtos = licenses.Select(l => new LicenseDto
            {
                Id = l.Id,
                Name = l.Name,
                Vendor = l.Vendor,
                Category = l.Category,
                CompanyId = l.CompanyId,
                HasLicense = l.HasLicense,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Cost = l.Cost,
                Users = l.Users,
                RemainingDays = l.RemainingDays
            }).ToList();

            return Ok(licenseDtos);
        }

        // GET: api/Licenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LicenseDto>> GetLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);

            if (license == null)
            {
                return NotFound();
            }

            var licenseDto = new LicenseDto
            {
                Id = license.Id,
                Name = license.Name,
                Vendor = license.Vendor,
                Category = license.Category,
                CompanyId = license.CompanyId,
                HasLicense = license.HasLicense,
                StartDate = license.StartDate,
                EndDate = license.EndDate,
                Cost = license.Cost,
                Users = license.Users,
                RemainingDays = license.RemainingDays
            };

            return Ok(licenseDto);
        }

        // POST: api/Licenses
        [HttpPost]
        public async Task<ActionResult<LicenseDto>> CreateLicense(CreateLicenseDto createDto)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var license = new License
            {
                Name = createDto.Name,
                Vendor = createDto.Vendor,
                Category = createDto.Category,
                CompanyId = createDto.CompanyId,
                HasLicense = createDto.HasLicense,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Cost = createDto.Cost,
                Users = createDto.Users,
                UserId = userId
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            var licenseDto = new LicenseDto
            {
                Id = license.Id,
                Name = license.Name,
                Vendor = license.Vendor,
                Category = license.Category,
                CompanyId = license.CompanyId,
                HasLicense = license.HasLicense,
                StartDate = license.StartDate,
                EndDate = license.EndDate,
                Cost = license.Cost,
                Users = license.Users,
                RemainingDays = license.RemainingDays
            };

            return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, licenseDto);
        }

        // PUT: api/Licenses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLicense(int id, UpdateLicenseDto updateDto)
        {
            var license = await _context.Licenses.FindAsync(id);

            if (license == null)
            {
                return NotFound();
            }

            license.Name = updateDto.Name;
            license.Vendor = updateDto.Vendor;
            license.Category = updateDto.Category;
            license.CompanyId = updateDto.CompanyId;
            license.HasLicense = updateDto.HasLicense;
            license.StartDate = updateDto.StartDate;
            license.EndDate = updateDto.EndDate;
            license.Cost = updateDto.Cost;
            license.Users = updateDto.Users;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LicenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Licenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null)
            {
                return NotFound();
            }

            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LicenseExists(int id)
        {
            return _context.Licenses.Any(e => e.Id == id);
        }
    }
}
