using LicenseTracker.Services.Interfaces;
using LicenseTracker.Services.VendorServices;
using Microsoft.AspNetCore.Mvc;

namespace LicenseTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VendorSyncController : ControllerBase
{
    private readonly IEnumerable<IVendorLicenseService> _vendorServices;
    private readonly ILogger<VendorSyncController> _logger;

    public VendorSyncController(
        IEnumerable<IVendorLicenseService> vendorServices,
        ILogger<VendorSyncController> logger)
    {
        _vendorServices = vendorServices;
        _logger = logger;
    }

    /// <summary>
    /// Tüm vendor'lardan lisans bilgilerini senkronize eder
    /// </summary>
    [HttpPost("sync-all")]
    public async Task<ActionResult<List<VendorSyncResult>>> SyncAllVendors()
    {
        _logger.LogInformation("Starting sync for all vendors");
        var results = new List<VendorSyncResult>();

        foreach (var service in _vendorServices)
        {
            try
            {
                var result = await service.SyncLicensesAsync();
                results.Add(result);
                _logger.LogInformation("Synced {VendorName}: {Success}", service.VendorName, result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing {VendorName}", service.VendorName);
                results.Add(new VendorSyncResult
                {
                    VendorName = service.VendorName,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Belirli bir vendor'dan lisans bilgilerini senkronize eder
    /// </summary>
    [HttpPost("sync/{vendorName}")]
    public async Task<ActionResult<VendorSyncResult>> SyncVendor(string vendorName)
    {
        var service = _vendorServices.FirstOrDefault(s =>
            s.VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            return NotFound(new { message = $"Vendor '{vendorName}' not found" });
        }

        try
        {
            var result = await service.SyncLicensesAsync();
            _logger.LogInformation("Synced {VendorName}: {Success}", vendorName, result.Success);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing {VendorName}", vendorName);
            return StatusCode(500, new VendorSyncResult
            {
                VendorName = vendorName,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Tüm vendor'lar için bağlantı testi yapar
    /// </summary>
    [HttpGet("test-all")]
    public async Task<ActionResult<Dictionary<string, bool>>> TestAllConnections()
    {
        var results = new Dictionary<string, bool>();

        foreach (var service in _vendorServices)
        {
            try
            {
                var isConnected = await service.TestConnectionAsync();
                results[service.VendorName] = isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for {VendorName}", service.VendorName);
                results[service.VendorName] = false;
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Belirli bir vendor için bağlantı testi yapar
    /// </summary>
    [HttpGet("test/{vendorName}")]
    public async Task<ActionResult<object>> TestConnection(string vendorName)
    {
        var service = _vendorServices.FirstOrDefault(s =>
            s.VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            return NotFound(new { message = $"Vendor '{vendorName}' not found" });
        }

        try
        {
            var isConnected = await service.TestConnectionAsync();
            return Ok(new
            {
                vendorName = service.VendorName,
                connected = isConnected,
                message = isConnected ? "Connection successful" : "Connection failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection for {VendorName}", vendorName);
            return Ok(new
            {
                vendorName = service.VendorName,
                connected = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Desteklenen tüm vendor'ları listeler
    /// </summary>
    [HttpGet("vendors")]
    public ActionResult<List<string>> GetSupportedVendors()
    {
        var vendors = _vendorServices.Select(s => s.VendorName).OrderBy(n => n).ToList();
        return Ok(vendors);
    }
}
