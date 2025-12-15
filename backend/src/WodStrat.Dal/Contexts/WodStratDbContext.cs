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
    public DbSet<BenchmarkDefinition> BenchmarkDefinitions => Set<BenchmarkDefinition>();
    public DbSet<AthleteBenchmark> AthleteBenchmarks => Set<AthleteBenchmark>();

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
        modelBuilder.HasPostgresEnum<BenchmarkCategory>("benchmark_category");
        modelBuilder.HasPostgresEnum<BenchmarkMetricType>("benchmark_metric_type");

        // Configure Athlete entity
        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.ToTable("athletes");

            // Primary key (SERIAL INT - auto-generated)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // User relationship (future FK - now INT)
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

        // Configure BenchmarkDefinition entity
        modelBuilder.Entity<BenchmarkDefinition>(entity =>
        {
            entity.ToTable("benchmark_definitions");

            // Primary key (SERIAL INT - auto-generated)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Benchmark identification
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            // Classification
            entity.Property(e => e.Category)
                .HasColumnName("category")
                .IsRequired();

            entity.Property(e => e.MetricType)
                .HasColumnName("metric_type")
                .IsRequired();

            entity.Property(e => e.Unit)
                .HasColumnName("unit")
                .HasMaxLength(50)
                .IsRequired();

            // Status and ordering
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.DisplayOrder)
                .HasColumnName("display_order")
                .HasDefaultValue(0);

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Indexes
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("uq_benchmark_definitions_name")
                .IsUnique();

            entity.HasIndex(e => e.Slug)
                .HasDatabaseName("uq_benchmark_definitions_slug")
                .IsUnique();

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("idx_benchmark_definitions_category");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_benchmark_definitions_is_active")
                .HasFilter("is_active = TRUE");
        });

        // Configure AthleteBenchmark entity
        modelBuilder.Entity<AthleteBenchmark>(entity =>
        {
            entity.ToTable("athlete_benchmarks");

            // Primary key (SERIAL INT - auto-generated)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign keys (now INT)
            entity.Property(e => e.AthleteId)
                .HasColumnName("athlete_id")
                .IsRequired();

            entity.Property(e => e.BenchmarkDefinitionId)
                .HasColumnName("benchmark_definition_id")
                .IsRequired();

            // Benchmark data
            entity.Property(e => e.Value)
                .HasColumnName("value")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.RecordedAt)
                .HasColumnName("recorded_at")
                .HasDefaultValueSql("CURRENT_DATE")
                .IsRequired();

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);

            // Soft delete
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.Athlete)
                .WithMany(a => a.Benchmarks)
                .HasForeignKey(e => e.AthleteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BenchmarkDefinition)
                .WithMany(bd => bd.AthleteBenchmarks)
                .HasForeignKey(e => e.BenchmarkDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.AthleteId)
                .HasDatabaseName("idx_athlete_benchmarks_athlete_id");

            entity.HasIndex(e => e.BenchmarkDefinitionId)
                .HasDatabaseName("idx_athlete_benchmarks_definition_id");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("idx_athlete_benchmarks_is_deleted")
                .HasFilter("is_deleted = FALSE");

            entity.HasIndex(e => new { e.AthleteId, e.IsDeleted })
                .HasDatabaseName("idx_athlete_benchmarks_athlete_active")
                .HasFilter("is_deleted = FALSE");

            // Unique constraint
            entity.HasIndex(e => new { e.AthleteId, e.BenchmarkDefinitionId })
                .HasDatabaseName("uq_athlete_benchmarks_athlete_definition")
                .IsUnique();
        });
    }
}
