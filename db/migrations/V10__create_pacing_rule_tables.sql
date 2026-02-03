-- =============================================================================
-- Migration: V10__create_pacing_rule_tables.sql
-- Description: Creates tables for pacing recommendation system including
--              benchmark-movement mappings and population percentile data
-- Author: database-executor agent
-- Ticket: WOD-18
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Types
-- -----------------------------------------------------------------------------

-- Pacing level enum for workout intensity recommendations
CREATE TYPE pacing_level AS ENUM (
    'light',     -- Athlete should pace conservatively (below 40th percentile)
    'moderate',  -- Athlete can maintain moderate pace (40th-60th percentile)
    'heavy'      -- Athlete can push harder (above 60th percentile)
);

COMMENT ON TYPE pacing_level IS 'Recommended pacing intensity level for workout movements based on athlete benchmark performance';

-- -----------------------------------------------------------------------------
-- Tables
-- -----------------------------------------------------------------------------

-- Benchmark to movement mappings table
CREATE TABLE benchmark_movement_mappings (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Foreign keys
    benchmark_definition_id INTEGER NOT NULL,
    movement_definition_id INTEGER NOT NULL,

    -- Mapping data
    relevance_factor DECIMAL(3,2) NOT NULL DEFAULT 1.0,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraints
    CONSTRAINT fk_benchmark_movement_mappings_benchmark
        FOREIGN KEY (benchmark_definition_id) REFERENCES benchmark_definitions(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_benchmark_movement_mappings_movement
        FOREIGN KEY (movement_definition_id) REFERENCES movement_definitions(id)
        ON DELETE CASCADE,

    -- Constraints
    CONSTRAINT uq_benchmark_movement_mappings_benchmark_movement
        UNIQUE (benchmark_definition_id, movement_definition_id),
    CONSTRAINT chk_benchmark_movement_mappings_relevance_range
        CHECK (relevance_factor >= 0 AND relevance_factor <= 1)
);

-- Table comment
COMMENT ON TABLE benchmark_movement_mappings IS 'Maps benchmarks to movements they indicate strength in for pacing calculations';

-- Column comments
COMMENT ON COLUMN benchmark_movement_mappings.id IS 'Unique auto-incrementing identifier';
COMMENT ON COLUMN benchmark_movement_mappings.benchmark_definition_id IS 'Reference to the benchmark definition';
COMMENT ON COLUMN benchmark_movement_mappings.movement_definition_id IS 'Reference to the movement definition';
COMMENT ON COLUMN benchmark_movement_mappings.relevance_factor IS 'How strongly the benchmark predicts movement performance (0.0-1.0)';
COMMENT ON COLUMN benchmark_movement_mappings.created_at IS 'Record creation timestamp';

-- Population benchmark percentiles table
CREATE TABLE population_benchmark_percentiles (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Foreign key
    benchmark_definition_id INTEGER NOT NULL,

    -- Percentile data (values at each percentile threshold)
    percentile_20 DECIMAL(10,2) NOT NULL,
    percentile_40 DECIMAL(10,2) NOT NULL,
    percentile_60 DECIMAL(10,2) NOT NULL,
    percentile_80 DECIMAL(10,2) NOT NULL,
    percentile_95 DECIMAL(10,2) NOT NULL,

    -- Segmentation (nullable for "all athletes" baseline)
    gender VARCHAR(20) NULL,
    experience_level experience_level NULL,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraint
    CONSTRAINT fk_population_benchmark_percentiles_benchmark
        FOREIGN KEY (benchmark_definition_id) REFERENCES benchmark_definitions(id)
        ON DELETE CASCADE,

    -- Unique constraint per segmentation
    CONSTRAINT uq_population_benchmark_percentiles_segment
        UNIQUE (benchmark_definition_id, gender, experience_level),

    -- Gender validation
    CONSTRAINT chk_population_benchmark_percentiles_gender_valid
        CHECK (gender IS NULL OR gender IN ('Male', 'Female'))
);

-- Table comment
COMMENT ON TABLE population_benchmark_percentiles IS 'Population reference data for benchmark percentile calculations';

-- Column comments
COMMENT ON COLUMN population_benchmark_percentiles.id IS 'Unique auto-incrementing identifier';
COMMENT ON COLUMN population_benchmark_percentiles.benchmark_definition_id IS 'Reference to the benchmark definition';
COMMENT ON COLUMN population_benchmark_percentiles.percentile_20 IS 'Value at 20th percentile';
COMMENT ON COLUMN population_benchmark_percentiles.percentile_40 IS 'Value at 40th percentile';
COMMENT ON COLUMN population_benchmark_percentiles.percentile_60 IS 'Value at 60th percentile';
COMMENT ON COLUMN population_benchmark_percentiles.percentile_80 IS 'Value at 80th percentile';
COMMENT ON COLUMN population_benchmark_percentiles.percentile_95 IS 'Value at 95th percentile';
COMMENT ON COLUMN population_benchmark_percentiles.gender IS 'Gender filter for segmented percentiles (NULL for all)';
COMMENT ON COLUMN population_benchmark_percentiles.experience_level IS 'Experience level filter for segmented percentiles (NULL for all)';
COMMENT ON COLUMN population_benchmark_percentiles.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN population_benchmark_percentiles.updated_at IS 'Last update timestamp';

-- -----------------------------------------------------------------------------
-- Indexes
-- -----------------------------------------------------------------------------

-- benchmark_movement_mappings indexes
CREATE INDEX idx_benchmark_movement_mappings_benchmark_id
    ON benchmark_movement_mappings(benchmark_definition_id);
CREATE INDEX idx_benchmark_movement_mappings_movement_id
    ON benchmark_movement_mappings(movement_definition_id);

-- population_benchmark_percentiles indexes
CREATE INDEX idx_population_benchmark_percentiles_benchmark_id
    ON population_benchmark_percentiles(benchmark_definition_id);
CREATE INDEX idx_population_benchmark_percentiles_gender
    ON population_benchmark_percentiles(gender)
    WHERE gender IS NOT NULL;
CREATE INDEX idx_population_benchmark_percentiles_experience_level
    ON population_benchmark_percentiles(experience_level)
    WHERE experience_level IS NOT NULL;
CREATE INDEX idx_population_benchmark_percentiles_segment
    ON population_benchmark_percentiles(benchmark_definition_id, gender, experience_level);

-- -----------------------------------------------------------------------------
-- Seed Data: Benchmark to Movement Mappings
-- -----------------------------------------------------------------------------

-- Back Squat 1RM -> Squat-based movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'back-squat-1rm'
  AND md.canonical_name IN ('thruster', 'wall_ball', 'air_squat', 'front_squat', 'overhead_squat', 'back_squat');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'back-squat-1rm'
  AND md.canonical_name IN ('pistol', 'box_jump', 'walking_lunge', 'lunge');

-- Front Squat 1RM -> Front-loaded squat movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'front-squat-1rm'
  AND md.canonical_name IN ('thruster', 'squat_clean', 'wall_ball', 'front_squat', 'cluster');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'front-squat-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'hang_clean', 'hang_power_clean');

-- Deadlift 1RM -> Hip hinge movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('deadlift', 'sumo_deadlift_high_pull');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'kettlebell_swing');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('snatch', 'power_snatch');

-- Clean 1RM -> Clean variations
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'squat_clean', 'hang_clean', 'hang_power_clean');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-1rm'
  AND md.canonical_name IN ('clean_and_jerk', 'cluster', 'dumbbell_clean');

-- Clean & Jerk 1RM -> Clean and jerk movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-and-jerk-1rm'
  AND md.canonical_name IN ('clean_and_jerk', 'clean', 'jerk', 'push_jerk', 'split_jerk');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-and-jerk-1rm'
  AND md.canonical_name IN ('power_clean', 'squat_clean', 'thruster');

-- Snatch 1RM -> Snatch variations
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'snatch-1rm'
  AND md.canonical_name IN ('snatch', 'power_snatch', 'squat_snatch', 'hang_snatch', 'hang_power_snatch');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'snatch-1rm'
  AND md.canonical_name IN ('dumbbell_snatch', 'overhead_squat');

-- Bench Press 1RM -> Pressing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'bench-press-1rm'
  AND md.canonical_name IN ('bench_press', 'push_up');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'bench-press-1rm'
  AND md.canonical_name IN ('bar_dip', 'ring_dip');

-- Strict Press 1RM -> Overhead pressing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('strict_press', 'push_press', 'push_jerk', 'jerk');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('thruster', 'dumbbell_thruster');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('handstand_push_up', 'strict_handstand_push_up', 'kipping_handstand_push_up');

-- Max Pull-ups -> Pulling movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('pull_up', 'kipping_pull_up', 'strict_pull_up', 'butterfly_pull_up', 'chest_to_bar_pull_up');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('muscle_up', 'bar_muscle_up', 'ring_muscle_up');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('rope_climb', 'legless_rope_climb', 'ring_row');

-- Max Toes-to-Bar -> Core/hanging movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-toes-to-bar'
  AND md.canonical_name IN ('toes_to_bar', 'knees_to_elbows');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-toes-to-bar'
  AND md.canonical_name IN ('v_up', 'sit_up', 'ghd_sit_up');

-- Max Muscle-ups -> High-skill gymnastics
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-muscle-ups'
  AND md.canonical_name IN ('muscle_up', 'bar_muscle_up', 'ring_muscle_up');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-muscle-ups'
  AND md.canonical_name IN ('pull_up', 'chest_to_bar_pull_up', 'ring_dip');

-- Max Double-unders -> Jump rope movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-double-unders'
  AND md.canonical_name IN ('double_under', 'single_under');

-- Max HSPU -> Inverted pressing
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-hspu'
  AND md.canonical_name IN ('handstand_push_up', 'strict_handstand_push_up', 'kipping_handstand_push_up');

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-hspu'
  AND md.canonical_name IN ('handstand_walk', 'push_up');

-- 500m Row -> Rowing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '500m-row'
  AND md.canonical_name = 'row';

-- 2k Row -> Rowing (endurance)
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '2k-row'
  AND md.canonical_name = 'row';

-- 1 Mile Run -> Running movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '1-mile-run'
  AND md.canonical_name IN ('run', 'shuttle_run');

-- 5k Run -> Running (endurance)
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '5k-run'
  AND md.canonical_name IN ('run', 'shuttle_run');

-- 1k Row Pace -> Rowing
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '1k-row-pace'
  AND md.canonical_name = 'row';

-- -----------------------------------------------------------------------------
-- Seed Data: Population Benchmark Percentiles
-- Note: Values are in the same unit as the benchmark (kg for weight, seconds for time, reps for reps)
-- For time-based benchmarks, LOWER is better. For weight/reps, HIGHER is better.
-- -----------------------------------------------------------------------------

-- STRENGTH BENCHMARKS (weight in kg)

-- Back Squat 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 70, 90, 110, 135, 170, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'back-squat-1rm';

-- Back Squat 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 40, 52, 65, 80, 100, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'back-squat-1rm';

-- Front Squat 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 55, 75, 90, 110, 140, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'front-squat-1rm';

-- Front Squat 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 32, 43, 55, 68, 85, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'front-squat-1rm';

-- Deadlift 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 100, 130, 160, 190, 230, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'deadlift-1rm';

-- Deadlift 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 55, 72, 90, 110, 140, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'deadlift-1rm';

-- Clean 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 55, 72, 90, 110, 135, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'clean-1rm';

-- Clean 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 32, 43, 55, 68, 85, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'clean-1rm';

-- Clean & Jerk 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 50, 67, 85, 105, 130, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'clean-and-jerk-1rm';

-- Clean & Jerk 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 30, 40, 52, 65, 82, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'clean-and-jerk-1rm';

-- Snatch 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 40, 55, 70, 88, 110, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'snatch-1rm';

-- Snatch 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 25, 34, 43, 55, 70, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'snatch-1rm';

-- Bench Press 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 55, 72, 90, 110, 135, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'bench-press-1rm';

-- Bench Press 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 27, 36, 45, 56, 70, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'bench-press-1rm';

-- Strict Press 1RM - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 38, 50, 62, 77, 95, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'strict-press-1rm';

-- Strict Press 1RM - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 20, 27, 34, 43, 55, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'strict-press-1rm';

-- GYMNASTICS BENCHMARKS (reps)

-- Max Pull-ups - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 5, 12, 20, 30, 45, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-pull-ups';

-- Max Pull-ups - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 1, 5, 10, 18, 28, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-pull-ups';

-- Max Toes-to-Bar - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 3, 8, 15, 25, 40, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-toes-to-bar';

-- Max Toes-to-Bar - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 1, 5, 10, 18, 30, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-toes-to-bar';

-- Max Muscle-ups - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 0, 1, 4, 8, 15, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-muscle-ups';

-- Max Muscle-ups - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 0, 0, 1, 4, 10, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-muscle-ups';

-- Max Double-unders - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 10, 35, 75, 125, 200, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-double-unders';

-- Max Double-unders - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 8, 30, 65, 110, 180, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-double-unders';

-- Max HSPU - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 1, 5, 12, 22, 35, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-hspu';

-- Max HSPU - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 0, 2, 6, 12, 22, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'max-unbroken-hspu';

-- CARDIO BENCHMARKS (time in seconds - LOWER is better)

-- 500m Row - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 115, 105, 95, 87, 78, 'Male', NULL
FROM benchmark_definitions WHERE slug = '500m-row';

-- 500m Row - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 135, 122, 112, 102, 92, 'Female', NULL
FROM benchmark_definitions WHERE slug = '500m-row';

-- 2k Row - Male (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 540, 480, 435, 400, 365, 'Male', NULL
FROM benchmark_definitions WHERE slug = '2k-row';

-- 2k Row - Female (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 600, 540, 490, 450, 410, 'Female', NULL
FROM benchmark_definitions WHERE slug = '2k-row';

-- 1 Mile Run - Male (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 540, 480, 420, 375, 330, 'Male', NULL
FROM benchmark_definitions WHERE slug = '1-mile-run';

-- 1 Mile Run - Female (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 630, 555, 480, 420, 375, 'Female', NULL
FROM benchmark_definitions WHERE slug = '1-mile-run';

-- 5k Run - Male (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 1680, 1500, 1350, 1200, 1050, 'Male', NULL
FROM benchmark_definitions WHERE slug = '5k-run';

-- 5k Run - Female (in seconds)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 1980, 1740, 1560, 1380, 1200, 'Female', NULL
FROM benchmark_definitions WHERE slug = '5k-run';

-- 1k Row Pace - Male (in seconds per 500m - LOWER is better)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 120, 110, 100, 92, 83, 'Male', NULL
FROM benchmark_definitions WHERE slug = '1k-row-pace';

-- 1k Row Pace - Female (in seconds per 500m - LOWER is better)
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 140, 128, 118, 108, 98, 'Female', NULL
FROM benchmark_definitions WHERE slug = '1k-row-pace';

-- HERO/GIRL WOD BENCHMARKS (time in seconds - LOWER is better)

-- Fran - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 480, 360, 270, 200, 140, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'fran';

-- Fran - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 600, 450, 330, 250, 180, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'fran';

-- Grace - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 360, 270, 195, 145, 100, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'grace';

-- Grace - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 450, 330, 250, 185, 135, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'grace';

-- Helen - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 780, 660, 560, 480, 400, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'helen';

-- Helen - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 900, 780, 660, 560, 470, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'helen';

-- Diane - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 600, 450, 330, 250, 180, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'diane';

-- Diane - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 720, 540, 400, 310, 230, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'diane';

-- Isabel - Male
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 420, 300, 210, 150, 100, 'Male', NULL
FROM benchmark_definitions WHERE slug = 'isabel';

-- Isabel - Female
INSERT INTO population_benchmark_percentiles
    (benchmark_definition_id, percentile_20, percentile_40, percentile_60, percentile_80, percentile_95, gender, experience_level)
SELECT id, 540, 390, 285, 210, 150, 'Female', NULL
FROM benchmark_definitions WHERE slug = 'isabel';
