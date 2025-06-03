using System.ComponentModel.DataAnnotations;

namespace DafornoMail.Core.Models;

public class EmailAccount
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    public string Provider { get; set; } = string.Empty; // Gmail, Outlook, etc.
    
    public bool IsDefault { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // These will be stored in Azure Key Vault, not in the database
    [Required]
    public string KeyVaultSecretName { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<EmailFolder> Folders { get; set; } = new List<EmailFolder>();
}
