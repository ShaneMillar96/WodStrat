-- =============================================================================
-- Migration: V2__create_athlete_profile_tables.sql
-- Description: Creates athlete profile tables with supporting ENUM types
-- Author: database-executor agent
-- Ticket: WOD-1
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Types
-- -----------------------------------------------------------------------------

-- Experience level enum for categorizing athlete fitness background
CREATE TYPE experience_level AS ENUM (
    'beginner',      -- Less than 1 year of functional fitness
    'intermediate',  -- 1-3 years of experience
    'advanced'       -- 3+ years, competition experience
);

COMMENT ON TYPE experience_level IS 'Athlete experience level in functional fitness';

-- Athlete goal enum for primary training focus
CREATE TYPE athlete_goal AS ENUM (
    'improve_pacing',       -- Better workout pacing and consistency
    'prepare_for_open',     -- CrossFit Open preparation
    'competition_prep',     -- General competition preparation
    'build_strength',       -- Focus on strength development
    'improve_conditioning', -- Cardio/engine development
    'weight_management',    -- Body composition goals
    'general_fitness'       -- Overall health and fitness
);

COMMENT ON TYPE athlete_goal IS 'Primary training goal for the athlete';

-- -----------------------------------------------------------------------------
-- Tables
-- -----------------------------------------------------------------------------

-- Athletes table: Core athlete profile information
CREATE TABLE athletes (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Future relationship to users table (authentication Phase 2+)
    user_id UUID NULL,

    -- Profile information
    name VARCHAR(100) NOT NULL,
    date_of_birth DATE NULL,
    gender VARCHAR(20) NULL,

    -- Physical measurements (metric units)
    height_cm DECIMAL(5,2) NULL,  -- Height in centimeters (e.g., 175.50)
    weight_kg DECIMAL(5,2) NULL,  -- Weight in kilograms (e.g., 82.30)

    -- Training profile
    experience_level experience_level NOT NULL DEFAULT 'intermediate',
    primary_goal athlete_goal NOT NULL DEFAULT 'improve_pacing',

    -- Soft delete support
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT chk_athletes_height_positive CHECK (height_cm IS NULL OR height_cm > 0),
    CONSTRAINT chk_athletes_weight_positive CHECK (weight_kg IS NULL OR weight_kg > 0),
    CONSTRAINT chk_athletes_gender_valid CHECK (
        gender IS NULL OR
        gender IN ('Male', 'Female', 'Other', 'Prefer not to say')
    )
);

-- Table comment
COMMENT ON TABLE athletes IS 'Core athlete profile information for WodStrat users';

-- Column comments
COMMENT ON COLUMN athletes.id IS 'Unique identifier for the athlete';
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

-- -----------------------------------------------------------------------------
-- Indexes
-- -----------------------------------------------------------------------------

-- Index for future user lookups (will be used with FK)
CREATE INDEX idx_athletes_user_id ON athletes(user_id) WHERE user_id IS NOT NULL;

-- Index for filtering by experience level
CREATE INDEX idx_athletes_experience_level ON athletes(experience_level);

-- Index for soft delete queries (most queries will filter on is_deleted = FALSE)
CREATE INDEX idx_athletes_is_deleted ON athletes(is_deleted) WHERE is_deleted = FALSE;

-- Composite index for common query pattern: active athletes by experience
CREATE INDEX idx_athletes_active_experience ON athletes(experience_level, is_deleted)
    WHERE is_deleted = FALSE;
