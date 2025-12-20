using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class MicrosoftGraphService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MicrosoftGraphService> _logger;

    public string VendorName => "Microsoft";

    public MicrosoftGraphService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<MicrosoftGraphService> logger)
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
            // Access token al
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                result.ErrorMessage = "Microsoft Graph API authentication failed";
                return result;
            }

            // Microsoft 365 lisanslarını çek
            var licenses = await FetchLicensesFromGraphApiAsync(accessToken);
            result.LicensesFound = licenses.Count;

            // Company'yi bul veya oluştur
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Microsoft");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Microsoft",
                    ApiKeyVaultReference = "microsoft-graph-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            // Lisansları kaydet veya güncelle
            foreach (var licenseData in licenses)
            {
                var existingLicense = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name == licenseData.Name && l.Vendor == VendorName);

                if (existingLicense == null)
                {
                    // Yeni lisans ekle
                    var newLicense = new License
                    {
                        Name = licenseData.Name,
                        Vendor = VendorName,
                        Category = "Cloud Service",
                        CompanyId = company.Id,
                        HasLicense = true,
                        Users = licenseData.AssignedCount,
                        StartDate = DateTime.UtcNow,
                        EndDate = null // Microsoft 365 genelde subscription based
                    };
                    _context.Licenses.Add(newLicense);
                    result.LicensesAdded++;
                }
                else
                {
                    // Mevcut lisansı güncelle
                    existingLicense.Users = licenseData.AssignedCount;
                    existingLicense.HasLicense = licenseData.AssignedCount > 0;
                    result.LicensesUpdated++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Microsoft Graph licenses");
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
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/organization");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft Graph connection test failed");
            return false;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var tenantId = _configuration["VendorAPIs:Microsoft:TenantId"];
            var clientId = _configuration["VendorAPIs:Microsoft:ClientId"];
            var clientSecret = _configuration["VendorAPIs:Microsoft:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("Microsoft Graph API credentials not configured");
                return string.Empty;
            }

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            return tokenResponse.GetProperty("access_token").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft Graph access token");
            return string.Empty;
        }
    }

    private async Task<List<MicrosoftLicenseData>> FetchLicensesFromGraphApiAsync(string accessToken)
    {
        var licenses = new List<MicrosoftLicenseData>();

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/subscribedSkus");

            if (!response.IsSuccessStatusCode) return licenses;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (data.TryGetProperty("value", out var skus))
            {
                foreach (var sku in skus.EnumerateArray())
                {
                    var skuPartNumber = sku.GetProperty("skuPartNumber").GetString() ?? "Unknown";
                    var consumedUnits = sku.GetProperty("consumedUnits").GetInt32();

                    licenses.Add(new MicrosoftLicenseData
                    {
                        Name = GetFriendlySkuName(skuPartNumber),
                        AssignedCount = consumedUnits
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching licenses from Microsoft Graph");
        }

        return licenses;
    }

    private string GetFriendlySkuName(string skuPartNumber)
    {
        return skuPartNumber switch
        {
            "ENTERPRISEPACK" => "Office 365 E3",
            "ENTERPRISEPREMIUM" => "Office 365 E5",
            "SPE_E3" => "Microsoft 365 E3",
            "SPE_E5" => "Microsoft 365 E5",
            "PROJECTPROFESSIONAL" => "Project Plan 3",
            "VISIOCLIENT" => "Visio Plan 2",
            "POWER_BI_PRO" => "Power BI Pro",
            _ => skuPartNumber
        };
    }

    private class MicrosoftLicenseData
    {
        public string Name { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
    }
}
