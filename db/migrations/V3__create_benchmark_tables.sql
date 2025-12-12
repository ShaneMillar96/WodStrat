-- =============================================================================
-- Migration: V3__create_benchmark_tables.sql
-- Description: Creates benchmark definition and athlete benchmark tables with ENUM types
-- Author: database-executor agent
-- Ticket: WOD-3
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Types
-- -----------------------------------------------------------------------------

-- Benchmark category enum for classifying benchmark types
CREATE TYPE benchmark_category AS ENUM (
    'cardio',       -- Cardiovascular/endurance benchmarks
    'strength',     -- Strength-based benchmarks (lifts)
    'gymnastics',   -- Bodyweight/gymnastics movements
    'hero_wod'      -- Hero and Girl benchmark workouts
);

COMMENT ON TYPE benchmark_category IS 'Category classification for benchmark types';

-- Benchmark metric type enum for specifying how results are measured
CREATE TYPE benchmark_metric_type AS ENUM (
    'time',    -- Duration in seconds (lower is better)
    'reps',    -- Count of repetitions (higher is better)
    'weight',  -- Weight in kilograms (higher is better)
    'pace'     -- Pace in seconds per unit (lower is better)
);

COMMENT ON TYPE benchmark_metric_type IS 'Metric type defining how benchmark results are measured';

-- -----------------------------------------------------------------------------
-- Tables
-- -----------------------------------------------------------------------------

-- Benchmark definitions table: Predefined benchmark types
CREATE TABLE benchmark_definitions (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

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

    -- Audit fields (no updated_at - reference data rarely changes)
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT uq_benchmark_definitions_name UNIQUE (name),
    CONSTRAINT uq_benchmark_definitions_slug UNIQUE (slug)
);

-- Table comments
COMMENT ON TABLE benchmark_definitions IS 'Predefined benchmark types for athlete performance tracking';

-- Column comments
COMMENT ON COLUMN benchmark_definitions.id IS 'Unique identifier for the benchmark definition';
COMMENT ON COLUMN benchmark_definitions.name IS 'Display name of the benchmark (e.g., "Fran", "Back Squat 1RM")';
COMMENT ON COLUMN benchmark_definitions.slug IS 'URL-friendly identifier (e.g., "fran", "back-squat-1rm")';
COMMENT ON COLUMN benchmark_definitions.description IS 'Detailed description of the benchmark';
COMMENT ON COLUMN benchmark_definitions.category IS 'Benchmark category (cardio/strength/gymnastics/hero_wod)';
COMMENT ON COLUMN benchmark_definitions.metric_type IS 'How results are measured (time/reps/weight/pace)';
COMMENT ON COLUMN benchmark_definitions.unit IS 'Display unit for the metric (e.g., "seconds", "kg", "reps")';
COMMENT ON COLUMN benchmark_definitions.is_active IS 'Whether this benchmark is currently available for use';
COMMENT ON COLUMN benchmark_definitions.display_order IS 'Sort order within category for UI display';
COMMENT ON COLUMN benchmark_definitions.created_at IS 'Record creation timestamp';

-- Athlete benchmarks table: Stores athlete's benchmark results
CREATE TABLE athlete_benchmarks (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Foreign keys
    athlete_id UUID NOT NULL,
    benchmark_definition_id UUID NOT NULL,

    -- Benchmark data
    value DECIMAL(10,2) NOT NULL,
    recorded_at DATE NOT NULL DEFAULT CURRENT_DATE,
    notes VARCHAR(500) NULL,

    -- Soft delete
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraints
    CONSTRAINT fk_athlete_benchmarks_athlete
        FOREIGN KEY (athlete_id) REFERENCES athletes(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_athlete_benchmarks_definition
        FOREIGN KEY (benchmark_definition_id) REFERENCES benchmark_definitions(id)
        ON DELETE RESTRICT,

    -- Unique constraint: one result per benchmark per athlete
    CONSTRAINT uq_athlete_benchmarks_athlete_definition
        UNIQUE (athlete_id, benchmark_definition_id)
);

-- Table comments
COMMENT ON TABLE athlete_benchmarks IS 'Athlete benchmark results linking athletes to their performance metrics';

-- Column comments
COMMENT ON COLUMN athlete_benchmarks.id IS 'Unique identifier for the athlete benchmark record';
COMMENT ON COLUMN athlete_benchmarks.athlete_id IS 'Reference to the athlete who recorded this benchmark';
COMMENT ON COLUMN athlete_benchmarks.benchmark_definition_id IS 'Reference to the benchmark type';
COMMENT ON COLUMN athlete_benchmarks.value IS 'The benchmark result value (interpretation depends on metric_type)';
COMMENT ON COLUMN athlete_benchmarks.recorded_at IS 'Date when this benchmark was achieved';
COMMENT ON COLUMN athlete_benchmarks.notes IS 'Optional notes (e.g., "RX", "scaled", equipment used)';
COMMENT ON COLUMN athlete_benchmarks.is_deleted IS 'Soft delete flag (true = deleted)';
COMMENT ON COLUMN athlete_benchmarks.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN athlete_benchmarks.updated_at IS 'Last update timestamp';

-- -----------------------------------------------------------------------------
-- Indexes
-- -----------------------------------------------------------------------------

-- benchmark_definitions indexes
CREATE INDEX idx_benchmark_definitions_category ON benchmark_definitions(category);
CREATE INDEX idx_benchmark_definitions_is_active ON benchmark_definitions(is_active)
    WHERE is_active = TRUE;

-- athlete_benchmarks indexes
CREATE INDEX idx_athlete_benchmarks_athlete_id ON athlete_benchmarks(athlete_id);
CREATE INDEX idx_athlete_benchmarks_definition_id ON athlete_benchmarks(benchmark_definition_id);
CREATE INDEX idx_athlete_benchmarks_is_deleted ON athlete_benchmarks(is_deleted)
    WHERE is_deleted = FALSE;
CREATE INDEX idx_athlete_benchmarks_athlete_active ON athlete_benchmarks(athlete_id, is_deleted)
    WHERE is_deleted = FALSE;
