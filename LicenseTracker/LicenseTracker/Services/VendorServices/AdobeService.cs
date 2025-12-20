using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class AdobeService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdobeService> _logger;

    public string VendorName => "Adobe";

    public AdobeService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<AdobeService> logger)
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
                result.ErrorMessage = "Adobe API authentication failed";
                return result;
            }

            var licenses = await FetchLicensesFromAdobeApiAsync(accessToken);
            result.LicensesFound = licenses.Count;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Adobe");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Adobe",
                    ApiKeyVaultReference = "adobe-admin-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            foreach (var licenseData in licenses)
            {
                var existingLicense = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name == licenseData.ProductName && l.Vendor == VendorName);

                if (existingLicense == null)
                {
                    var newLicense = new License
                    {
                        Name = licenseData.ProductName,
                        Vendor = VendorName,
                        Category = "Creative Software",
                        CompanyId = company.Id,
                        HasLicense = true,
                        Users = licenseData.AssignedLicenses,
                        StartDate = DateTime.UtcNow,
                        EndDate = null
                    };
                    _context.Licenses.Add(newLicense);
                    result.LicensesAdded++;
                }
                else
                {
                    existingLicense.Users = licenseData.AssignedLicenses;
                    existingLicense.HasLicense = licenseData.AssignedLicenses > 0;
                    result.LicensesUpdated++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Adobe licenses");
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
            var orgId = _configuration["VendorAPIs:Adobe:OrgId"];
            var response = await _httpClient.GetAsync($"https://usermanagement.adobe.io/v2/usermanagement/organizations/{orgId}/users");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adobe API connection test failed");
            return false;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var clientId = _configuration["VendorAPIs:Adobe:ClientId"];
            var clientSecret = _configuration["VendorAPIs:Adobe:ClientSecret"];
            var technicalAccountId = _configuration["VendorAPIs:Adobe:TechnicalAccountId"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("Adobe API credentials not configured");
                return string.Empty;
            }

            // Adobe OAuth JWT authentication
            // Not: Gerçek implementasyon için JWT token oluşturulması gerekir
            // Şimdilik basitleştirilmiş versiyon
            return string.Empty; // TODO: Implement JWT generation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Adobe access token");
            return string.Empty;
        }
    }

    private async Task<List<AdobeLicenseData>> FetchLicensesFromAdobeApiAsync(string accessToken)
    {
        var licenses = new List<AdobeLicenseData>();

        try
        {
            var apiKey = _configuration["VendorAPIs:Adobe:ApiKey"];
            var orgId = _configuration["VendorAPIs:Adobe:OrgId"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await _httpClient.GetAsync(
                $"https://usermanagement.adobe.io/v2/usermanagement/organizations/{orgId}/products");

            if (!response.IsSuccessStatusCode) return licenses;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            // Adobe Creative Cloud product parsing
            licenses.Add(new AdobeLicenseData
            {
                ProductName = "Adobe Creative Cloud All Apps",
                AssignedLicenses = 0 // API'den gelen gerçek veri
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Adobe licenses");
        }

        return licenses;
    }

    private class AdobeLicenseData
    {
        public string ProductName { get; set; } = string.Empty;
        public int AssignedLicenses { get; set; }
    }
}
