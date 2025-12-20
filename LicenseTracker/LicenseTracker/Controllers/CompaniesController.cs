using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.DTOs;

namespace LicenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
        {
            var companies = await _context.Companies
                .Include(c => c.Licenses)
                .ToListAsync();

            var companyDtos = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                ApiKeyVaultReference = c.ApiKeyVaultReference,
                Licenses = c.Licenses.Select(l => new LicenseDto
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
                }).ToList()
            }).ToList();

            return Ok(companyDtos);
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Licenses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                ApiKeyVaultReference = company.ApiKeyVaultReference,
                Licenses = company.Licenses.Select(l => new LicenseDto
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
                }).ToList()
            };

            return Ok(companyDto);
        }

        // POST: api/Companies
        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto createDto)
        {
            var company = new Company
            {
                Name = createDto.Name,
                ApiKeyVaultReference = createDto.ApiKeyVaultReference
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                ApiKeyVaultReference = company.ApiKeyVaultReference,
                Licenses = new List<LicenseDto>()
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, companyDto);
        }

        // PUT: api/Companies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyDto updateDto)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            company.Name = updateDto.Name;
            company.ApiKeyVaultReference = updateDto.ApiKeyVaultReference;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
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

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
