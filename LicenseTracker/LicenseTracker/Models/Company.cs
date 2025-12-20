// Models/Company.cs
using System.ComponentModel.DataAnnotations;

namespace LicenseTracker.Models
{
    public class Company
    {
        // Şirket için anahtar (primary key)
        [Key]
        public int Id { get; set; }

        // Şirketin adı (Örn: "TechCorp A.Ş.")
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; //başlangıç değeri de atadık, hata alıyorduk...

        // Şirket için API anahtarlarının depolandığı yere referans ama en başta bu olmayabilir diye boş string atadık
        // Bu, Azure Key Vault veya benzeri güvenli bir depolama olabilir.
        public string ApiKeyVaultReference { get; set; } = string.Empty;

        // Navigasyon özelliği: Bu şirkete ait lisanslar
        public ICollection<License> Licenses { get; set; } = new List<License>();
        
        // Bu şirkete ait kullanıcılar
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}