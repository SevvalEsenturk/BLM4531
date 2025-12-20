using Microsoft.AspNetCore.Identity;

namespace LicenseTracker.Models
{
    // IdentityUser'dan kalıtım alarak temel kimlik alanlarını (kullanıcı adı, şifre hash, vb.) ekler.
    // İhtiyaç duyarsanız buraya ek özellikler (Örn: FirstName, LastName) ekleyebilirsiniz.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}