using DafornoMail.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace DafornoMail.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmailAccount> EmailAccounts { get; set; } = null!;
    public DbSet<EmailFolder> EmailFolders { get; set; } = null!;
    public DbSet<EmailMessage> EmailMessages { get; set; } = null!;
    public DbSet<EmailAttachment> EmailAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure EmailAccount entity
        modelBuilder.Entity<EmailAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.KeyVaultSecretName).IsRequired().HasMaxLength(256);
            
            // Relationships
            entity.HasMany(e => e.Folders)
                .WithOne(f => f.EmailAccount)
                .HasForeignKey(f => f.EmailAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmailFolder entity
        modelBuilder.Entity<EmailFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(1000);
            entity.Property(e => e.OriginalName).HasMaxLength(1000);
            entity.Property(e => e.Attributes).HasColumnType("nvarchar(max)");
            
            // Relationships
            entity.HasOne(f => f.EmailAccount)
                .WithMany(a => a.Folders)
                .HasForeignKey(f => f.EmailAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmailMessage entity
        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageId).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Subject).HasMaxLength(1000);
            entity.Property(e => e.PreviewText).HasMaxLength(1000);
            entity.Property(e => e.HtmlBody).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TextBody).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Labels).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Headers).HasColumnType("nvarchar(max)");
            entity.Property(e => e.From).HasMaxLength(1000);
            entity.Property(e => e.SenderName).HasMaxLength(500);
            entity.Property(e => e.SenderEmail).HasMaxLength(500);
            entity.Property(e => e.To).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Cc).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Bcc).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ReplyTo).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AttachmentsMetadata).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ThreadId).HasMaxLength(1000);
            entity.Property(e => e.References).HasMaxLength(4000);
            entity.Property(e => e.InReplyTo).HasMaxLength(1000);
            
            // Indexes
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.IsFlagged);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.EmailAccountId);
            entity.HasIndex(e => e.ThreadId);
            
            // Relationships
            entity.HasOne(m => m.EmailAccount)
                .WithMany()
                .HasForeignKey(m => m.EmailAccountId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(m => m.Folder)
                .WithMany(f => f.Messages)
                .HasForeignKey(m => m.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure EmailAttachment entity
        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(255);
            entity.Property(e => e.ContentId).HasMaxLength(255);
            entity.Property(e => e.BlobPath).HasMaxLength(1000);
            
            // Relationships
            entity.HasOne(a => a.Message)
                .WithMany()
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
