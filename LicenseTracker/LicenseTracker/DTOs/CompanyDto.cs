using System.Collections.Generic;

namespace LicenseTracker.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiKeyVaultReference { get; set; } = string.Empty;
        public List<LicenseDto> Licenses { get; set; } = new List<LicenseDto>();
    }

    public class CreateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string ApiKeyVaultReference { get; set; } = string.Empty;
    }

    public class UpdateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string ApiKeyVaultReference { get; set; } = string.Empty;
    }
}
