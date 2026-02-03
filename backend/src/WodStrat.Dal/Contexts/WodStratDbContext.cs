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
    public DbSet<User> Users => Set<User>();
    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<BenchmarkDefinition> BenchmarkDefinitions => Set<BenchmarkDefinition>();
    public DbSet<AthleteBenchmark> AthleteBenchmarks => Set<AthleteBenchmark>();
    public DbSet<MovementDefinition> MovementDefinitions => Set<MovementDefinition>();
    public DbSet<MovementAlias> MovementAliases => Set<MovementAlias>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutMovement> WorkoutMovements => Set<WorkoutMovement>();
    public DbSet<BenchmarkMovementMapping> BenchmarkMovementMappings => Set<BenchmarkMovementMapping>();
    public DbSet<PopulationBenchmarkPercentile> PopulationBenchmarkPercentiles => Set<PopulationBenchmarkPercentile>();

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
        modelBuilder.HasPostgresEnum<WorkoutType>("workout_type");
        modelBuilder.HasPostgresEnum<MovementCategory>("movement_category");
        modelBuilder.HasPostgresEnum<LoadUnit>("load_unit");
        modelBuilder.HasPostgresEnum<DistanceUnit>("distance_unit");
        modelBuilder.HasPostgresEnum<RepSchemeType>("rep_scheme_type");
        modelBuilder.HasPostgresEnum<PacingLevel>("pacing_level");

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            // Primary key (SERIAL INT - auto-generated)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Authentication fields
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            // Account status
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // Indexes
            entity.HasIndex(e => e.Email)
                .HasDatabaseName("idx_users_email");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_users_is_active")
                .HasFilter("is_active = TRUE");

            entity.HasIndex(e => new { e.Email, e.IsActive })
                .HasDatabaseName("idx_users_email_active")
                .HasFilter("is_active = TRUE");

            // Unique constraint on email
            entity.HasIndex(e => e.Email)
                .HasDatabaseName("uq_users_email")
                .IsUnique();
        });

        // Configure Athlete entity
        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.ToTable("athletes");

            // Primary key (SERIAL INT - auto-generated)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // User relationship
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_athletes_user_id")
                .HasFilter("user_id IS NOT NULL");

            // One-to-one relationship with User
            entity.HasOne(e => e.User)
                .WithOne(u => u.Athlete)
                .HasForeignKey<Athlete>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on user_id
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("uq_athletes_user_id")
                .IsUnique();

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

        // Configure MovementDefinition entity
        modelBuilder.Entity<MovementDefinition>(entity =>
        {
            entity.ToTable("movement_definitions");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Movement identification
            entity.Property(e => e.CanonicalName)
                .HasColumnName("canonical_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(100)
                .IsRequired();

            // Classification
            entity.Property(e => e.Category)
                .HasColumnName("category")
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            // Equipment array
            entity.Property(e => e.Equipment)
                .HasColumnName("equipment");

            // Default load unit
            entity.Property(e => e.DefaultLoadUnit)
                .HasColumnName("default_load_unit");

            // Bodyweight indicator
            entity.Property(e => e.IsBodyweight)
                .HasColumnName("is_bodyweight")
                .HasDefaultValue(false);

            // RX weights indicator
            entity.Property(e => e.HasRxWeights)
                .HasColumnName("has_rx_weights")
                .HasDefaultValue(false);

            // Scaling options (JSONB stored as string)
            entity.Property(e => e.ScalingOptions)
                .HasColumnName("scaling_options")
                .HasColumnType("jsonb");

            // Status
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

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

            // Indexes
            entity.HasIndex(e => e.CanonicalName)
                .HasDatabaseName("uq_movement_definitions_canonical_name")
                .IsUnique();

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("idx_movement_definitions_category");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_movement_definitions_is_active")
                .HasFilter("is_active = TRUE");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("idx_movement_definitions_is_deleted")
                .HasFilter("is_deleted = FALSE");

            entity.HasIndex(e => e.IsBodyweight)
                .HasDatabaseName("idx_movement_definitions_is_bodyweight")
                .HasFilter("is_bodyweight = TRUE");

            entity.HasIndex(e => e.HasRxWeights)
                .HasDatabaseName("idx_movement_definitions_has_rx_weights")
                .HasFilter("has_rx_weights = TRUE");
        });

        // Configure MovementAlias entity
        modelBuilder.Entity<MovementAlias>(entity =>
        {
            entity.ToTable("movement_aliases");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign key
            entity.Property(e => e.MovementDefinitionId)
                .HasColumnName("movement_definition_id")
                .IsRequired();

            // Alias data
            entity.Property(e => e.Alias)
                .HasColumnName("alias")
                .HasMaxLength(100)
                .IsRequired();

            // Normalized alias for matching
            entity.Property(e => e.AliasNormalized)
                .HasColumnName("alias_normalized")
                .HasMaxLength(100)
                .IsRequired();

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.MovementDefinition)
                .WithMany(md => md.Aliases)
                .HasForeignKey(e => e.MovementDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.Alias)
                .HasDatabaseName("uq_movement_aliases_alias")
                .IsUnique();

            entity.HasIndex(e => e.AliasNormalized)
                .HasDatabaseName("uq_movement_aliases_alias_normalized")
                .IsUnique();

            entity.HasIndex(e => e.AliasNormalized)
                .HasDatabaseName("idx_movement_aliases_alias_normalized");

            entity.HasIndex(e => e.MovementDefinitionId)
                .HasDatabaseName("idx_movement_aliases_definition_id");
        });

        // Configure Workout entity
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("workouts");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign key
            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            // Workout identification
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200);

            entity.Property(e => e.WorkoutType)
                .HasColumnName("workout_type")
                .IsRequired();

            // Raw and parsed text
            entity.Property(e => e.OriginalText)
                .HasColumnName("original_text")
                .IsRequired();

            entity.Property(e => e.ParsedDescription)
                .HasColumnName("parsed_description");

            // Time domain
            entity.Property(e => e.TimeCapSeconds)
                .HasColumnName("time_cap_seconds");

            entity.Property(e => e.RoundCount)
                .HasColumnName("round_count");

            entity.Property(e => e.IntervalDurationSeconds)
                .HasColumnName("interval_duration_seconds");

            // Parse confidence
            entity.Property(e => e.ParseConfidence)
                .HasColumnName("parse_confidence")
                .HasPrecision(3, 2);

            // Rep scheme type
            entity.Property(e => e.RepSchemeType)
                .HasColumnName("rep_scheme_type");

            // Rep scheme reps array
            entity.Property(e => e.RepSchemeReps)
                .HasColumnName("rep_scheme_reps");

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
            entity.HasOne(e => e.User)
                .WithMany(u => u.Workouts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_workouts_user_id");

            entity.HasIndex(e => e.WorkoutType)
                .HasDatabaseName("idx_workouts_workout_type");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("idx_workouts_is_deleted")
                .HasFilter("is_deleted = FALSE");

            entity.HasIndex(e => new { e.UserId, e.IsDeleted })
                .HasDatabaseName("idx_workouts_user_active")
                .HasFilter("is_deleted = FALSE");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_workouts_created_at")
                .IsDescending();

            // Parse confidence index
            entity.HasIndex(e => e.ParseConfidence)
                .HasDatabaseName("idx_workouts_parse_confidence")
                .HasFilter("parse_confidence IS NOT NULL")
                .IsDescending();

            // Rep scheme type index
            entity.HasIndex(e => e.RepSchemeType)
                .HasDatabaseName("idx_workouts_rep_scheme_type")
                .HasFilter("rep_scheme_type IS NOT NULL");
        });

        // Configure WorkoutMovement entity
        modelBuilder.Entity<WorkoutMovement>(entity =>
        {
            entity.ToTable("workout_movements");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign keys
            entity.Property(e => e.WorkoutId)
                .HasColumnName("workout_id")
                .IsRequired();

            entity.Property(e => e.MovementDefinitionId)
                .HasColumnName("movement_definition_id")
                .IsRequired();

            // Ordering
            entity.Property(e => e.SequenceOrder)
                .HasColumnName("sequence_order")
                .IsRequired();

            // Movement specifications
            entity.Property(e => e.RepCount)
                .HasColumnName("rep_count");

            entity.Property(e => e.LoadValue)
                .HasColumnName("load_value")
                .HasPrecision(8, 2);

            entity.Property(e => e.LoadUnit)
                .HasColumnName("load_unit");

            entity.Property(e => e.DistanceValue)
                .HasColumnName("distance_value")
                .HasPrecision(8, 2);

            entity.Property(e => e.DistanceUnit)
                .HasColumnName("distance_unit");

            entity.Property(e => e.Calories)
                .HasColumnName("calories");

            entity.Property(e => e.DurationSeconds)
                .HasColumnName("duration_seconds");

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);

            // EMOM minute range
            entity.Property(e => e.MinuteStart)
                .HasColumnName("minute_start");

            entity.Property(e => e.MinuteEnd)
                .HasColumnName("minute_end");

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.Workout)
                .WithMany(w => w.Movements)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MovementDefinition)
                .WithMany(md => md.WorkoutMovements)
                .HasForeignKey(e => e.MovementDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.WorkoutId)
                .HasDatabaseName("idx_workout_movements_workout_id");

            entity.HasIndex(e => e.MovementDefinitionId)
                .HasDatabaseName("idx_workout_movements_definition_id");

            entity.HasIndex(e => new { e.WorkoutId, e.SequenceOrder })
                .HasDatabaseName("idx_workout_movements_workout_order");

            // EMOM minute range index
            entity.HasIndex(e => new { e.WorkoutId, e.MinuteStart, e.MinuteEnd })
                .HasDatabaseName("idx_workout_movements_minute_range")
                .HasFilter("minute_start IS NOT NULL");
        });

        // Configure BenchmarkMovementMapping entity
        modelBuilder.Entity<BenchmarkMovementMapping>(entity =>
        {
            entity.ToTable("benchmark_movement_mappings");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign keys
            entity.Property(e => e.BenchmarkDefinitionId)
                .HasColumnName("benchmark_definition_id")
                .IsRequired();

            entity.Property(e => e.MovementDefinitionId)
                .HasColumnName("movement_definition_id")
                .IsRequired();

            // Mapping data
            entity.Property(e => e.RelevanceFactor)
                .HasColumnName("relevance_factor")
                .HasPrecision(3, 2)
                .HasDefaultValue(1.0m)
                .IsRequired();

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.BenchmarkDefinition)
                .WithMany(bd => bd.MovementMappings)
                .HasForeignKey(e => e.BenchmarkDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MovementDefinition)
                .WithMany(md => md.BenchmarkMappings)
                .HasForeignKey(e => e.MovementDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.BenchmarkDefinitionId)
                .HasDatabaseName("idx_benchmark_movement_mappings_benchmark_id");

            entity.HasIndex(e => e.MovementDefinitionId)
                .HasDatabaseName("idx_benchmark_movement_mappings_movement_id");

            // Unique constraint
            entity.HasIndex(e => new { e.BenchmarkDefinitionId, e.MovementDefinitionId })
                .HasDatabaseName("uq_benchmark_movement_mappings_benchmark_movement")
                .IsUnique();
        });

        // Configure PopulationBenchmarkPercentile entity
        modelBuilder.Entity<PopulationBenchmarkPercentile>(entity =>
        {
            entity.ToTable("population_benchmark_percentiles");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // Foreign key
            entity.Property(e => e.BenchmarkDefinitionId)
                .HasColumnName("benchmark_definition_id")
                .IsRequired();

            // Percentile data
            entity.Property(e => e.Percentile20)
                .HasColumnName("percentile_20")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.Percentile40)
                .HasColumnName("percentile_40")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.Percentile60)
                .HasColumnName("percentile_60")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.Percentile80)
                .HasColumnName("percentile_80")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.Percentile95)
                .HasColumnName("percentile_95")
                .HasPrecision(10, 2)
                .IsRequired();

            // Segmentation
            entity.Property(e => e.Gender)
                .HasColumnName("gender")
                .HasMaxLength(20);

            entity.Property(e => e.ExperienceLevel)
                .HasColumnName("experience_level");

            // Audit fields
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.BenchmarkDefinition)
                .WithMany(bd => bd.PopulationPercentiles)
                .HasForeignKey(e => e.BenchmarkDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.BenchmarkDefinitionId)
                .HasDatabaseName("idx_population_benchmark_percentiles_benchmark_id");

            entity.HasIndex(e => e.Gender)
                .HasDatabaseName("idx_population_benchmark_percentiles_gender")
                .HasFilter("gender IS NOT NULL");

            entity.HasIndex(e => e.ExperienceLevel)
                .HasDatabaseName("idx_population_benchmark_percentiles_experience_level")
                .HasFilter("experience_level IS NOT NULL");

            entity.HasIndex(e => new { e.BenchmarkDefinitionId, e.Gender, e.ExperienceLevel })
                .HasDatabaseName("idx_population_benchmark_percentiles_segment");

            // Unique constraint per segmentation
            entity.HasIndex(e => new { e.BenchmarkDefinitionId, e.Gender, e.ExperienceLevel })
                .HasDatabaseName("uq_population_benchmark_percentiles_segment")
                .IsUnique();
        });
    }
}
