-- =============================================================================
-- Migration: V5__convert_guids_to_ints.sql
-- Description: Converts all primary keys and foreign keys from UUID to SERIAL INT
-- Author: database-executor agent
-- Ticket: WOD-8
-- =============================================================================

-- -----------------------------------------------------------------------------
-- STEP 1: Create new tables with INT primary keys
-- -----------------------------------------------------------------------------

-- New benchmark_definitions table with INT primary key
CREATE TABLE benchmark_definitions_new (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Benchmark identification
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,

    -- Classification
    category benchmark_category NOT NULL,
    metric_type benchmark_metric_type NOT NULL,
    unit VARCHAR(50) NOT NULL,

    -- Status and ordering
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    display_order INTEGER NOT NULL DEFAULT 0,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT uq_benchmark_definitions_new_name UNIQUE (name),
    CONSTRAINT uq_benchmark_definitions_new_slug UNIQUE (slug)
);

-- New athletes table with INT primary key
CREATE TABLE athletes_new (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Future relationship to users table (changed from UUID to INT for consistency)
    user_id INTEGER NULL,

    -- Profile information
    name VARCHAR(100) NOT NULL,
    date_of_birth DATE NULL,
    gender VARCHAR(20) NULL,

    -- Physical measurements (metric units)
    height_cm DECIMAL(5,2) NULL,
    weight_kg DECIMAL(5,2) NULL,

    -- Training profile
    experience_level experience_level NOT NULL DEFAULT 'intermediate',
    primary_goal athlete_goal NOT NULL DEFAULT 'improve_pacing',

    -- Soft delete support
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT chk_athletes_new_height_positive CHECK (height_cm IS NULL OR height_cm > 0),
    CONSTRAINT chk_athletes_new_weight_positive CHECK (weight_kg IS NULL OR weight_kg > 0),
    CONSTRAINT chk_athletes_new_gender_valid CHECK (
        gender IS NULL OR
        gender IN ('Male', 'Female', 'Other', 'Prefer not to say')
    )
);

-- New athlete_benchmarks table with INT primary key and foreign keys
CREATE TABLE athlete_benchmarks_new (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Foreign keys (now INT)
    athlete_id INTEGER NOT NULL,
    benchmark_definition_id INTEGER NOT NULL,

    -- Benchmark data
    value DECIMAL(10,2) NOT NULL,
    recorded_at DATE NOT NULL DEFAULT CURRENT_DATE,
    notes VARCHAR(500) NULL,

    -- Soft delete
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- -----------------------------------------------------------------------------
-- STEP 2: Create temporary mapping tables to track old UUID -> new INT mappings
-- -----------------------------------------------------------------------------

CREATE TEMP TABLE benchmark_definition_mapping (
    old_id UUID,
    new_id INTEGER
);

CREATE TEMP TABLE athlete_mapping (
    old_id UUID,
    new_id INTEGER
);

-- -----------------------------------------------------------------------------
-- STEP 3: Migrate data from old tables to new tables
-- -----------------------------------------------------------------------------

-- Migrate benchmark_definitions (preserving display_order for consistent ordering)
INSERT INTO benchmark_definitions_new (name, slug, description, category, metric_type, unit, is_active, display_order, created_at)
SELECT name, slug, description, category, metric_type, unit, is_active, display_order, created_at
FROM benchmark_definitions
ORDER BY category, display_order, created_at;

-- Create mapping for benchmark_definitions
INSERT INTO benchmark_definition_mapping (old_id, new_id)
SELECT old.id, new.id
FROM benchmark_definitions old
JOIN benchmark_definitions_new new ON old.slug = new.slug;

-- Migrate athletes
INSERT INTO athletes_new (user_id, name, date_of_birth, gender, height_cm, weight_kg, experience_level, primary_goal, is_deleted, created_at, updated_at)
SELECT NULL, name, date_of_birth, gender, height_cm, weight_kg, experience_level, primary_goal, is_deleted, created_at, updated_at
FROM athletes
ORDER BY created_at;

-- Create mapping for athletes (using created_at and name as composite key for mapping)
INSERT INTO athlete_mapping (old_id, new_id)
SELECT old.id, new.id
FROM athletes old
JOIN athletes_new new ON old.name = new.name AND old.created_at = new.created_at;

-- Migrate athlete_benchmarks with mapped foreign keys
INSERT INTO athlete_benchmarks_new (athlete_id, benchmark_definition_id, value, recorded_at, notes, is_deleted, created_at, updated_at)
SELECT
    am.new_id,
    bdm.new_id,
    ab.value,
    ab.recorded_at,
    ab.notes,
    ab.is_deleted,
    ab.created_at,
    ab.updated_at
FROM athlete_benchmarks ab
JOIN athlete_mapping am ON ab.athlete_id = am.old_id
JOIN benchmark_definition_mapping bdm ON ab.benchmark_definition_id = bdm.old_id;

-- -----------------------------------------------------------------------------
-- STEP 4: Drop old tables
-- -----------------------------------------------------------------------------

DROP TABLE athlete_benchmarks;
DROP TABLE athletes;
DROP TABLE benchmark_definitions;

-- -----------------------------------------------------------------------------
-- STEP 5: Rename new tables to original names
-- -----------------------------------------------------------------------------

ALTER TABLE benchmark_definitions_new RENAME TO benchmark_definitions;
ALTER TABLE athletes_new RENAME TO athletes;
ALTER TABLE athlete_benchmarks_new RENAME TO athlete_benchmarks;

-- -----------------------------------------------------------------------------
-- STEP 6: Rename constraints to match original naming convention
-- -----------------------------------------------------------------------------

ALTER TABLE benchmark_definitions RENAME CONSTRAINT uq_benchmark_definitions_new_name TO uq_benchmark_definitions_name;
ALTER TABLE benchmark_definitions RENAME CONSTRAINT uq_benchmark_definitions_new_slug TO uq_benchmark_definitions_slug;
ALTER TABLE athletes RENAME CONSTRAINT chk_athletes_new_height_positive TO chk_athletes_height_positive;
ALTER TABLE athletes RENAME CONSTRAINT chk_athletes_new_weight_positive TO chk_athletes_weight_positive;
ALTER TABLE athletes RENAME CONSTRAINT chk_athletes_new_gender_valid TO chk_athletes_gender_valid;

-- -----------------------------------------------------------------------------
-- STEP 7: Add foreign key constraints to athlete_benchmarks
-- -----------------------------------------------------------------------------

ALTER TABLE athlete_benchmarks
    ADD CONSTRAINT fk_athlete_benchmarks_athlete
    FOREIGN KEY (athlete_id) REFERENCES athletes(id)
    ON DELETE CASCADE;

ALTER TABLE athlete_benchmarks
    ADD CONSTRAINT fk_athlete_benchmarks_definition
    FOREIGN KEY (benchmark_definition_id) REFERENCES benchmark_definitions(id)
    ON DELETE RESTRICT;

-- Unique constraint: one result per benchmark per athlete
ALTER TABLE athlete_benchmarks
    ADD CONSTRAINT uq_athlete_benchmarks_athlete_definition
    UNIQUE (athlete_id, benchmark_definition_id);

-- -----------------------------------------------------------------------------
-- STEP 8: Recreate all indexes
-- -----------------------------------------------------------------------------

-- benchmark_definitions indexes
CREATE INDEX idx_benchmark_definitions_category ON benchmark_definitions(category);
CREATE INDEX idx_benchmark_definitions_is_active ON benchmark_definitions(is_active)
    WHERE is_active = TRUE;

-- athletes indexes
CREATE INDEX idx_athletes_user_id ON athletes(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX idx_athletes_experience_level ON athletes(experience_level);
CREATE INDEX idx_athletes_is_deleted ON athletes(is_deleted) WHERE is_deleted = FALSE;
CREATE INDEX idx_athletes_active_experience ON athletes(experience_level, is_deleted)
    WHERE is_deleted = FALSE;

-- athlete_benchmarks indexes
CREATE INDEX idx_athlete_benchmarks_athlete_id ON athlete_benchmarks(athlete_id);
CREATE INDEX idx_athlete_benchmarks_definition_id ON athlete_benchmarks(benchmark_definition_id);
CREATE INDEX idx_athlete_benchmarks_is_deleted ON athlete_benchmarks(is_deleted)
    WHERE is_deleted = FALSE;
CREATE INDEX idx_athlete_benchmarks_athlete_active ON athlete_benchmarks(athlete_id, is_deleted)
    WHERE is_deleted = FALSE;

-- -----------------------------------------------------------------------------
-- STEP 9: Update table and column comments
-- -----------------------------------------------------------------------------

-- Table comments
COMMENT ON TABLE athletes IS 'Core athlete profile information for WodStrat users';
COMMENT ON TABLE benchmark_definitions IS 'Predefined benchmark types for athlete performance tracking';
COMMENT ON TABLE athlete_benchmarks IS 'Athlete benchmark results linking athletes to their performance metrics';

-- athletes column comments
COMMENT ON COLUMN athletes.id IS 'Unique auto-incrementing identifier for the athlete';
COMMENT ON COLUMN athletes.user_id IS 'Future FK to users table (authentication Phase 2+)';
COMMENT ON COLUMN athletes.name IS 'Athlete display name';
COMMENT ON COLUMN athletes.date_of_birth IS 'Date of birth for age calculation';
COMMENT ON COLUMN athletes.gender IS 'Gender identity (Male/Female/Other/Prefer not to say)';
COMMENT ON COLUMN athletes.height_cm IS 'Height in centimeters';
COMMENT ON COLUMN athletes.weight_kg IS 'Weight in kilograms';
COMMENT ON COLUMN athletes.experience_level IS 'Functional fitness experience level';
COMMENT ON COLUMN athletes.primary_goal IS 'Primary training goal';
COMMENT ON COLUMN athletes.is_deleted IS 'Soft delete flag (true = deleted)';
COMMENT ON COLUMN athletes.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN athletes.updated_at IS 'Last update timestamp';

-- benchmark_definitions column comments
COMMENT ON COLUMN benchmark_definitions.id IS 'Unique auto-incrementing identifier for the benchmark definition';
COMMENT ON COLUMN benchmark_definitions.name IS 'Display name of the benchmark (e.g., "Fran", "Back Squat 1RM")';
COMMENT ON COLUMN benchmark_definitions.slug IS 'URL-friendly identifier (e.g., "fran", "back-squat-1rm")';
COMMENT ON COLUMN benchmark_definitions.description IS 'Detailed description of the benchmark';
COMMENT ON COLUMN benchmark_definitions.category IS 'Benchmark category (cardio/strength/gymnastics/hero_wod)';
COMMENT ON COLUMN benchmark_definitions.metric_type IS 'How results are measured (time/reps/weight/pace)';
COMMENT ON COLUMN benchmark_definitions.unit IS 'Display unit for the metric (e.g., "seconds", "kg", "reps")';
COMMENT ON COLUMN benchmark_definitions.is_active IS 'Whether this benchmark is currently available for use';
COMMENT ON COLUMN benchmark_definitions.display_order IS 'Sort order within category for UI display';
COMMENT ON COLUMN benchmark_definitions.created_at IS 'Record creation timestamp';

-- athlete_benchmarks column comments
COMMENT ON COLUMN athlete_benchmarks.id IS 'Unique auto-incrementing identifier for the athlete benchmark record';
COMMENT ON COLUMN athlete_benchmarks.athlete_id IS 'Reference to the athlete who recorded this benchmark';
COMMENT ON COLUMN athlete_benchmarks.benchmark_definition_id IS 'Reference to the benchmark type';
COMMENT ON COLUMN athlete_benchmarks.value IS 'The benchmark result value (interpretation depends on metric_type)';
COMMENT ON COLUMN athlete_benchmarks.recorded_at IS 'Date when this benchmark was achieved';
COMMENT ON COLUMN athlete_benchmarks.notes IS 'Optional notes (e.g., "RX", "scaled", equipment used)';
COMMENT ON COLUMN athlete_benchmarks.is_deleted IS 'Soft delete flag (true = deleted)';
COMMENT ON COLUMN athlete_benchmarks.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN athlete_benchmarks.updated_at IS 'Last update timestamp';
