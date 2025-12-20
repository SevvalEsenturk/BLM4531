using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class DropboxService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DropboxService> _logger;

    public string VendorName => "Dropbox";

    public DropboxService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<DropboxService> logger)
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
            var accessToken = _configuration["VendorAPIs:Dropbox:AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                result.ErrorMessage = "Dropbox access token not configured";
                return result;
            }

            var teamInfo = await FetchTeamInfoAsync(accessToken);
            if (teamInfo == null)
            {
                result.ErrorMessage = "Failed to fetch Dropbox team info";
                return result;
            }

            result.LicensesFound = 1;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Dropbox");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Dropbox",
                    ApiKeyVaultReference = "dropbox-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Name == "Dropbox Business" && l.Vendor == VendorName);

            if (existingLicense == null)
            {
                var newLicense = new License
                {
                    Name = "Dropbox Business",
                    Vendor = VendorName,
                    Category = "Cloud Storage",
                    CompanyId = company.Id,
                    HasLicense = true,
                    Users = teamInfo.NumLicensedUsers,
                    StartDate = DateTime.UtcNow,
                    EndDate = null
                };
                _context.Licenses.Add(newLicense);
                result.LicensesAdded++;
            }
            else
            {
                existingLicense.Users = teamInfo.NumLicensedUsers;
                result.LicensesUpdated++;
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Dropbox licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var accessToken = _configuration["VendorAPIs:Dropbox:AccessToken"];
            if (string.IsNullOrEmpty(accessToken)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync("https://api.dropboxapi.com/2/team/get_info", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dropbox API connection test failed");
            return false;
        }
    }

    private async Task<DropboxTeamInfo?> FetchTeamInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync("https://api.dropboxapi.com/2/team/get_info", null);

            if (!response.IsSuccessStatusCode) return null;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var numLicensedUsers = data.GetProperty("num_licensed_users").GetInt32();

            return new DropboxTeamInfo { NumLicensedUsers = numLicensedUsers };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Dropbox team info");
            return null;
        }
    }

    private class DropboxTeamInfo
    {
        public int NumLicensedUsers { get; set; }
    }
}
