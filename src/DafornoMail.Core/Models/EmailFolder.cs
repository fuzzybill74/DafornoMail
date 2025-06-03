using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DafornoMail.Core.Models;

public class EmailFolder
{
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? FullName { get; set; } // For Gmail labels with hierarchy (e.g., "Parent/Child")
    
    public string? OriginalName { get; set; } // Original name from the email provider
    
    public bool IsSystemFolder { get; set; } // INBOX, Sent, Drafts, etc.
    
    public bool IsSelectable { get; set; } = true; // Some folders like [Gmail] are not selectable
    
    public bool HasChildren { get; set; }
    
    public string? ParentFolderId { get; set; } // For nested folders/labels
    
    public string? Attributes { get; set; } // JSON string of folder attributes
    
    public DateTime LastSynced { get; set; }
    
    // Foreign key
    [Required]
    public Guid EmailAccountId { get; set; }
    
    // Navigation properties
    public EmailAccount EmailAccount { get; set; } = null!;
    public ICollection<EmailMessage> Messages { get; set; } = new List<EmailMessage>();
}
