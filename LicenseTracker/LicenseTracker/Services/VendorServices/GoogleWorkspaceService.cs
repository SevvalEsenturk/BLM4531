using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class GoogleWorkspaceService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleWorkspaceService> _logger;

    public string VendorName => "Google";

    public GoogleWorkspaceService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<GoogleWorkspaceService> logger)
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
                result.ErrorMessage = "Google Workspace API authentication failed";
                return result;
            }

            var subscriptions = await FetchSubscriptionsAsync(accessToken);
            result.LicensesFound = subscriptions.Count;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Google");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Google",
                    ApiKeyVaultReference = "google-workspace-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            foreach (var subscription in subscriptions)
            {
                var existingLicense = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name == subscription.PlanName && l.Vendor == VendorName);

                if (existingLicense == null)
                {
                    var newLicense = new License
                    {
                        Name = subscription.PlanName,
                        Vendor = VendorName,
                        Category = "Cloud Service",
                        CompanyId = company.Id,
                        HasLicense = true,
                        Users = subscription.LicenseCount,
                        StartDate = DateTime.UtcNow,
                        EndDate = null
                    };
                    _context.Licenses.Add(newLicense);
                    result.LicensesAdded++;
                }
                else
                {
                    existingLicense.Users = subscription.LicenseCount;
                    result.LicensesUpdated++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Google Workspace licenses");
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
            var customerId = _configuration["VendorAPIs:Google:CustomerId"];
            var response = await _httpClient.GetAsync($"https://admin.googleapis.com/admin/directory/v1/customer/{customerId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Workspace API connection test failed");
            return false;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var serviceAccountKey = _configuration["VendorAPIs:Google:ServiceAccountKey"];
            if (string.IsNullOrEmpty(serviceAccountKey))
            {
                _logger.LogWarning("Google Workspace service account key not configured");
                return string.Empty;
            }

            // Google OAuth2 service account authentication
            // Not: Gerçek implementasyon için JWT token oluşturulması gerekir
            return string.Empty; // TODO: Implement JWT generation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google Workspace access token");
            return string.Empty;
        }
    }

    private async Task<List<GoogleSubscription>> FetchSubscriptionsAsync(string accessToken)
    {
        var subscriptions = new List<GoogleSubscription>();

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var customerId = _configuration["VendorAPIs:Google:CustomerId"];

            var response = await _httpClient.GetAsync(
                $"https://reseller.googleapis.com/apps/reseller/v1/subscriptions?customerId={customerId}");

            if (!response.IsSuccessStatusCode) return subscriptions;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (data.TryGetProperty("subscriptions", out var subs))
            {
                foreach (var sub in subs.EnumerateArray())
                {
                    var planName = sub.GetProperty("plan").GetProperty("planName").GetString() ?? "Unknown";
                    var licenseCount = sub.GetProperty("seats").GetProperty("numberOfSeats").GetInt32();

                    subscriptions.Add(new GoogleSubscription
                    {
                        PlanName = $"Google Workspace {planName}",
                        LicenseCount = licenseCount
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Google Workspace subscriptions");
        }

        return subscriptions;
    }

    private class GoogleSubscription
    {
        public string PlanName { get; set; } = string.Empty;
        public int LicenseCount { get; set; }
    }
}
