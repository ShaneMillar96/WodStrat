-- =============================================================================
-- Migration: V1000__seed_local_test_data.sql
-- Description: Seeds comprehensive test data for local development only.
--              Creates a test user with athlete profile, benchmarks, and workouts.
-- WARNING: This migration is for LOCAL DEVELOPMENT ONLY - do not run in production!
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Test User: localtest@example.com / TestPassword123
-- -----------------------------------------------------------------------------

-- Insert test user (password: TestPassword123)
INSERT INTO users (email, password_hash, is_active, created_at, updated_at)
VALUES (
    'localtest@example.com',
    '$2a$11$/buIaY8ma.9Sl.TfN0ajcOp6u42bHgdpZ2L/5jMmXO6IM7q.YFtXe',
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Get the user ID (handle case where user already exists)
DO $$
DECLARE
    v_user_id INT;
    v_athlete_id INT;
    v_workout_fortime_id INT;
    v_workout_amrap_id INT;
    v_workout_emom_id INT;
BEGIN
    -- Get user ID
    SELECT id INTO v_user_id FROM users WHERE email = 'localtest@example.com';

    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Failed to create or find test user';
    END IF;

    -- -----------------------------------------------------------------------------
    -- Create Athlete Profile
    -- -----------------------------------------------------------------------------
    INSERT INTO athletes (
        user_id, name, date_of_birth, gender, height_cm, weight_kg,
        experience_level, primary_goal, is_deleted, created_at, updated_at
    )
    VALUES (
        v_user_id,
        'Local Test Athlete',
        '1990-05-15',
        'Male',
        180.0,
        82.0,
        'intermediate',
        'improve_pacing',
        false,
        NOW(),
        NOW()
    )
    ON CONFLICT (user_id) DO UPDATE SET
        name = EXCLUDED.name,
        updated_at = NOW()
    RETURNING id INTO v_athlete_id;

    -- -----------------------------------------------------------------------------
    -- Create Athlete Benchmarks (Strength)
    -- -----------------------------------------------------------------------------

    -- Back Squat 1RM (benchmark_definition_id = 6) - 120kg = ~70th percentile for intermediate male
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 6, 120.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Front Squat 1RM (benchmark_definition_id = 7) - 100kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 7, 100.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Deadlift 1RM (benchmark_definition_id = 8) - 170kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 8, 170.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Clean 1RM (benchmark_definition_id = 9) - 95kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 9, 95.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Clean & Jerk 1RM (benchmark_definition_id = 10) - 90kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 10, 90.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Snatch 1RM (benchmark_definition_id = 11) - 70kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 11, 70.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Strict Press 1RM (benchmark_definition_id = 13) - 60kg
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 13, 60.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- -----------------------------------------------------------------------------
    -- Create Athlete Benchmarks (Gymnastics)
    -- -----------------------------------------------------------------------------

    -- Max Unbroken Pull-ups (benchmark_definition_id = 14) - 18 reps
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 14, 18.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- Max Unbroken Toes-to-Bar (benchmark_definition_id = 15) - 12 reps
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 15, 12.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- -----------------------------------------------------------------------------
    -- Create Athlete Benchmarks (Cardio)
    -- -----------------------------------------------------------------------------

    -- 500m Row (benchmark_definition_id = 1) - 95 seconds (1:35)
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 1, 95.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- 2k Row (benchmark_definition_id = 2) - 450 seconds (7:30)
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 2, 450.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- 1 Mile Run (benchmark_definition_id = 3) - 420 seconds (7:00)
    INSERT INTO athlete_benchmarks (athlete_id, benchmark_definition_id, value, recorded_at, is_deleted, created_at, updated_at)
    VALUES (v_athlete_id, 3, 420.0, CURRENT_DATE - INTERVAL '7 days', false, NOW(), NOW())
    ON CONFLICT DO NOTHING;

    -- -----------------------------------------------------------------------------
    -- Create ForTime Workout (Fran-style: 21-15-9 Thrusters & Pull-ups)
    -- -----------------------------------------------------------------------------
    INSERT INTO workouts (
        user_id, name, original_text, parsed_description, workout_type,
        rep_scheme_type, time_cap_seconds, is_deleted, created_at, updated_at
    )
    VALUES (
        v_user_id,
        'Local Test Fran',
        '21-15-9
Thrusters (43kg/30kg)
Pull-ups',
        'ForTime - 21-15-9 Thrusters and Pull-ups',
        'for_time',
        'descending',
        600,  -- 10 minute cap
        false,
        NOW(),
        NOW()
    )
    RETURNING id INTO v_workout_fortime_id;

    -- Add workout movements for ForTime workout
    -- Thrusters (movement_definition_id = 20) - 21 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        load_value, load_unit, created_at
    )
    VALUES (v_workout_fortime_id, 20, 1, 21, 43.0, 'kg', NOW());

    -- Pull-ups (movement_definition_id = 31) - 21 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        created_at
    )
    VALUES (v_workout_fortime_id, 31, 2, 21, NOW());

    -- Thrusters - 15 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        load_value, load_unit, created_at
    )
    VALUES (v_workout_fortime_id, 20, 3, 15, 43.0, 'kg', NOW());

    -- Pull-ups - 15 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        created_at
    )
    VALUES (v_workout_fortime_id, 31, 4, 15, NOW());

    -- Thrusters - 9 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        load_value, load_unit, created_at
    )
    VALUES (v_workout_fortime_id, 20, 5, 9, 43.0, 'kg', NOW());

    -- Pull-ups - 9 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        created_at
    )
    VALUES (v_workout_fortime_id, 31, 6, 9, NOW());

    -- -----------------------------------------------------------------------------
    -- Create AMRAP Workout (12 min AMRAP)
    -- -----------------------------------------------------------------------------
    INSERT INTO workouts (
        user_id, name, original_text, parsed_description, workout_type,
        rep_scheme_type, time_cap_seconds, is_deleted, created_at, updated_at
    )
    VALUES (
        v_user_id,
        'Local Test AMRAP',
        '12 minute AMRAP:
5 Power Cleans (60kg)
10 Push-ups
15 Air Squats',
        'AMRAP 12 - Power Cleans, Push-ups, Air Squats',
        'amrap',
        'fixed',
        720,  -- 12 minutes
        false,
        NOW(),
        NOW()
    )
    RETURNING id INTO v_workout_amrap_id;

    -- Add workout movements for AMRAP workout
    -- Power Cleans (movement_definition_id = 7) - 5 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        load_value, load_unit, created_at
    )
    VALUES (v_workout_amrap_id, 7, 1, 5, 60.0, 'kg', NOW());

    -- Push-ups (movement_definition_id = 45) - 10 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        created_at
    )
    VALUES (v_workout_amrap_id, 45, 2, 10, NOW());

    -- Air Squats (movement_definition_id = 48) - 15 reps
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        created_at
    )
    VALUES (v_workout_amrap_id, 48, 3, 15, NOW());

    -- -----------------------------------------------------------------------------
    -- Create EMOM Workout (10 min EMOM)
    -- -----------------------------------------------------------------------------
    INSERT INTO workouts (
        user_id, name, original_text, parsed_description, workout_type,
        rep_scheme_type, time_cap_seconds, interval_duration_seconds,
        is_deleted, created_at, updated_at
    )
    VALUES (
        v_user_id,
        'Local Test EMOM',
        'EMOM 10:
Odd: 8 Hang Power Cleans (50kg)
Even: 12 Toes-to-Bar',
        'EMOM 10 - Alternating Hang Power Cleans and Toes-to-Bar',
        'emom',
        'fixed',
        600,  -- 10 minutes
        60,   -- 60 second intervals
        false,
        NOW(),
        NOW()
    )
    RETURNING id INTO v_workout_emom_id;

    -- Add workout movements for EMOM workout
    -- Hang Power Cleans (movement_definition_id = 10) - odd minutes
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        load_value, load_unit, minute_start, minute_end, created_at
    )
    VALUES (v_workout_emom_id, 10, 1, 8, 50.0, 'kg', 1, 9, NOW());

    -- Toes-to-Bar (movement_definition_id = 39) - even minutes
    INSERT INTO workout_movements (
        workout_id, movement_definition_id, sequence_order, rep_count,
        minute_start, minute_end, created_at
    )
    VALUES (v_workout_emom_id, 39, 2, 12, 2, 10, NOW());

    -- -----------------------------------------------------------------------------
    -- Output created IDs for reference
    -- -----------------------------------------------------------------------------
    RAISE NOTICE 'Created test data:';
    RAISE NOTICE '  User ID: %', v_user_id;
    RAISE NOTICE '  Athlete ID: %', v_athlete_id;
    RAISE NOTICE '  ForTime Workout ID: %', v_workout_fortime_id;
    RAISE NOTICE '  AMRAP Workout ID: %', v_workout_amrap_id;
    RAISE NOTICE '  EMOM Workout ID: %', v_workout_emom_id;
    RAISE NOTICE '';
    RAISE NOTICE 'Login credentials:';
    RAISE NOTICE '  Email: localtest@example.com';
    RAISE NOTICE '  Password: TestPassword123';

END $$;
