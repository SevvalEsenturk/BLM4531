using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class AtlassianService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AtlassianService> _logger;

    public string VendorName => "Atlassian";

    public AtlassianService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<AtlassianService> logger)
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
            var (email, apiToken) = GetCredentials();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(apiToken))
            {
                result.ErrorMessage = "Atlassian API credentials not configured";
                return result;
            }

            var products = await FetchAtlassianProductsAsync(email, apiToken);
            result.LicensesFound = products.Count;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Atlassian");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Atlassian",
                    ApiKeyVaultReference = "atlassian-api"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            foreach (var product in products)
            {
                var existingLicense = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name == product.Name && l.Vendor == VendorName);

                if (existingLicense == null)
                {
                    var newLicense = new License
                    {
                        Name = product.Name,
                        Vendor = VendorName,
                        Category = "Project Management",
                        CompanyId = company.Id,
                        HasLicense = true,
                        Users = product.UserCount,
                        StartDate = DateTime.UtcNow,
                        EndDate = null
                    };
                    _context.Licenses.Add(newLicense);
                    result.LicensesAdded++;
                }
                else
                {
                    existingLicense.Users = product.UserCount;
                    result.LicensesUpdated++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Atlassian licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var (email, apiToken) = GetCredentials();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(apiToken)) return false;

            SetBasicAuth(email, apiToken);
            var cloudId = _configuration["VendorAPIs:Atlassian:CloudId"];
            var response = await _httpClient.GetAsync($"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/myself");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Atlassian API connection test failed");
            return false;
        }
    }

    private (string email, string apiToken) GetCredentials()
    {
        var email = _configuration["VendorAPIs:Atlassian:Email"];
        var apiToken = _configuration["VendorAPIs:Atlassian:ApiToken"];
        return (email ?? string.Empty, apiToken ?? string.Empty);
    }

    private void SetBasicAuth(string email, string apiToken)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    private async Task<List<AtlassianProduct>> FetchAtlassianProductsAsync(string email, string apiToken)
    {
        var products = new List<AtlassianProduct>();

        try
        {
            SetBasicAuth(email, apiToken);
            var cloudId = _configuration["VendorAPIs:Atlassian:CloudId"];

            // Jira users
            var jiraResponse = await _httpClient.GetAsync(
                $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/users/search");

            if (jiraResponse.IsSuccessStatusCode)
            {
                var jiraJson = await jiraResponse.Content.ReadAsStringAsync();
                var jiraUsers = JsonSerializer.Deserialize<JsonElement>(jiraJson);
                var jiraUserCount = jiraUsers.GetArrayLength();

                products.Add(new AtlassianProduct
                {
                    Name = "Jira Software",
                    UserCount = jiraUserCount
                });
            }

            // Confluence would be similar
            products.Add(new AtlassianProduct
            {
                Name = "Confluence",
                UserCount = 0 // API call yapılacak
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Atlassian products");
        }

        return products;
    }

    private class AtlassianProduct
    {
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
