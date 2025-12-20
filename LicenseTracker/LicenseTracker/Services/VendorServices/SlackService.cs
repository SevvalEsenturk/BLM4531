using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LicenseTracker.Services.VendorServices;

public class SlackService : IVendorLicenseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackService> _logger;

    public string VendorName => "Slack";

    public SlackService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<SlackService> logger)
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
            var apiToken = _configuration["VendorAPIs:Slack:BotToken"];
            if (string.IsNullOrEmpty(apiToken))
            {
                result.ErrorMessage = "Slack API token not configured";
                return result;
            }

            var workspaceInfo = await FetchWorkspaceInfoAsync(apiToken);
            if (workspaceInfo == null)
            {
                result.ErrorMessage = "Failed to fetch Slack workspace info";
                return result;
            }

            result.LicensesFound = 1;

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == "Slack");

            if (company == null)
            {
                company = new Company
                {
                    Name = "Slack",
                    ApiKeyVaultReference = "slack-bot-token"
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Name == "Slack Workspace" && l.Vendor == VendorName);

            if (existingLicense == null)
            {
                var newLicense = new License
                {
                    Name = "Slack Workspace",
                    Vendor = VendorName,
                    Category = "Communication",
                    CompanyId = company.Id,
                    HasLicense = true,
                    Users = workspaceInfo.ActiveUsers,
                    StartDate = DateTime.UtcNow,
                    EndDate = null
                };
                _context.Licenses.Add(newLicense);
                result.LicensesAdded++;
            }
            else
            {
                existingLicense.Users = workspaceInfo.ActiveUsers;
                result.LicensesUpdated++;
            }

            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Slack licenses");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var apiToken = _configuration["VendorAPIs:Slack:BotToken"];
            if (string.IsNullOrEmpty(apiToken)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            var response = await _httpClient.GetAsync("https://slack.com/api/auth.test");

            if (!response.IsSuccessStatusCode) return false;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            return data.GetProperty("ok").GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slack API connection test failed");
            return false;
        }
    }

    private async Task<SlackWorkspaceInfo?> FetchWorkspaceInfoAsync(string apiToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            // Get team info
            var teamResponse = await _httpClient.GetAsync("https://slack.com/api/team.info");

            // Get billable users count
            var usersResponse = await _httpClient.GetAsync("https://slack.com/api/team.billableInfo");

            if (!teamResponse.IsSuccessStatusCode || !usersResponse.IsSuccessStatusCode)
                return null;

            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            var usersData = JsonSerializer.Deserialize<JsonElement>(usersJson);

            if (usersData.TryGetProperty("billable_info", out var billableInfo))
            {
                var activeUsers = billableInfo.EnumerateObject().Count();
                return new SlackWorkspaceInfo { ActiveUsers = activeUsers };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Slack workspace info");
            return null;
        }
    }

    private class SlackWorkspaceInfo
    {
        public int ActiveUsers { get; set; }
    }
}
