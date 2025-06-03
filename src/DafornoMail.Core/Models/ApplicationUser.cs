using Microsoft.AspNetCore.Identity;

namespace DafornoMail.Core.Models;

public class ApplicationUser : IdentityUser
{
    // Custom properties can be added here
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TimeZone { get; set; } = "UTC";
    public string? CultureInfo { get; set; } = "en-US";
    
    // Navigation properties
    public virtual ICollection<EmailAccount> EmailAccounts { get; set; } = new List<EmailAccount>();
    
    // Add any additional properties or methods as needed
}
