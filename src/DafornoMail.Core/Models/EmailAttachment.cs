using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DafornoMail.Core.Models;

public class EmailAttachment
{
    public Guid Id { get; set; }
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public string? ContentId { get; set; } // For inline images in HTML emails
    
    // Path to the file in blob storage
    public string? BlobPath { get; set; }
    
    // Foreign key
    public Guid MessageId { get; set; }
    
    // Navigation property
    public EmailMessage Message { get; set; } = null!;
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Helper properties
    [NotMapped]
    public string? DownloadUrl { get; set; } // Will be set when generating download links
    
    [NotMapped]
    public Stream? Content { get; set; } // For handling uploads/downloads
}
