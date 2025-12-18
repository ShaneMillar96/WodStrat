-- =============================================================================
-- Migration: V9__add_workout_parsing_fields.sql
-- Description: Adds fields required for workout parsing utility (WOD-14)
--              Including tabata workout type, rep scheme support, and parse confidence
-- Author: database-executor agent
-- Ticket: WOD-14
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Type Modifications
-- -----------------------------------------------------------------------------

-- Add 'tabata' to workout_type enum
ALTER TYPE workout_type ADD VALUE IF NOT EXISTS 'tabata';

COMMENT ON TYPE workout_type IS 'Classification of workout structure/format including tabata protocol';

-- Create rep_scheme_type enum for rep scheme patterns
CREATE TYPE rep_scheme_type AS ENUM (
    'fixed',       -- Same reps each round (e.g., 10-10-10)
    'descending',  -- Decreasing reps (e.g., 21-15-9)
    'ascending',   -- Increasing reps (e.g., 9-15-21)
    'custom'       -- Custom pattern (e.g., 1-2-3-4-5-4-3-2-1)
);

COMMENT ON TYPE rep_scheme_type IS 'Pattern type for workout rep schemes';

-- -----------------------------------------------------------------------------
-- Table Alterations: workouts
-- -----------------------------------------------------------------------------

-- Add parse_confidence column for parser confidence scoring
ALTER TABLE workouts
ADD COLUMN IF NOT EXISTS parse_confidence DECIMAL(3,2) NULL;

COMMENT ON COLUMN workouts.parse_confidence IS 'Parser confidence score (0.00-1.00) indicating reliability of parsing';

-- Add rep_scheme_type column for rep scheme pattern classification
ALTER TABLE workouts
ADD COLUMN IF NOT EXISTS rep_scheme_type rep_scheme_type NULL;

COMMENT ON COLUMN workouts.rep_scheme_type IS 'Type of rep scheme pattern (fixed/descending/ascending/custom)';

-- Add rep_scheme_reps column as INTEGER array for rep values
ALTER TABLE workouts
ADD COLUMN IF NOT EXISTS rep_scheme_reps INTEGER[] NULL;

COMMENT ON COLUMN workouts.rep_scheme_reps IS 'Array of rep values for the rep scheme (e.g., {21,15,9} for Fran)';

-- Add check constraint for parse_confidence range
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_workouts_parse_confidence_range'
    ) THEN
        ALTER TABLE workouts
        ADD CONSTRAINT chk_workouts_parse_confidence_range
            CHECK (parse_confidence IS NULL OR (parse_confidence >= 0 AND parse_confidence <= 1));
    END IF;
END $$;

-- Note: rep_scheme_reps positive value validation is handled at application level
-- PostgreSQL check constraints cannot use subqueries on arrays

-- -----------------------------------------------------------------------------
-- Table Alterations: workout_movements
-- -----------------------------------------------------------------------------

-- Add minute_start column for EMOM workouts
ALTER TABLE workout_movements
ADD COLUMN IF NOT EXISTS minute_start INTEGER NULL;

COMMENT ON COLUMN workout_movements.minute_start IS 'Starting minute for EMOM workouts (1-indexed)';

-- Add minute_end column for EMOM workouts (for multi-minute movements)
ALTER TABLE workout_movements
ADD COLUMN IF NOT EXISTS minute_end INTEGER NULL;

COMMENT ON COLUMN workout_movements.minute_end IS 'Ending minute for EMOM workouts (for movements spanning multiple minutes)';

-- Add check constraints for minute columns
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_workout_movements_minute_start_positive'
    ) THEN
        ALTER TABLE workout_movements
        ADD CONSTRAINT chk_workout_movements_minute_start_positive
            CHECK (minute_start IS NULL OR minute_start > 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_workout_movements_minute_end_positive'
    ) THEN
        ALTER TABLE workout_movements
        ADD CONSTRAINT chk_workout_movements_minute_end_positive
            CHECK (minute_end IS NULL OR minute_end > 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_workout_movements_minute_range_valid'
    ) THEN
        ALTER TABLE workout_movements
        ADD CONSTRAINT chk_workout_movements_minute_range_valid
            CHECK (minute_start IS NULL OR minute_end IS NULL OR minute_end >= minute_start);
    END IF;
END $$;

-- -----------------------------------------------------------------------------
-- Additional Indexes
-- -----------------------------------------------------------------------------

-- Index for workouts by parse_confidence (useful for quality filtering)
CREATE INDEX IF NOT EXISTS idx_workouts_parse_confidence
ON workouts(parse_confidence DESC)
WHERE parse_confidence IS NOT NULL;

-- Index for workouts by rep_scheme_type
CREATE INDEX IF NOT EXISTS idx_workouts_rep_scheme_type
ON workouts(rep_scheme_type)
WHERE rep_scheme_type IS NOT NULL;

-- GIN index for rep_scheme_reps array searches
CREATE INDEX IF NOT EXISTS idx_workouts_rep_scheme_reps
ON workouts USING GIN (rep_scheme_reps)
WHERE rep_scheme_reps IS NOT NULL;

-- Index for workout_movements by minute range (for EMOM queries)
CREATE INDEX IF NOT EXISTS idx_workout_movements_minute_range
ON workout_movements(workout_id, minute_start, minute_end)
WHERE minute_start IS NOT NULL;
