using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class GitLabService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitLabService> _logger;

    public string VendorName => "GitLab";

    public GitLabService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<GitLabService> logger)
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
            var accessToken = _configuration["VendorAPIs:GitLab:PersonalAccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                result.ErrorMessage = "GitLab token not configured";
                return result;
            }

            var licenseInfo = await FetchLicenseInfoAsync(accessToken);
            if (licenseInfo == null)
            {
                result.ErrorMessage = "Failed to fetch GitLab license info";
                return result;
            }

            result.LicensesFound = 1;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "GitLab");

            if (company == null)
            {
                company = new Company
                {
                    Name = "GitLab",
                    ApiKeyVaultReference = "gitlab-pat"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Name == "GitLab" && l.Vendor == VendorName);

            if (existingLicense == null)
            {
                var newLicense = new License
                {
                    Name = "GitLab",
                    Vendor = VendorName,
                    Category = "Development Tools",
                    CompanyId = company.Id,
                    HasLicense = true,
                    Users = licenseInfo.ActiveUsers,
                    StartDate = DateTime.UtcNow,
                    EndDate = licenseInfo.ExpiresAt
                };
                _context.Licenses.Add(newLicense);
                result.LicensesAdded++;
            }
            else
            {
                existingLicense.Users = licenseInfo.ActiveUsers;
                existingLicense.EndDate = licenseInfo.ExpiresAt;
                result.LicensesUpdated++;
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing GitLab licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var accessToken = _configuration["VendorAPIs:GitLab:PersonalAccessToken"];
            if (string.IsNullOrEmpty(accessToken)) return false;

            var baseUrl = _configuration["VendorAPIs:GitLab:BaseUrl"] ?? "https://gitlab.com";
            _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", accessToken);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/v4/user");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitLab API connection test failed");
            return false;
        }
    }

    private async Task<GitLabLicenseInfo?> FetchLicenseInfoAsync(string accessToken)
    {
        try
        {
            var baseUrl = _configuration["VendorAPIs:GitLab:BaseUrl"] ?? "https://gitlab.com";
            _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", accessToken);

            // Get license info (self-hosted GitLab only)
            var response = await _httpClient.GetAsync($"{baseUrl}/api/v4/license");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                var activeUsers = data.GetProperty("active_users").GetInt32();
                var expiresAt = data.GetProperty("expires_at").GetString();

                return new GitLabLicenseInfo
                {
                    ActiveUsers = activeUsers,
                    ExpiresAt = DateTime.TryParse(expiresAt, out var expiry) ? expiry : null
                };
            }

            // Fallback: count users
            var usersResponse = await _httpClient.GetAsync($"{baseUrl}/api/v4/users?active=true&per_page=100");
            if (usersResponse.IsSuccessStatusCode)
            {
                var usersJson = await usersResponse.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement>(usersJson);
                var activeUsers = users.GetArrayLength();

                return new GitLabLicenseInfo { ActiveUsers = activeUsers };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitLab license info");
            return null;
        }
    }

    private class GitLabLicenseInfo
    {
        public int ActiveUsers { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
