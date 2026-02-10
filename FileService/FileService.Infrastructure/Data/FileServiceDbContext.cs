using Microsoft.EntityFrameworkCore;
using FileService.Domain.Entities;
using DomainFileShare = FileService.Domain.Entities.FileShare;

namespace FileService.Infrastructure.Data;

public class FileServiceDbContext : DbContext
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) : base(options)
    {
    }

    public DbSet<FileEntity> Files { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<DomainFileShare> FileShares { get; set; }
    public DbSet<ShareAccess> ShareAccesses { get; set; }
    public DbSet<FilePermission> FilePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileEntity>(entity =>
        {
            entity.ToTable("T_Files");
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(e => e.Metadata, metadata =>
            {
                metadata.Property(m => m.FileName).HasColumnName("FileName").HasMaxLength(255).IsRequired();
                metadata.Property(m => m.ContentType).HasColumnName("ContentType").HasMaxLength(100);
                metadata.Property(m => m.FileSize).HasColumnName("FileSize");
                metadata.Property(m => m.Extension).HasColumnName("Extension").HasMaxLength(20);
                metadata.Property(m => m.Hash).HasColumnName("Hash").HasMaxLength(128);
            });

            entity.OwnsOne(e => e.EncryptionInfo, encryption =>
            {
                encryption.Property(e => e.Algorithm).HasColumnName("EncryptionAlgorithm").HasMaxLength(50);
                encryption.Property(e => e.EncryptedPath).HasColumnName("EncryptedPath").HasMaxLength(500);
                encryption.Property(e => e.KeyIdentifier).HasColumnName("KeyIdentifier").HasMaxLength(100);
                encryption.Property(e => e.EncryptedAt).HasColumnName("EncryptedAt");
            });

            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.VirusScanResult).HasMaxLength(500);
            
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.Status);

            entity.HasMany(e => e.Shares)
                .WithOne()
                .HasForeignKey("FileId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Permissions)
                .WithOne()
                .HasForeignKey("FileId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.ToTable("T_Folders");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.OwnerId).IsRequired();
            
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ParentFolderId);
            entity.HasIndex(e => e.Path);
        });

        modelBuilder.Entity<DomainFileShare>(entity =>
        {
            entity.ToTable("T_FileShares");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Token).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileId).IsRequired();
            
            entity.OwnsOne(e => e.Settings, settings =>
            {
                settings.Property(s => s.ExpirationDate).HasColumnName("ExpirationDate");
                settings.Property(s => s.MaxAccessCount).HasColumnName("MaxAccessCount");
                settings.Property(s => s.Password).HasColumnName("Password").HasMaxLength(500);
                settings.Property(s => s.RequireAuthentication).HasColumnName("RequireAuthentication");
            });
            
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.FileId);

            entity.HasMany(e => e.AccessHistory)
                .WithOne()
                .HasForeignKey("ShareId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShareAccess>(entity =>
        {
            entity.ToTable("T_ShareAccesses");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ShareId).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(200);
            
            entity.HasIndex(e => e.ShareId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<FilePermission>(entity =>
        {
            entity.ToTable("T_FilePermissions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Permissions).IsRequired();
            
            entity.HasIndex(e => new { e.FileId, e.UserId }).IsUnique();
        });
    }
}
