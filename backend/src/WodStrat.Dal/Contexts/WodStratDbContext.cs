using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;

namespace WodStrat.Dal.Contexts;

public class WodStratDbContext : DbContext, IWodStratDatabase
{
    public WodStratDbContext(DbContextOptions<WodStratDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<Athlete> Athletes => Set<Athlete>();

    public IQueryable<T> Get<T>() where T : class => Set<T>();
    public new void Add<T>(T entity) where T : class => Set<T>().Add(entity);
    public new void Update<T>(T entity) where T : class => Set<T>().Update(entity);
    public new void Remove<T>(T entity) where T : class => Set<T>().Remove(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL enum mappings
        modelBuilder.HasPostgresEnum<ExperienceLevel>("experience_level");
        modelBuilder.HasPostgresEnum<AthleteGoal>("athlete_goal");

        // Configure Athlete entity
        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.ToTable("athletes");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            // User relationship (future FK)
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_athletes_user_id")
                .HasFilter("user_id IS NOT NULL");

            // Profile fields
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.DateOfBirth)
                .HasColumnName("date_of_birth");

            entity.Property(e => e.Gender)
                .HasColumnName("gender")
                .HasMaxLength(20);

            // Physical measurements
            entity.Property(e => e.HeightCm)
                .HasColumnName("height_cm")
                .HasPrecision(5, 2);

            entity.Property(e => e.WeightKg)
                .HasColumnName("weight_kg")
                .HasPrecision(5, 2);

            // Training profile with enum mappings
            entity.Property(e => e.ExperienceLevel)
                .HasColumnName("experience_level")
                .HasDefaultValue(ExperienceLevel.Intermediate);

            entity.Property(e => e.PrimaryGoal)
                .HasColumnName("primary_goal")
                .HasDefaultValue(AthleteGoal.ImprovePacing);

            // Soft delete
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("idx_athletes_is_deleted")
                .HasFilter("is_deleted = FALSE");

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // Additional indexes
            entity.HasIndex(e => e.ExperienceLevel)
                .HasDatabaseName("idx_athletes_experience_level");
        });
    }
}
