using Microsoft.EntityFrameworkCore;
using WarpTube.Shared.Models;

namespace WarpTube.Shared.Data;

public class WarpTubeDbContext : DbContext
{
    public WarpTubeDbContext(DbContextOptions<WarpTubeDbContext> options) : base(options)
    {
    }

    public DbSet<Video> Videos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Video entity
        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.VideoId)
                  .IsUnique();

            entity.Property(e => e.VideoId)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Title)
                  .HasMaxLength(500)
                  .IsRequired();

            entity.Property(e => e.Description)
                  .HasMaxLength(5000)
                  .IsRequired(false);

            entity.Property(e => e.Channel)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(e => e.ChannelId)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(e => e.ThumbnailUrl)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(e => e.OriginalUrl)
                  .HasMaxLength(500)
                  .IsRequired();


            entity.Property(e => e.ErrorMessage)
                  .HasMaxLength(1000);

            entity.Property(e => e.Format)
                  .HasMaxLength(100);

            entity.Property(e => e.Resolution)
                  .HasMaxLength(20);

            entity.Property(e => e.AudioCodec)
                  .HasMaxLength(50);

            entity.Property(e => e.VideoCodec)
                  .HasMaxLength(50);

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                  .ValueGeneratedOnAddOrUpdate();
        });
    }
}
