-- =============================================================================
-- Migration: V12__fix_benchmark_movement_aliases.sql
-- Description: Fixes benchmark-to-movement mapping issues by:
--              1. Adding missing run aliases for distance-prefixed runs
--              2. Adding missing row aliases for distance-prefixed rows
--              3. Re-seeding any missing benchmark-movement mappings (idempotent)
-- Author: database-executor agent
-- Ticket: WOD-33
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Additional Run Aliases (Distance-Prefixed)
-- These aliases ensure "400m run", "1 mile run", etc. map to the "run" movement
-- Uses ON CONFLICT to handle duplicate normalized aliases within the same batch
-- -----------------------------------------------------------------------------

-- Distance-prefixed run aliases
INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES
        ('400m run'),
        ('800m run'),
        ('1 mile run'),
        ('1600m run'),
        ('200m run'),
        ('100m run'),
        ('5k run'),
        ('10k run'),
        ('1k run'),
        ('2k run'),
        ('3k run'),
        ('mile run'),
        ('400 meter run'),
        ('800 meter run'),
        ('1 mile'),
        ('400m'),
        ('800m'),
        ('200m'),
        ('1600m'),
        ('run 400m'),
        ('run 800m'),
        ('run 400'),
        ('run 800'),
        ('run 200'),
        ('run 1 mile'),
        ('run 1600m'),
        ('runs')
    ) AS v(alias)
) a
WHERE md.canonical_name = 'run'
ON CONFLICT (alias_normalized) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Additional Row Aliases (Distance-Prefixed)
-- These aliases ensure "500m row", "1000m row", etc. map to the "row" movement
-- -----------------------------------------------------------------------------

INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES
        ('500m row'),
        ('1000m row'),
        ('1k row'),
        ('2k row'),
        ('250m row'),
        ('row 500m'),
        ('row 1000m'),
        ('row 500'),
        ('row 1000'),
        ('row 1k'),
        ('row 2k'),
        ('500 meter row'),
        ('1000 meter row'),
        ('2000m row'),
        ('2000 meter row'),
        ('rows')
    ) AS v(alias)
) a
WHERE md.canonical_name = 'row'
ON CONFLICT (alias_normalized) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Additional Bike/Assault Bike Aliases (Distance/Calorie-Prefixed)
-- -----------------------------------------------------------------------------

INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES
        ('50 cal bike'),
        ('40 cal bike'),
        ('30 cal bike'),
        ('20 cal bike'),
        ('10 cal bike'),
        ('cal bike'),
        ('calorie bike'),
        ('calories bike'),
        ('bike calories'),
        ('bike cals')
    ) AS v(alias)
) a
WHERE md.canonical_name = 'assault_bike'
ON CONFLICT (alias_normalized) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Additional Pull-up Aliases
-- Ensure common pull-up variations are captured
-- Note: chinup, chin up, chin-up all normalize to "chinup" - ON CONFLICT handles this
-- -----------------------------------------------------------------------------

INSERT INTO movement_aliases (movement_definition_id, alias, alias_normalized)
SELECT md.id, a.alias, a.normalized
FROM movement_definitions md
CROSS JOIN (
    SELECT alias, LOWER(REGEXP_REPLACE(alias, '[^a-zA-Z0-9]', '', 'g')) as normalized
    FROM (VALUES
        ('pull ups'),
        ('pull-up'),
        ('chinup'),
        ('chin up'),
        ('chin-up'),
        ('chin ups')
    ) AS v(alias)
) a
WHERE md.canonical_name = 'pull_up'
ON CONFLICT (alias_normalized) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Re-seed Missing Benchmark-Movement Mappings (Idempotent)
-- Uses ON CONFLICT DO NOTHING to safely re-seed without duplicates
-- -----------------------------------------------------------------------------

-- 5k Run -> Running movements 
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '5k-run'
  AND md.canonical_name IN ('run', 'shuttle_run')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- 1 Mile Run -> Running movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '1-mile-run'
  AND md.canonical_name IN ('run', 'shuttle_run')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Max Pull-ups -> Pulling movements 
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('pull_up', 'kipping_pull_up', 'strict_pull_up', 'butterfly_pull_up', 'chest_to_bar_pull_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('muscle_up', 'bar_muscle_up', 'ring_muscle_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-pull-ups'
  AND md.canonical_name IN ('rope_climb', 'legless_rope_climb', 'ring_row')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- 500m Row -> Rowing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '500m-row'
  AND md.canonical_name = 'row'
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- 2k Row -> Rowing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '2k-row'
  AND md.canonical_name = 'row'
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- 1k Row Pace -> Rowing
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = '1k-row-pace'
  AND md.canonical_name = 'row'
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Back Squat 1RM -> Squat-based movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'back-squat-1rm'
  AND md.canonical_name IN ('thruster', 'wall_ball', 'air_squat', 'front_squat', 'overhead_squat', 'back_squat')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'back-squat-1rm'
  AND md.canonical_name IN ('pistol', 'box_jump', 'walking_lunge', 'lunge')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Front Squat 1RM -> Front-loaded squat movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'front-squat-1rm'
  AND md.canonical_name IN ('thruster', 'squat_clean', 'wall_ball', 'front_squat', 'cluster')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'front-squat-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'hang_clean', 'hang_power_clean')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Deadlift 1RM -> Hip hinge movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('deadlift', 'sumo_deadlift_high_pull')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'kettlebell_swing')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'deadlift-1rm'
  AND md.canonical_name IN ('snatch', 'power_snatch')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Clean 1RM -> Clean variations
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-1rm'
  AND md.canonical_name IN ('clean', 'power_clean', 'squat_clean', 'hang_clean', 'hang_power_clean')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-1rm'
  AND md.canonical_name IN ('clean_and_jerk', 'cluster', 'dumbbell_clean')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Clean & Jerk 1RM -> Clean and jerk movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-and-jerk-1rm'
  AND md.canonical_name IN ('clean_and_jerk', 'clean', 'jerk', 'push_jerk', 'split_jerk')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'clean-and-jerk-1rm'
  AND md.canonical_name IN ('power_clean', 'squat_clean', 'thruster')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Snatch 1RM -> Snatch variations
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'snatch-1rm'
  AND md.canonical_name IN ('snatch', 'power_snatch', 'squat_snatch', 'hang_snatch', 'hang_power_snatch')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'snatch-1rm'
  AND md.canonical_name IN ('dumbbell_snatch', 'overhead_squat')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Bench Press 1RM -> Pressing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'bench-press-1rm'
  AND md.canonical_name IN ('bench_press', 'push_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'bench-press-1rm'
  AND md.canonical_name IN ('bar_dip', 'ring_dip')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Strict Press 1RM -> Overhead pressing movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('strict_press', 'push_press', 'push_jerk', 'jerk')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('thruster', 'dumbbell_thruster')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'strict-press-1rm'
  AND md.canonical_name IN ('handstand_push_up', 'strict_handstand_push_up', 'kipping_handstand_push_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Max Toes-to-Bar -> Core/hanging movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-toes-to-bar'
  AND md.canonical_name IN ('toes_to_bar', 'knees_to_elbows')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-toes-to-bar'
  AND md.canonical_name IN ('v_up', 'sit_up', 'ghd_sit_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Max Muscle-ups -> High-skill gymnastics
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-muscle-ups'
  AND md.canonical_name IN ('muscle_up', 'bar_muscle_up', 'ring_muscle_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.9
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-muscle-ups'
  AND md.canonical_name IN ('pull_up', 'chest_to_bar_pull_up', 'ring_dip')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Max Double-unders -> Jump rope movements
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-double-unders'
  AND md.canonical_name IN ('double_under', 'single_under')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

-- Max HSPU -> Inverted pressing
INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 1.0
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-hspu'
  AND md.canonical_name IN ('handstand_push_up', 'strict_handstand_push_up', 'kipping_handstand_push_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;

INSERT INTO benchmark_movement_mappings (benchmark_definition_id, movement_definition_id, relevance_factor)
SELECT bd.id, md.id, 0.8
FROM benchmark_definitions bd, movement_definitions md
WHERE bd.slug = 'max-unbroken-hspu'
  AND md.canonical_name IN ('handstand_walk', 'push_up')
ON CONFLICT (benchmark_definition_id, movement_definition_id) DO NOTHING;
