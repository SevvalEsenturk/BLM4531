using System;
using System.ComponentModel.DataAnnotations;

namespace LicenseTracker.Models
{
    public class License
    {
        [Key]
        public int Id { get; set; }  // Birincil anahtar

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // Ürün adı (örn: "Microsoft Office 365")

        [Required]
        [MaxLength(100)]
        public string Vendor { get; set; } = string.Empty; // Satıcı (örn: "Microsoft")

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Kategori (örn: "Productivity")

        public string? UserId { get; set; } // Kullanıcı kimliği
        
        public int? CompanyId { get; set; } // Şirket kimliği (opsiyonel foreign key)

        // Navigation property
        public Company? Company { get; set; }

        public bool HasLicense { get; set; } = false; // Lisans var mı?

        public DateTime? StartDate { get; set; } // Başlangıç tarihi

        public DateTime? EndDate { get; set; } // Bitiş tarihi (ExpiryDate yerine)

        public decimal? Cost { get; set; } // Yıllık maliyet

        public int? Users { get; set; } // Kullanıcı sayısı

        public int RemainingDays
        {
            get
            {
                if (!HasLicense || !EndDate.HasValue)
                    return 0;

                return (EndDate.Value - DateTime.Now).Days;
            }
        }
    }
}
