-- =============================================================================
-- Migration: V11__add_workout_time_references.sql
-- Description: Creates workout_time_references table for storing reference
--              completion times for known workouts at various percentile levels
-- Author: database-executor agent
-- Ticket: WOD-21
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Tables
-- -----------------------------------------------------------------------------

-- Workout time references table
CREATE TABLE workout_time_references (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Workout identification
    workout_name VARCHAR(100) NOT NULL,

    -- Percentile times (stored in seconds)
    -- Note: For time-based workouts, LOWER time = BETTER performance
    -- So 20th percentile (slower) > 95th percentile (faster)
    percentile_20_seconds INTEGER NOT NULL,
    percentile_40_seconds INTEGER NOT NULL,
    percentile_60_seconds INTEGER NOT NULL,
    percentile_80_seconds INTEGER NOT NULL,
    percentile_95_seconds INTEGER NOT NULL,

    -- Segmentation (nullable for "all athletes" baseline)
    gender VARCHAR(20) NULL,
    experience_level experience_level NULL,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Unique constraint per segmentation
    CONSTRAINT uq_workout_time_references_name_segment
        UNIQUE (workout_name, gender, experience_level),

    -- Gender validation
    CONSTRAINT chk_workout_time_references_gender_valid
        CHECK (gender IS NULL OR gender IN ('Male', 'Female')),

    -- Ensure percentiles are in proper order (20th is slowest, 95th is fastest)
    CONSTRAINT chk_workout_time_references_percentiles_ascending
        CHECK (percentile_20_seconds >= percentile_40_seconds
           AND percentile_40_seconds >= percentile_60_seconds
           AND percentile_60_seconds >= percentile_80_seconds
           AND percentile_80_seconds >= percentile_95_seconds)
);

-- Table comment
COMMENT ON TABLE workout_time_references IS 'Reference completion times for known workouts at various population percentile levels';

-- Column comments
COMMENT ON COLUMN workout_time_references.id IS 'Unique auto-incrementing identifier';
COMMENT ON COLUMN workout_time_references.workout_name IS 'Name of the workout (e.g., Fran, Grace, Helen)';
COMMENT ON COLUMN workout_time_references.percentile_20_seconds IS 'Completion time at 20th percentile (slower end) in seconds';
COMMENT ON COLUMN workout_time_references.percentile_40_seconds IS 'Completion time at 40th percentile in seconds';
COMMENT ON COLUMN workout_time_references.percentile_60_seconds IS 'Completion time at 60th percentile in seconds';
COMMENT ON COLUMN workout_time_references.percentile_80_seconds IS 'Completion time at 80th percentile in seconds';
COMMENT ON COLUMN workout_time_references.percentile_95_seconds IS 'Completion time at 95th percentile (elite) in seconds';
COMMENT ON COLUMN workout_time_references.gender IS 'Gender filter for segmented data (NULL for all)';
COMMENT ON COLUMN workout_time_references.experience_level IS 'Experience level filter (NULL for all)';
COMMENT ON COLUMN workout_time_references.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN workout_time_references.updated_at IS 'Last update timestamp';

-- -----------------------------------------------------------------------------
-- Indexes
-- -----------------------------------------------------------------------------

-- Index for workout name lookups
CREATE INDEX idx_workout_time_references_workout_name
    ON workout_time_references(workout_name);

-- Index for gender-based filtering
CREATE INDEX idx_workout_time_references_gender
    ON workout_time_references(gender)
    WHERE gender IS NOT NULL;

-- Index for experience level filtering
CREATE INDEX idx_workout_time_references_experience_level
    ON workout_time_references(experience_level)
    WHERE experience_level IS NOT NULL;

-- Composite index for segment lookups
CREATE INDEX idx_workout_time_references_segment
    ON workout_time_references(workout_name, gender, experience_level);

-- -----------------------------------------------------------------------------
-- Seed Data: Hero/Girl WOD Reference Times
-- Note: Times are in seconds. Values provided are for "all genders/levels" baseline
-- Time format in comments: MM:SS
-- -----------------------------------------------------------------------------

-- Fran (21-15-9 Thrusters @ 95/65 lb + Pull-ups)
-- | 20th %ile | 40th %ile | 60th %ile | 80th %ile | 95th %ile |
-- | 8:00      | 5:30      | 4:00      | 3:00      | 2:15      |
INSERT INTO workout_time_references
    (workout_name, percentile_20_seconds, percentile_40_seconds, percentile_60_seconds, percentile_80_seconds, percentile_95_seconds, gender, experience_level)
VALUES
    ('Fran', 480, 330, 240, 180, 135, NULL, NULL);

-- Grace (30 Clean & Jerks @ 135/95 lb for time)
-- | 20th %ile | 40th %ile | 60th %ile | 80th %ile | 95th %ile |
-- | 5:00      | 3:30      | 2:30      | 1:45      | 1:15      |
INSERT INTO workout_time_references
    (workout_name, percentile_20_seconds, percentile_40_seconds, percentile_60_seconds, percentile_80_seconds, percentile_95_seconds, gender, experience_level)
VALUES
    ('Grace', 300, 210, 150, 105, 75, NULL, NULL);

-- Helen (3 rounds: 400m Run, 21 KB Swings @ 53/35 lb, 12 Pull-ups)
-- | 20th %ile | 40th %ile | 60th %ile | 80th %ile | 95th %ile |
-- | 14:00     | 11:00     | 9:00      | 7:30      | 6:00      |
INSERT INTO workout_time_references
    (workout_name, percentile_20_seconds, percentile_40_seconds, percentile_60_seconds, percentile_80_seconds, percentile_95_seconds, gender, experience_level)
VALUES
    ('Helen', 840, 660, 540, 450, 360, NULL, NULL);

-- Diane (21-15-9 Deadlifts @ 225/155 lb + Handstand Push-ups)
-- | 20th %ile | 40th %ile | 60th %ile | 80th %ile | 95th %ile |
-- | 8:00      | 5:30      | 4:00      | 3:00      | 2:00      |
INSERT INTO workout_time_references
    (workout_name, percentile_20_seconds, percentile_40_seconds, percentile_60_seconds, percentile_80_seconds, percentile_95_seconds, gender, experience_level)
VALUES
    ('Diane', 480, 330, 240, 180, 120, NULL, NULL);

-- Isabel (30 Snatches @ 135/95 lb for time)
-- | 20th %ile | 40th %ile | 60th %ile | 80th %ile | 95th %ile |
-- | 6:00      | 4:00      | 2:45      | 2:00      | 1:20      |
INSERT INTO workout_time_references
    (workout_name, percentile_20_seconds, percentile_40_seconds, percentile_60_seconds, percentile_80_seconds, percentile_95_seconds, gender, experience_level)
VALUES
    ('Isabel', 360, 240, 165, 120, 80, NULL, NULL);
