using System;

namespace LicenseTracker.DTOs
{
    public class LicenseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public bool HasLicense { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Cost { get; set; }
        public int? Users { get; set; }
        public int RemainingDays { get; set; }
    }

    public class CreateLicenseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public bool HasLicense { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Cost { get; set; }
        public int? Users { get; set; }
    }

    public class UpdateLicenseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public bool HasLicense { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Cost { get; set; }
        public int? Users { get; set; }
    }
}
