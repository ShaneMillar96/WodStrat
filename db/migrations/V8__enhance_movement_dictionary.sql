-- =============================================================================
-- Migration: V8__enhance_movement_dictionary.sql
-- Description: Enhances movement dictionary tables with additional fields for
--              equipment, load units, bodyweight indicators, and normalized aliases
-- Author: database-executor agent
-- Ticket: WOD-12
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Type Enhancement
-- -----------------------------------------------------------------------------

-- Add 'accessory' to movement_category enum
ALTER TYPE movement_category ADD VALUE IF NOT EXISTS 'accessory';

COMMENT ON TYPE movement_category IS 'Category classification for movement types including weightlifting, gymnastics, cardio, strongman, and accessory';

-- -----------------------------------------------------------------------------
-- Table Alterations: movement_definitions
-- -----------------------------------------------------------------------------

-- Add equipment array column (e.g., ["pull_up_bar", "rings"])
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS equipment VARCHAR(100)[] NULL;

COMMENT ON COLUMN movement_definitions.equipment IS 'Array of required equipment (e.g., ["pull_up_bar", "rings"])';

-- Add default load unit column
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS default_load_unit load_unit NULL;

COMMENT ON COLUMN movement_definitions.default_load_unit IS 'Default unit for weight-based movements (kg/lb/pood)';

-- Add bodyweight indicator
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS is_bodyweight BOOLEAN NOT NULL DEFAULT FALSE;

COMMENT ON COLUMN movement_definitions.is_bodyweight IS 'Whether this is a bodyweight movement (no external load required)';

-- Add RX weights indicator
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS has_rx_weights BOOLEAN NOT NULL DEFAULT FALSE;

COMMENT ON COLUMN movement_definitions.has_rx_weights IS 'Whether this movement has standard RX weights in CrossFit';

-- Add scaling options as JSONB
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS scaling_options JSONB NULL;

COMMENT ON COLUMN movement_definitions.scaling_options IS 'JSON object containing scaling options (e.g., {"beginner": "ring_row", "intermediate": "banded_pull_up"})';

-- Add updated_at timestamp
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW();

COMMENT ON COLUMN movement_definitions.updated_at IS 'Last update timestamp';

-- Add soft delete flag
ALTER TABLE movement_definitions
ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE;

COMMENT ON COLUMN movement_definitions.is_deleted IS 'Soft delete flag (true = deleted)';

-- -----------------------------------------------------------------------------
-- Table Alterations: movement_aliases
-- -----------------------------------------------------------------------------

-- Add normalized alias column for case-insensitive matching
ALTER TABLE movement_aliases
ADD COLUMN IF NOT EXISTS alias_normalized VARCHAR(100) NULL;

COMMENT ON COLUMN movement_aliases.alias_normalized IS 'Lowercase, no special characters version of alias for matching';

-- Populate alias_normalized from existing aliases
UPDATE movement_aliases
SET alias_normalized = LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g'))
WHERE alias_normalized IS NULL;

-- Remove duplicate normalized aliases (keep the one with the lowest id)
-- This handles cases like "chest to bar" and "chest-to-bar" which both normalize to "chesttobar"
DELETE FROM movement_aliases a
USING movement_aliases b
WHERE a.alias_normalized = b.alias_normalized
  AND a.id > b.id;

-- Make alias_normalized NOT NULL after population
ALTER TABLE movement_aliases
ALTER COLUMN alias_normalized SET NOT NULL;

-- Add unique constraint on alias_normalized
ALTER TABLE movement_aliases
ADD CONSTRAINT uq_movement_aliases_alias_normalized UNIQUE (alias_normalized);

-- -----------------------------------------------------------------------------
-- Additional Indexes
-- -----------------------------------------------------------------------------

-- Index for soft delete on movement_definitions
CREATE INDEX IF NOT EXISTS idx_movement_definitions_is_deleted
ON movement_definitions(is_deleted)
WHERE is_deleted = FALSE;

-- Index for bodyweight movements
CREATE INDEX IF NOT EXISTS idx_movement_definitions_is_bodyweight
ON movement_definitions(is_bodyweight)
WHERE is_bodyweight = TRUE;

-- Index for has_rx_weights
CREATE INDEX IF NOT EXISTS idx_movement_definitions_has_rx_weights
ON movement_definitions(has_rx_weights)
WHERE has_rx_weights = TRUE;

-- Index on alias_normalized for fast lookups
CREATE INDEX IF NOT EXISTS idx_movement_aliases_alias_normalized
ON movement_aliases(alias_normalized);

-- GIN index for equipment array searches
CREATE INDEX IF NOT EXISTS idx_movement_definitions_equipment
ON movement_definitions USING GIN (equipment);

-- -----------------------------------------------------------------------------
-- Seed Data Updates: movement_definitions
-- -----------------------------------------------------------------------------

-- Update Weightlifting movements with equipment and flags
UPDATE movement_definitions SET
    equipment = ARRAY['barbell', 'squat_rack'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'back_squat';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell', 'squat_rack'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'front_squat';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'overhead_squat';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "102kg", "rx_female": "70kg"}'::jsonb
WHERE canonical_name = 'deadlift';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "34kg", "rx_female": "25kg"}'::jsonb
WHERE canonical_name = 'sumo_deadlift_high_pull';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "61kg", "rx_female": "43kg"}'::jsonb
WHERE canonical_name = 'clean';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE
WHERE canonical_name = 'power_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'squat_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'hang_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'hang_power_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'jerk';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'push_jerk';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'split_jerk';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "61kg", "rx_female": "43kg"}'::jsonb
WHERE canonical_name = 'clean_and_jerk';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "61kg", "rx_female": "43kg"}'::jsonb
WHERE canonical_name = 'snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE
WHERE canonical_name = 'power_snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'squat_snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'hang_snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'hang_power_snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "43kg", "rx_female": "29kg"}'::jsonb
WHERE canonical_name = 'thruster';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'cluster';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'push_press';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'strict_press';

UPDATE movement_definitions SET
    equipment = ARRAY['barbell', 'bench'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'bench_press';

UPDATE movement_definitions SET
    equipment = ARRAY['kettlebell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "24kg", "rx_female": "16kg", "american": true, "russian": true}'::jsonb
WHERE canonical_name = 'kettlebell_swing';

UPDATE movement_definitions SET
    equipment = ARRAY['dumbbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "22.5kg", "rx_female": "15kg"}'::jsonb
WHERE canonical_name = 'dumbbell_snatch';

UPDATE movement_definitions SET
    equipment = ARRAY['dumbbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'dumbbell_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['dumbbell'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "22.5kg", "rx_female": "15kg"}'::jsonb
WHERE canonical_name = 'dumbbell_thruster';

UPDATE movement_definitions SET
    equipment = ARRAY['wall_ball'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "9kg/3m", "rx_female": "6kg/2.7m"}'::jsonb
WHERE canonical_name = 'wall_ball';

UPDATE movement_definitions SET
    equipment = ARRAY['medicine_ball'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'medicine_ball_clean';

-- Update Gymnastics movements with equipment and flags
UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "ring_row", "intermediate": "banded_pull_up"}'::jsonb
WHERE canonical_name = 'pull_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "jumping_pull_up", "intermediate": "strict_pull_up"}'::jsonb
WHERE canonical_name = 'kipping_pull_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "ring_row", "intermediate": "banded_strict_pull_up"}'::jsonb
WHERE canonical_name = 'strict_pull_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'butterfly_pull_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "pull_up", "intermediate": "jumping_chest_to_bar"}'::jsonb
WHERE canonical_name = 'chest_to_bar_pull_up';

UPDATE movement_definitions SET
    equipment = ARRAY['rings'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "pull_up_and_dip", "intermediate": "banded_muscle_up"}'::jsonb
WHERE canonical_name = 'muscle_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "chest_to_bar", "intermediate": "jumping_bar_muscle_up"}'::jsonb
WHERE canonical_name = 'bar_muscle_up';

UPDATE movement_definitions SET
    equipment = ARRAY['rings'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "pull_up_and_ring_dip", "intermediate": "banded_ring_muscle_up"}'::jsonb
WHERE canonical_name = 'ring_muscle_up';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "hanging_knee_raise", "intermediate": "knees_to_elbows"}'::jsonb
WHERE canonical_name = 'toes_to_bar';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "hanging_knee_raise"}'::jsonb
WHERE canonical_name = 'knees_to_elbows';

UPDATE movement_definitions SET
    equipment = ARRAY['wall'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "pike_push_up", "intermediate": "box_hspu"}'::jsonb
WHERE canonical_name = 'handstand_push_up';

UPDATE movement_definitions SET
    equipment = ARRAY['wall'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "pike_push_up", "intermediate": "deficit_pike_push_up"}'::jsonb
WHERE canonical_name = 'strict_handstand_push_up';

UPDATE movement_definitions SET
    equipment = ARRAY['wall'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'kipping_handstand_push_up';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'handstand_walk';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "knee_push_up", "intermediate": "hand_release_push_up"}'::jsonb
WHERE canonical_name = 'push_up';

UPDATE movement_definitions SET
    equipment = ARRAY['rings'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "box_dip", "intermediate": "banded_ring_dip"}'::jsonb
WHERE canonical_name = 'ring_dip';

UPDATE movement_definitions SET
    equipment = ARRAY['parallel_bars'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "box_dip", "intermediate": "bench_dip"}'::jsonb
WHERE canonical_name = 'bar_dip';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'air_squat';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "box_assisted_pistol", "intermediate": "pistol_to_box"}'::jsonb
WHERE canonical_name = 'pistol';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'lunge';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'walking_lunge';

UPDATE movement_definitions SET
    equipment = ARRAY['box'],
    is_bodyweight = TRUE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "24in", "rx_female": "20in", "beginner": "step_up"}'::jsonb
WHERE canonical_name = 'box_jump';

UPDATE movement_definitions SET
    equipment = ARRAY['box'],
    is_bodyweight = TRUE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "24in", "rx_female": "20in", "beginner": "box_step_over"}'::jsonb
WHERE canonical_name = 'box_jump_over';

UPDATE movement_definitions SET
    equipment = ARRAY['box'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'step_up';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'burpee';

UPDATE movement_definitions SET
    equipment = ARRAY['box'],
    is_bodyweight = TRUE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_male": "24in", "rx_female": "20in"}'::jsonb
WHERE canonical_name = 'burpee_box_jump_over';

UPDATE movement_definitions SET
    equipment = ARRAY['pull_up_bar'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'burpee_pull_up';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "abmat_sit_up"}'::jsonb
WHERE canonical_name = 'sit_up';

UPDATE movement_definitions SET
    equipment = ARRAY['ghd_machine'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "abmat_sit_up", "intermediate": "partial_ghd"}'::jsonb
WHERE canonical_name = 'ghd_sit_up';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'v_up';

UPDATE movement_definitions SET
    equipment = ARRAY['jump_rope'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE,
    scaling_options = '{"beginner": "single_under", "intermediate": "double_under_attempts"}'::jsonb
WHERE canonical_name = 'double_under';

UPDATE movement_definitions SET
    equipment = ARRAY['jump_rope'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'single_under';

UPDATE movement_definitions SET
    equipment = ARRAY['rope'],
    is_bodyweight = TRUE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_height": "15ft", "beginner": "rope_pull", "intermediate": "rope_climb_with_legs"}'::jsonb
WHERE canonical_name = 'rope_climb';

UPDATE movement_definitions SET
    equipment = ARRAY['rope'],
    is_bodyweight = TRUE,
    has_rx_weights = TRUE,
    scaling_options = '{"rx_height": "15ft", "beginner": "rope_climb"}'::jsonb
WHERE canonical_name = 'legless_rope_climb';

UPDATE movement_definitions SET
    equipment = ARRAY['rings'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'ring_row';

UPDATE movement_definitions SET
    equipment = ARRAY['parallettes'],
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'l_sit';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'plank';

-- Update Cardio movements with equipment
UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'run';

UPDATE movement_definitions SET
    equipment = ARRAY['rower'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'row';

UPDATE movement_definitions SET
    equipment = ARRAY['bike'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'bike';

UPDATE movement_definitions SET
    equipment = ARRAY['assault_bike'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'assault_bike';

UPDATE movement_definitions SET
    equipment = ARRAY['echo_bike'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'echo_bike';

UPDATE movement_definitions SET
    equipment = ARRAY['ski_erg'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'ski_erg';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'swim';

UPDATE movement_definitions SET
    equipment = NULL,
    is_bodyweight = TRUE,
    has_rx_weights = FALSE
WHERE canonical_name = 'shuttle_run';

-- Update Strongman movements with equipment
UPDATE movement_definitions SET
    equipment = ARRAY['farmers_handles', 'dumbbells', 'kettlebells'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'farmers_carry';

UPDATE movement_definitions SET
    equipment = ARRAY['yoke'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'yoke_carry';

UPDATE movement_definitions SET
    equipment = ARRAY['sandbag'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'sandbag_carry';

UPDATE movement_definitions SET
    equipment = ARRAY['atlas_stone'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'atlas_stone';

UPDATE movement_definitions SET
    equipment = ARRAY['tire'],
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'tire_flip';

UPDATE movement_definitions SET
    equipment = ARRAY['sled'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'sled_push';

UPDATE movement_definitions SET
    equipment = ARRAY['sled'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'sled_pull';

UPDATE movement_definitions SET
    equipment = ARRAY['sandbag'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'sandbag_clean';

UPDATE movement_definitions SET
    equipment = ARRAY['stone'],
    default_load_unit = 'kg',
    is_bodyweight = FALSE,
    has_rx_weights = FALSE
WHERE canonical_name = 'stone_to_shoulder';

-- -----------------------------------------------------------------------------
-- Additional Movement Aliases (Expanding Coverage)
-- Using a CTE approach to skip aliases that conflict on either alias or alias_normalized
-- -----------------------------------------------------------------------------

-- Additional Back Squat aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bs'), ('back squats')) AS v(alias)
) a
WHERE md.canonical_name = 'back_squat'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Additional Front Squat aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('fs'), ('front squats')) AS v(alias)
) a
WHERE md.canonical_name = 'front_squat'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Additional Overhead Squat aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('ohs'), ('overhead squats'), ('oh squat'), ('oh squats')) AS v(alias)
) a
WHERE md.canonical_name = 'overhead_squat'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('cln'), ('cleans'), ('full clean')) AS v(alias)
) a
WHERE md.canonical_name = 'clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Hang Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('hc'), ('hang cleans'), ('hang cln')) AS v(alias)
) a
WHERE md.canonical_name = 'hang_clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Hang Power Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('hpc'), ('hang pwr clean'), ('hang power cleans')) AS v(alias)
) a
WHERE md.canonical_name = 'hang_power_clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Snatch aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sn'), ('snatches'), ('full snatch')) AS v(alias)
) a
WHERE md.canonical_name = 'snatch'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Hang Snatch aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('hang snatches'), ('hang sn')) AS v(alias)
) a
WHERE md.canonical_name = 'hang_snatch'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Hang Power Snatch aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('hps'), ('hang pwr snatch'), ('hang power snatches')) AS v(alias)
) a
WHERE md.canonical_name = 'hang_power_snatch'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Jerk aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('j'), ('jerks')) AS v(alias)
) a
WHERE md.canonical_name = 'jerk'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Push Jerk aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('pj'), ('push jerks')) AS v(alias)
) a
WHERE md.canonical_name = 'push_jerk'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Split Jerk aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sj'), ('split jerks')) AS v(alias)
) a
WHERE md.canonical_name = 'split_jerk'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Cluster aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('clusters'), ('squat clean thruster')) AS v(alias)
) a
WHERE md.canonical_name = 'cluster'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Bench Press aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bp'), ('bench'), ('bench presses')) AS v(alias)
) a
WHERE md.canonical_name = 'bench_press'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Dumbbell Snatch aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('db snatch'), ('dbsn'), ('db snatches'), ('dumbbell snatches')) AS v(alias)
) a
WHERE md.canonical_name = 'dumbbell_snatch'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Dumbbell Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('db clean'), ('dbcln'), ('db cleans'), ('dumbbell cleans')) AS v(alias)
) a
WHERE md.canonical_name = 'dumbbell_clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Dumbbell Thruster aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('db thruster'), ('db thrusters'), ('dumbbell thrusters')) AS v(alias)
) a
WHERE md.canonical_name = 'dumbbell_thruster'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Medicine Ball Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('mb clean'), ('med ball clean'), ('medicine ball cleans')) AS v(alias)
) a
WHERE md.canonical_name = 'medicine_ball_clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Strict Pull-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('strict pu'), ('strict pull ups')) AS v(alias)
) a
WHERE md.canonical_name = 'strict_pull_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Butterfly Pull-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('butterfly pu'), ('butterfly pull ups')) AS v(alias)
) a
WHERE md.canonical_name = 'butterfly_pull_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Strict HSPU aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('strict hspu'), ('strict handstand push up')) AS v(alias)
) a
WHERE md.canonical_name = 'strict_handstand_push_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Kipping HSPU aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('kipping hspu'), ('kipping handstand push up')) AS v(alias)
) a
WHERE md.canonical_name = 'kipping_handstand_push_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Handstand Walk aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('hs walk'), ('hsw'), ('handstand walks'), ('hs walks')) AS v(alias)
) a
WHERE md.canonical_name = 'handstand_walk'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Ring Dip aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('rd'), ('ring dips')) AS v(alias)
) a
WHERE md.canonical_name = 'ring_dip'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Bar Dip aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bd'), ('bar dips'), ('dip'), ('dips'), ('parallel bar dip')) AS v(alias)
) a
WHERE md.canonical_name = 'bar_dip'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Lunge aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('lunges'), ('forward lunge'), ('reverse lunge')) AS v(alias)
) a
WHERE md.canonical_name = 'lunge'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Walking Lunge aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('wl'), ('walking lunges'), ('walk lunge'), ('walk lunges')) AS v(alias)
) a
WHERE md.canonical_name = 'walking_lunge'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Box Jump Over aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bjo'), ('box jump overs'), ('box jumps over')) AS v(alias)
) a
WHERE md.canonical_name = 'box_jump_over'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Step-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('step ups'), ('box step up'), ('box step ups')) AS v(alias)
) a
WHERE md.canonical_name = 'step_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Burpee Box Jump Over aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bbjo'), ('burpee bjo'), ('burpee box jump overs')) AS v(alias)
) a
WHERE md.canonical_name = 'burpee_box_jump_over'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Burpee Pull-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('bpu'), ('burpee pu'), ('burpee pull ups')) AS v(alias)
) a
WHERE md.canonical_name = 'burpee_pull_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- V-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT DISTINCT ON (a.normalized) md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('v ups'), ('vup')) AS v(alias)
) a
WHERE md.canonical_name = 'v_up'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Single-under aliases (note: 'su' conflicts with sit_up in V7)
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('singles'), ('single unders')) AS v(alias)
) a
WHERE md.canonical_name = 'single_under'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Legless Rope Climb aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('lrc'), ('legless rc'), ('legless rope climbs'), ('legless')) AS v(alias)
) a
WHERE md.canonical_name = 'legless_rope_climb'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Ring Row aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('rr'), ('ring rows'), ('inverted row'), ('inverted rows')) AS v(alias)
) a
WHERE md.canonical_name = 'ring_row'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- L-sit aliases (note: 'l sit' and 'lsit' normalize to same value)
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT DISTINCT ON (a.normalized) md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('l sit'), ('l sits')) AS v(alias)
) a
WHERE md.canonical_name = 'l_sit'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Plank aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('planks'), ('plank hold'), ('front plank')) AS v(alias)
) a
WHERE md.canonical_name = 'plank'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Bike aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('biking'), ('cycle'), ('cycling'), ('bike erg')) AS v(alias)
) a
WHERE md.canonical_name = 'bike'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Echo Bike aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('echo'), ('rogue echo'), ('rogue bike')) AS v(alias)
) a
WHERE md.canonical_name = 'echo_bike'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Ski Erg aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('ski'), ('skierg'), ('c2 ski'), ('concept2 ski')) AS v(alias)
) a
WHERE md.canonical_name = 'ski_erg'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Swim aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('swimming'), ('freestyle'), ('swim laps')) AS v(alias)
) a
WHERE md.canonical_name = 'swim'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Shuttle Run aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('shuttle runs'), ('shuttle'), ('shuttles')) AS v(alias)
) a
WHERE md.canonical_name = 'shuttle_run'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Farmers Carry aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('fc'), ('farmers walk'), ('farmers carries'), ('farmer carry'), ('farmer walk')) AS v(alias)
) a
WHERE md.canonical_name = 'farmers_carry'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Yoke Carry aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('yoke'), ('yoke walk'), ('yoke carries')) AS v(alias)
) a
WHERE md.canonical_name = 'yoke_carry'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Sandbag Carry aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sb carry'), ('sandbag carries'), ('sand bag carry')) AS v(alias)
) a
WHERE md.canonical_name = 'sandbag_carry'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Atlas Stone aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('atlas stones'), ('stone lift'), ('atlas'), ('stone')) AS v(alias)
) a
WHERE md.canonical_name = 'atlas_stone'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Tire Flip aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('tire flips'), ('tyre flip'), ('tyre flips')) AS v(alias)
) a
WHERE md.canonical_name = 'tire_flip'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Sled Push aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sled pushes'), ('prowler push'), ('prowler')) AS v(alias)
) a
WHERE md.canonical_name = 'sled_push'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Sled Pull aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sled pulls'), ('sled drag'), ('drag sled')) AS v(alias)
) a
WHERE md.canonical_name = 'sled_pull'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Sandbag Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('sb clean'), ('sandbag cleans'), ('sand bag clean')) AS v(alias)
) a
WHERE md.canonical_name = 'sandbag_clean'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);

-- Stone to Shoulder aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES ('stone shoulder'), ('stone to shoulders'), ('shoulder stone')) AS v(alias)
) a
WHERE md.canonical_name = 'stone_to_shoulder'
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias = a.alias)
  AND NOT EXISTS (SELECT 1 FROM movement_aliases ma WHERE ma.alias_normalized = a.normalized);
