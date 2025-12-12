-- =============================================================================
-- Migration: V4__seed_benchmark_definitions.sql
-- Description: Seeds predefined benchmark definitions (23 total)
-- Author: database-executor agent
-- Ticket: WOD-3
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Cardio Benchmarks (5)
-- -----------------------------------------------------------------------------

INSERT INTO benchmark_definitions (name, slug, description, category, metric_type, unit, display_order) VALUES
    ('500m Row', '500m-row', 'Sprint row distance for time', 'cardio', 'time', 'seconds', 1),
    ('2k Row', '2k-row', 'Standard 2000 meter rowing test', 'cardio', 'time', 'seconds', 2),
    ('1 Mile Run', '1-mile-run', 'Running benchmark for 1 mile distance', 'cardio', 'time', 'seconds', 3),
    ('5k Run', '5k-run', 'Endurance running benchmark for 5 kilometers', 'cardio', 'time', 'seconds', 4),
    ('1k Row Pace', '1k-row-pace', 'Average pace per 500m for 1000m row', 'cardio', 'pace', 'sec/500m', 5);

-- -----------------------------------------------------------------------------
-- Strength Benchmarks (8)
-- -----------------------------------------------------------------------------

INSERT INTO benchmark_definitions (name, slug, description, category, metric_type, unit, display_order) VALUES
    ('Back Squat 1RM', 'back-squat-1rm', 'One rep max back squat', 'strength', 'weight', 'kg', 1),
    ('Front Squat 1RM', 'front-squat-1rm', 'One rep max front squat', 'strength', 'weight', 'kg', 2),
    ('Deadlift 1RM', 'deadlift-1rm', 'One rep max deadlift', 'strength', 'weight', 'kg', 3),
    ('Clean 1RM', 'clean-1rm', 'One rep max clean (squat or power)', 'strength', 'weight', 'kg', 4),
    ('Clean & Jerk 1RM', 'clean-and-jerk-1rm', 'One rep max clean and jerk', 'strength', 'weight', 'kg', 5),
    ('Snatch 1RM', 'snatch-1rm', 'One rep max snatch (squat or power)', 'strength', 'weight', 'kg', 6),
    ('Bench Press 1RM', 'bench-press-1rm', 'One rep max bench press', 'strength', 'weight', 'kg', 7),
    ('Strict Press 1RM', 'strict-press-1rm', 'One rep max strict press (shoulder press)', 'strength', 'weight', 'kg', 8);

-- -----------------------------------------------------------------------------
-- Gymnastics Benchmarks (5)
-- -----------------------------------------------------------------------------

INSERT INTO benchmark_definitions (name, slug, description, category, metric_type, unit, display_order) VALUES
    ('Max Unbroken Pull-ups', 'max-unbroken-pull-ups', 'Maximum consecutive pull-ups without dropping', 'gymnastics', 'reps', 'reps', 1),
    ('Max Unbroken Toes-to-Bar', 'max-unbroken-toes-to-bar', 'Maximum consecutive toes-to-bar without dropping', 'gymnastics', 'reps', 'reps', 2),
    ('Max Unbroken Muscle-ups', 'max-unbroken-muscle-ups', 'Maximum consecutive muscle-ups (ring or bar)', 'gymnastics', 'reps', 'reps', 3),
    ('Max Unbroken Double-unders', 'max-unbroken-double-unders', 'Maximum consecutive double-unders', 'gymnastics', 'reps', 'reps', 4),
    ('Max Unbroken HSPU', 'max-unbroken-hspu', 'Maximum consecutive handstand push-ups', 'gymnastics', 'reps', 'reps', 5);

-- -----------------------------------------------------------------------------
-- Hero/Girl WOD Benchmarks (5)
-- -----------------------------------------------------------------------------

INSERT INTO benchmark_definitions (name, slug, description, category, metric_type, unit, display_order) VALUES
    ('Fran', 'fran', '21-15-9 thrusters (95/65 lb) and pull-ups for time', 'hero_wod', 'time', 'seconds', 1),
    ('Grace', 'grace', '30 clean and jerks (135/95 lb) for time', 'hero_wod', 'time', 'seconds', 2),
    ('Helen', 'helen', '3 rounds: 400m run, 21 KB swings (53/35 lb), 12 pull-ups', 'hero_wod', 'time', 'seconds', 3),
    ('Diane', 'diane', '21-15-9 deadlifts (225/155 lb) and handstand push-ups', 'hero_wod', 'time', 'seconds', 4),
    ('Isabel', 'isabel', '30 snatches (135/95 lb) for time', 'hero_wod', 'time', 'seconds', 5);
