using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class ZoomService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZoomService> _logger;

    public string VendorName => "Zoom";

    public ZoomService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<ZoomService> logger)
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
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                result.ErrorMessage = "Zoom API authentication failed";
                return result;
            }

            var accountInfo = await FetchAccountInfoAsync(accessToken);
            if (accountInfo == null)
            {
                result.ErrorMessage = "Failed to fetch Zoom account info";
                return result;
            }

            result.LicensesFound = 1;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Zoom");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Zoom",
                    ApiKeyVaultReference = "zoom-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Name == "Zoom" && l.Vendor == VendorName);

            if (existingLicense == null)
            {
                var newLicense = new License
                {
                    Name = "Zoom",
                    Vendor = VendorName,
                    Category = "Video Conferencing",
                    CompanyId = company.Id,
                    HasLicense = true,
                    Users = accountInfo.LicensedUsers,
                    StartDate = DateTime.UtcNow,
                    EndDate = null
                };
                _context.Licenses.Add(newLicense);
                result.LicensesAdded++;
            }
            else
            {
                existingLicense.Users = accountInfo.LicensedUsers;
                result.LicensesUpdated++;
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Zoom licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.zoom.us/v2/users/me");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zoom API connection test failed");
            return false;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var accountId = _configuration["VendorAPIs:Zoom:AccountId"];
            var clientId = _configuration["VendorAPIs:Zoom:ClientId"];
            var clientSecret = _configuration["VendorAPIs:Zoom:ClientSecret"];

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("Zoom API credentials not configured");
                return string.Empty;
            }

            var tokenEndpoint = $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={accountId}";
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            var response = await _httpClient.PostAsync(tokenEndpoint, null);

            if (!response.IsSuccessStatusCode) return string.Empty;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            return tokenResponse.GetProperty("access_token").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Zoom access token");
            return string.Empty;
        }
    }

    private async Task<ZoomAccountInfo?> FetchAccountInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.zoom.us/v2/users?status=active&page_size=300");

            if (!response.IsSuccessStatusCode) return null;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var totalRecords = data.GetProperty("total_records").GetInt32();

            return new ZoomAccountInfo { LicensedUsers = totalRecords };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Zoom account info");
            return null;
        }
    }

    private class ZoomAccountInfo
    {
        public int LicensedUsers { get; set; }
    }
}
