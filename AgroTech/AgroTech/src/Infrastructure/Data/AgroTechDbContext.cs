using AgroTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroTech.Infrastructure.Data
{
    public class AgroTechDbContext : DbContext
    {
        public AgroTechDbContext(DbContextOptions<AgroTechDbContext> options) : base(options)
        {
        }

        public DbSet<Sensor> Sensors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.Value)
                    .IsRequired();

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);
            });
        }
    }
}