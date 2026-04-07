using AgroTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroTech.Infrastructure.Data
{
    public class AgroTechDbContext : DbContext
    {
        public AgroTechDbContext(DbContextOptions<AgroTechDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Sensor> Sensors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Farm>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Crop>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PlantingDate)
                    .IsRequired();

                entity.Property(e => e.HarvestDate);

                entity.HasOne(e => e.Farm)
                      .WithMany(f => f.Crops)
                      .HasForeignKey(e => e.FarmId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Value)
                    .IsRequired();

                entity.HasOne(e => e.Farm)
                      .WithMany(f => f.Sensors)
                      .HasForeignKey(e => e.FarmId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);
            });
        }
    }
}
