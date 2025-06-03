using System.ComponentModel.DataAnnotations;


namespace DafornoMail.Core.Models;

public class EmailMessage
{
    public Guid Id { get; set; }
    
    [Required]
    public string MessageId { get; set; } = string.Empty; // Unique ID from the email provider
    
    [Required]
    public string Subject { get; set; } = "(No subject)";
    
    public string? PreviewText { get; set; }
    
    public string? HtmlBody { get; set; }
    
    public string? TextBody { get; set; }
    
    [Required]
    public DateTimeOffset Date { get; set; }
    
    public bool IsRead { get; set; }
    
    public bool IsFlagged { get; set; }
    
    public bool IsAnswered { get; set; }
    
    public bool IsDraft { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public string? Labels { get; set; } // JSON array of label IDs for Gmail
    
    public string? Headers { get; set; } // Serialized email headers
    
    // Sender information
    public string From { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    
    // Recipients (stored as JSON)
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string? ReplyTo { get; set; }
    
    // Attachments info (metadata only, actual files stored in blob storage)
    public bool HasAttachments { get; set; }
    public string? AttachmentsMetadata { get; set; } // JSON array of attachment metadata
    
    // Foreign keys
    public Guid EmailAccountId { get; set; }
    public Guid? FolderId { get; set; }
    
    // Navigation properties
    public EmailAccount EmailAccount { get; set; } = null!;
    public EmailFolder? Folder { get; set; }
    
    // For thread support
    public string? ThreadId { get; set; } // For Gmail threads
    public string? References { get; set; } // Message-Id references for threading
    public string? InReplyTo { get; set; } // Message-Id this is a reply to
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
}
