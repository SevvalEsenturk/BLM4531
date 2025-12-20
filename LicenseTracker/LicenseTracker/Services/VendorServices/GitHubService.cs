using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class GitHubService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;

    public string VendorName => "GitHub";

    public GitHubService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<GitHubService> logger)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<VendorSyncResult> SyncLicensesAsync()
    {
        var result = new VendorSyncResult { VendorName = VendorName };

        try
        {
            var token = _configuration["VendorAPIs:GitHub:PersonalAccessToken"];
            if (string.IsNullOrEmpty(token))
            {
                result.ErrorMessage = "GitHub token not configured";
                return result;
            }

            var orgInfo = await FetchOrganizationInfoAsync(token);
            if (orgInfo == null)
            {
                result.ErrorMessage = "Failed to fetch GitHub organization info";
                return result;
            }

            result.LicensesFound = 1;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "GitHub");

            if (company == null)
            {
                company = new Company
                {
                    Name = "GitHub",
                    ApiKeyVaultReference = "github-pat"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Name == "GitHub Enterprise" && l.Vendor == VendorName);

            if (existingLicense == null)
            {
                var newLicense = new License
                {
                    Name = "GitHub Enterprise",
                    Vendor = VendorName,
                    Category = "Development Tools",
                    CompanyId = company.Id,
                    HasLicense = true,
                    Users = orgInfo.TotalSeats,
                    StartDate = DateTime.UtcNow,
                    EndDate = null
                };
                _context.Licenses.Add(newLicense);
                result.LicensesAdded++;
            }
            else
            {
                existingLicense.Users = orgInfo.TotalSeats;
                result.LicensesUpdated++;
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing GitHub licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var token = _configuration["VendorAPIs:GitHub:PersonalAccessToken"];
            if (string.IsNullOrEmpty(token)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LicenseTracker");
            var response = await _httpClient.GetAsync("https://api.github.com/user");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub API connection test failed");
            return false;
        }
    }

    private async Task<GitHubOrgInfo?> FetchOrganizationInfoAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LicenseTracker");

            var org = _configuration["VendorAPIs:GitHub:Organization"];
            var response = await _httpClient.GetAsync($"https://api.github.com/orgs/{org}/members?per_page=100");

            if (!response.IsSuccessStatusCode) return null;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var members = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var totalSeats = members.GetArrayLength();

            return new GitHubOrgInfo { TotalSeats = totalSeats };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub organization info");
            return null;
        }
    }

    private class GitHubOrgInfo
    {
        public int TotalSeats { get; set; }
    }
}
