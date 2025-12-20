namespace LicenseTracker.Services.Interfaces;

public interface IVendorLicenseService
{
    /// <summary>
    /// Vendor adı (Microsoft, Adobe, Slack, vb.)
    /// </summary>
    string VendorName { get; }

    /// <summary>
    /// Vendor'dan lisans bilgilerini çeker ve veritabanına kaydeder
    /// </summary>
    Task<VendorSyncResult> SyncLicensesAsync();

    /// <summary>
    /// Vendor API bağlantısının çalışıp çalışmadığını kontrol eder
    /// </summary>
    Task<bool> TestConnectionAsync();
}

public class VendorSyncResult
{
    public bool Success { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int LicensesFound { get; set; }
    public int LicensesAdded { get; set; }
    public int LicensesUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncTime { get; set; } = DateTime.UtcNow;
}
