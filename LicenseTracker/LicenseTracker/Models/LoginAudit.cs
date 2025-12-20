using System;
using System.ComponentModel.DataAnnotations;

namespace LicenseTracker.Models
{
    public class LoginAudit
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; } // Nullable if login failed (unknown user)
        
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        public bool IsSuccess { get; set; }
        
        public string IPAddress { get; set; } = string.Empty;
        
        public string UserAgent { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? FailureReason { get; set; }
    }
}
