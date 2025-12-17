-- =============================================================================
-- Migration: V7__create_workout_tables.sql
-- Description: Creates workout parsing tables with ENUM types for Phase 2
-- Author: database-executor agent
-- Ticket: WOD-11
-- =============================================================================

-- -----------------------------------------------------------------------------
-- ENUM Types
-- -----------------------------------------------------------------------------

-- Workout type enum for classifying workout structure
CREATE TYPE workout_type AS ENUM (
    'amrap',      -- As Many Rounds As Possible
    'for_time',   -- Complete work for time
    'emom',       -- Every Minute On the Minute
    'intervals',  -- Interval-based training
    'rounds'      -- Fixed rounds for quality (not timed)
);

COMMENT ON TYPE workout_type IS 'Classification of workout structure/format';

-- Movement category enum for classifying movement types
CREATE TYPE movement_category AS ENUM (
    'weightlifting',  -- Olympic lifts, barbell movements
    'gymnastics',     -- Bodyweight/gymnastics movements
    'cardio',         -- Cardiovascular/monostructural
    'strongman'       -- Strongman implements, carries
);

COMMENT ON TYPE movement_category IS 'Category classification for movement types';

-- Load unit enum for weight specifications
CREATE TYPE load_unit AS ENUM (
    'kg',    -- Kilograms
    'lb',    -- Pounds
    'pood'   -- Russian kettlebell measurement (~16kg)
);

COMMENT ON TYPE load_unit IS 'Units for load/weight specification';

-- Distance unit enum for distance specifications
CREATE TYPE distance_unit AS ENUM (
    'm',    -- Meters
    'km',   -- Kilometers
    'ft',   -- Feet
    'mi',   -- Miles
    'cal'   -- Calories (for erg machines)
);

COMMENT ON TYPE distance_unit IS 'Units for distance specification';

-- -----------------------------------------------------------------------------
-- Tables
-- -----------------------------------------------------------------------------

-- Movement definitions table: Canonical movement library
CREATE TABLE movement_definitions (
    -- Primary key
    id SERIAL PRIMARY KEY,

    -- Movement identification
    canonical_name VARCHAR(100) NOT NULL,
    display_name VARCHAR(100) NOT NULL,

    -- Classification
    category movement_category NOT NULL,
    description VARCHAR(500) NULL,

    -- Status
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT uq_movement_definitions_canonical_name UNIQUE (canonical_name)
);

-- Table comment
COMMENT ON TABLE movement_definitions IS 'Canonical movement library for workout parsing';

-- Column comments
COMMENT ON COLUMN movement_definitions.id IS 'Unique auto-incrementing identifier for the movement definition';
COMMENT ON COLUMN movement_definitions.canonical_name IS 'Internal identifier for the movement (e.g., "toes_to_bar")';
COMMENT ON COLUMN movement_definitions.display_name IS 'Human-readable name (e.g., "Toes-to-Bar")';
COMMENT ON COLUMN movement_definitions.category IS 'Movement category (weightlifting/gymnastics/cardio/strongman)';
COMMENT ON COLUMN movement_definitions.description IS 'Optional description of the movement';
COMMENT ON COLUMN movement_definitions.is_active IS 'Whether this movement is currently available for use';
COMMENT ON COLUMN movement_definitions.created_at IS 'Record creation timestamp';

-- Movement aliases table: Common variations/abbreviations
CREATE TABLE movement_aliases (
    -- Primary key
    id SERIAL PRIMARY KEY,

    -- Foreign key
    movement_definition_id INTEGER NOT NULL,

    -- Alias data
    alias VARCHAR(100) NOT NULL,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraint
    CONSTRAINT fk_movement_aliases_definition
        FOREIGN KEY (movement_definition_id) REFERENCES movement_definitions(id)
        ON DELETE CASCADE,

    -- Constraints
    CONSTRAINT uq_movement_aliases_alias UNIQUE (alias)
);

-- Table comment
COMMENT ON TABLE movement_aliases IS 'Maps common variations/abbreviations to canonical movements';

-- Column comments
COMMENT ON COLUMN movement_aliases.id IS 'Unique auto-incrementing identifier for the alias';
COMMENT ON COLUMN movement_aliases.movement_definition_id IS 'Reference to the canonical movement definition';
COMMENT ON COLUMN movement_aliases.alias IS 'Alias text (e.g., "T2B", "toes-to-bar", "TTB")';
COMMENT ON COLUMN movement_aliases.created_at IS 'Record creation timestamp';

-- Workouts table: User workouts with parsed structure
CREATE TABLE workouts (
    -- Primary key
    id SERIAL PRIMARY KEY,

    -- Foreign key
    user_id INTEGER NOT NULL,

    -- Workout identification
    name VARCHAR(200) NULL,
    workout_type workout_type NOT NULL,

    -- Raw and parsed text
    original_text TEXT NOT NULL,
    parsed_description TEXT NULL,

    -- Time domain
    time_cap_seconds INTEGER NULL,
    round_count INTEGER NULL,
    interval_duration_seconds INTEGER NULL,

    -- Soft delete
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraint
    CONSTRAINT fk_workouts_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    -- Constraints
    CONSTRAINT chk_workouts_time_cap_positive CHECK (time_cap_seconds IS NULL OR time_cap_seconds > 0),
    CONSTRAINT chk_workouts_round_count_positive CHECK (round_count IS NULL OR round_count > 0),
    CONSTRAINT chk_workouts_interval_duration_positive CHECK (interval_duration_seconds IS NULL OR interval_duration_seconds > 0)
);

-- Table comment
COMMENT ON TABLE workouts IS 'User workouts with parsed structure from text input';

-- Column comments
COMMENT ON COLUMN workouts.id IS 'Unique auto-incrementing identifier for the workout';
COMMENT ON COLUMN workouts.user_id IS 'Reference to the user who owns this workout';
COMMENT ON COLUMN workouts.name IS 'Optional workout name';
COMMENT ON COLUMN workouts.workout_type IS 'Workout format (amrap/for_time/emom/intervals/rounds)';
COMMENT ON COLUMN workouts.original_text IS 'Raw text input from the user';
COMMENT ON COLUMN workouts.parsed_description IS 'Cleaned/normalized description after parsing';
COMMENT ON COLUMN workouts.time_cap_seconds IS 'Time cap in seconds (for timed workouts)';
COMMENT ON COLUMN workouts.round_count IS 'Number of rounds (for round-based workouts)';
COMMENT ON COLUMN workouts.interval_duration_seconds IS 'Interval duration in seconds (for EMOM)';
COMMENT ON COLUMN workouts.is_deleted IS 'Soft delete flag (true = deleted)';
COMMENT ON COLUMN workouts.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN workouts.updated_at IS 'Last update timestamp';

-- Workout movements table: Parsed movements within a workout
CREATE TABLE workout_movements (
    -- Primary key
    id SERIAL PRIMARY KEY,

    -- Foreign keys
    workout_id INTEGER NOT NULL,
    movement_definition_id INTEGER NOT NULL,

    -- Ordering
    sequence_order INTEGER NOT NULL,

    -- Movement specifications
    rep_count INTEGER NULL,
    load_value DECIMAL(8,2) NULL,
    load_unit load_unit NULL,
    distance_value DECIMAL(8,2) NULL,
    distance_unit distance_unit NULL,
    calories INTEGER NULL,
    duration_seconds INTEGER NULL,
    notes VARCHAR(500) NULL,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Foreign key constraints
    CONSTRAINT fk_workout_movements_workout
        FOREIGN KEY (workout_id) REFERENCES workouts(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_workout_movements_definition
        FOREIGN KEY (movement_definition_id) REFERENCES movement_definitions(id)
        ON DELETE RESTRICT,

    -- Constraints
    CONSTRAINT chk_workout_movements_rep_count_positive CHECK (rep_count IS NULL OR rep_count > 0),
    CONSTRAINT chk_workout_movements_load_value_positive CHECK (load_value IS NULL OR load_value > 0),
    CONSTRAINT chk_workout_movements_distance_value_positive CHECK (distance_value IS NULL OR distance_value > 0),
    CONSTRAINT chk_workout_movements_calories_positive CHECK (calories IS NULL OR calories > 0),
    CONSTRAINT chk_workout_movements_duration_positive CHECK (duration_seconds IS NULL OR duration_seconds > 0),
    CONSTRAINT chk_workout_movements_sequence_order_positive CHECK (sequence_order > 0)
);

-- Table comment
COMMENT ON TABLE workout_movements IS 'Parsed movements within a workout with specifications';

-- Column comments
COMMENT ON COLUMN workout_movements.id IS 'Unique auto-incrementing identifier for the workout movement';
COMMENT ON COLUMN workout_movements.workout_id IS 'Reference to the parent workout';
COMMENT ON COLUMN workout_movements.movement_definition_id IS 'Reference to the canonical movement definition';
COMMENT ON COLUMN workout_movements.sequence_order IS 'Order of the movement within the workout (1-indexed)';
COMMENT ON COLUMN workout_movements.rep_count IS 'Number of repetitions';
COMMENT ON COLUMN workout_movements.load_value IS 'Weight/load amount';
COMMENT ON COLUMN workout_movements.load_unit IS 'Weight unit (kg/lb/pood)';
COMMENT ON COLUMN workout_movements.distance_value IS 'Distance amount';
COMMENT ON COLUMN workout_movements.distance_unit IS 'Distance unit (m/km/ft/mi/cal)';
COMMENT ON COLUMN workout_movements.calories IS 'Calorie target (for erg machines)';
COMMENT ON COLUMN workout_movements.duration_seconds IS 'Time duration in seconds (for holds, etc.)';
COMMENT ON COLUMN workout_movements.notes IS 'Additional specifications or modifications';
COMMENT ON COLUMN workout_movements.created_at IS 'Record creation timestamp';

-- -----------------------------------------------------------------------------
-- Indexes
-- -----------------------------------------------------------------------------

-- movement_definitions indexes
CREATE INDEX idx_movement_definitions_category ON movement_definitions(category);
CREATE INDEX idx_movement_definitions_is_active ON movement_definitions(is_active)
    WHERE is_active = TRUE;

-- movement_aliases indexes
CREATE INDEX idx_movement_aliases_definition_id ON movement_aliases(movement_definition_id);
-- Note: alias already has unique constraint which creates an index

-- workouts indexes
CREATE INDEX idx_workouts_user_id ON workouts(user_id);
CREATE INDEX idx_workouts_workout_type ON workouts(workout_type);
CREATE INDEX idx_workouts_is_deleted ON workouts(is_deleted)
    WHERE is_deleted = FALSE;
CREATE INDEX idx_workouts_user_active ON workouts(user_id, is_deleted)
    WHERE is_deleted = FALSE;
CREATE INDEX idx_workouts_created_at ON workouts(created_at DESC);

-- workout_movements indexes
CREATE INDEX idx_workout_movements_workout_id ON workout_movements(workout_id);
CREATE INDEX idx_workout_movements_definition_id ON workout_movements(movement_definition_id);
CREATE INDEX idx_workout_movements_workout_order ON workout_movements(workout_id, sequence_order);

-- -----------------------------------------------------------------------------
-- Seed Data: Movement Definitions
-- -----------------------------------------------------------------------------

-- Weightlifting movements
INSERT INTO movement_definitions (canonical_name, display_name, category, description) VALUES
    ('back_squat', 'Back Squat', 'weightlifting', 'Barbell squat with bar on upper back'),
    ('front_squat', 'Front Squat', 'weightlifting', 'Barbell squat with bar in front rack position'),
    ('overhead_squat', 'Overhead Squat', 'weightlifting', 'Squat with barbell locked out overhead'),
    ('deadlift', 'Deadlift', 'weightlifting', 'Barbell lift from floor to standing'),
    ('sumo_deadlift_high_pull', 'Sumo Deadlift High Pull', 'weightlifting', 'Wide stance deadlift to high pull'),
    ('clean', 'Clean', 'weightlifting', 'Olympic lift from floor to front rack'),
    ('power_clean', 'Power Clean', 'weightlifting', 'Clean caught in power position'),
    ('squat_clean', 'Squat Clean', 'weightlifting', 'Clean caught in full squat'),
    ('hang_clean', 'Hang Clean', 'weightlifting', 'Clean starting from hang position'),
    ('hang_power_clean', 'Hang Power Clean', 'weightlifting', 'Power clean from hang position'),
    ('jerk', 'Jerk', 'weightlifting', 'Overhead lift from front rack'),
    ('push_jerk', 'Push Jerk', 'weightlifting', 'Jerk with push under'),
    ('split_jerk', 'Split Jerk', 'weightlifting', 'Jerk with split stance catch'),
    ('clean_and_jerk', 'Clean & Jerk', 'weightlifting', 'Combined clean and jerk'),
    ('snatch', 'Snatch', 'weightlifting', 'Olympic lift from floor to overhead'),
    ('power_snatch', 'Power Snatch', 'weightlifting', 'Snatch caught in power position'),
    ('squat_snatch', 'Squat Snatch', 'weightlifting', 'Snatch caught in full squat'),
    ('hang_snatch', 'Hang Snatch', 'weightlifting', 'Snatch from hang position'),
    ('hang_power_snatch', 'Hang Power Snatch', 'weightlifting', 'Power snatch from hang position'),
    ('thruster', 'Thruster', 'weightlifting', 'Front squat to push press'),
    ('cluster', 'Cluster', 'weightlifting', 'Squat clean to thruster'),
    ('push_press', 'Push Press', 'weightlifting', 'Overhead press with leg drive'),
    ('strict_press', 'Strict Press', 'weightlifting', 'Overhead press without leg drive'),
    ('bench_press', 'Bench Press', 'weightlifting', 'Horizontal pressing movement'),
    ('kettlebell_swing', 'Kettlebell Swing', 'weightlifting', 'Ballistic hip hinge with kettlebell'),
    ('dumbbell_snatch', 'Dumbbell Snatch', 'weightlifting', 'Single-arm snatch with dumbbell'),
    ('dumbbell_clean', 'Dumbbell Clean', 'weightlifting', 'Single or double dumbbell clean'),
    ('dumbbell_thruster', 'Dumbbell Thruster', 'weightlifting', 'Thruster with dumbbells'),
    ('wall_ball', 'Wall Ball', 'weightlifting', 'Squat and throw to target'),
    ('medicine_ball_clean', 'Medicine Ball Clean', 'weightlifting', 'Clean with medicine ball');

-- Gymnastics movements
INSERT INTO movement_definitions (canonical_name, display_name, category, description) VALUES
    ('pull_up', 'Pull-up', 'gymnastics', 'Vertical pulling movement'),
    ('kipping_pull_up', 'Kipping Pull-up', 'gymnastics', 'Pull-up with kipping motion'),
    ('strict_pull_up', 'Strict Pull-up', 'gymnastics', 'Pull-up without kipping'),
    ('butterfly_pull_up', 'Butterfly Pull-up', 'gymnastics', 'Pull-up with butterfly kip'),
    ('chest_to_bar_pull_up', 'Chest-to-Bar Pull-up', 'gymnastics', 'Pull-up touching chest to bar'),
    ('muscle_up', 'Muscle-up', 'gymnastics', 'Pull-up transitioning to dip'),
    ('bar_muscle_up', 'Bar Muscle-up', 'gymnastics', 'Muscle-up on pull-up bar'),
    ('ring_muscle_up', 'Ring Muscle-up', 'gymnastics', 'Muscle-up on gymnastics rings'),
    ('toes_to_bar', 'Toes-to-Bar', 'gymnastics', 'Hanging leg raise to bar'),
    ('knees_to_elbows', 'Knees-to-Elbows', 'gymnastics', 'Hanging knee raise'),
    ('handstand_push_up', 'Handstand Push-up', 'gymnastics', 'Inverted pressing movement'),
    ('strict_handstand_push_up', 'Strict Handstand Push-up', 'gymnastics', 'HSPU without kipping'),
    ('kipping_handstand_push_up', 'Kipping Handstand Push-up', 'gymnastics', 'HSPU with kipping'),
    ('handstand_walk', 'Handstand Walk', 'gymnastics', 'Walking on hands'),
    ('push_up', 'Push-up', 'gymnastics', 'Horizontal pressing bodyweight movement'),
    ('ring_dip', 'Ring Dip', 'gymnastics', 'Dip on gymnastics rings'),
    ('bar_dip', 'Bar Dip', 'gymnastics', 'Dip on parallel bars'),
    ('air_squat', 'Air Squat', 'gymnastics', 'Bodyweight squat'),
    ('pistol', 'Pistol', 'gymnastics', 'Single-leg squat'),
    ('lunge', 'Lunge', 'gymnastics', 'Walking or stationary lunge'),
    ('walking_lunge', 'Walking Lunge', 'gymnastics', 'Continuous lunging movement'),
    ('box_jump', 'Box Jump', 'gymnastics', 'Jump onto elevated platform'),
    ('box_jump_over', 'Box Jump Over', 'gymnastics', 'Jump over box to other side'),
    ('step_up', 'Step-up', 'gymnastics', 'Step onto elevated platform'),
    ('burpee', 'Burpee', 'gymnastics', 'Full body conditioning movement'),
    ('burpee_box_jump_over', 'Burpee Box Jump Over', 'gymnastics', 'Burpee with box jump over'),
    ('burpee_pull_up', 'Burpee Pull-up', 'gymnastics', 'Burpee with pull-up'),
    ('sit_up', 'Sit-up', 'gymnastics', 'Abdominal crunch movement'),
    ('ghd_sit_up', 'GHD Sit-up', 'gymnastics', 'Sit-up on GHD machine'),
    ('v_up', 'V-up', 'gymnastics', 'V-shaped abdominal movement'),
    ('double_under', 'Double-under', 'gymnastics', 'Jump rope passing twice per jump'),
    ('single_under', 'Single-under', 'gymnastics', 'Standard jump rope'),
    ('rope_climb', 'Rope Climb', 'gymnastics', 'Climbing vertical rope'),
    ('legless_rope_climb', 'Legless Rope Climb', 'gymnastics', 'Rope climb without leg assistance'),
    ('ring_row', 'Ring Row', 'gymnastics', 'Horizontal pull on rings'),
    ('l_sit', 'L-sit', 'gymnastics', 'Static hold in L position'),
    ('plank', 'Plank', 'gymnastics', 'Static core hold position');

-- Cardio movements
INSERT INTO movement_definitions (canonical_name, display_name, category, description) VALUES
    ('run', 'Run', 'cardio', 'Running for distance'),
    ('row', 'Row', 'cardio', 'Rowing on ergometer'),
    ('bike', 'Bike', 'cardio', 'Cycling (any type)'),
    ('assault_bike', 'Assault Bike', 'cardio', 'Air bike/fan bike'),
    ('echo_bike', 'Echo Bike', 'cardio', 'Rogue Echo Bike'),
    ('ski_erg', 'Ski Erg', 'cardio', 'Skiing ergometer'),
    ('swim', 'Swim', 'cardio', 'Swimming for distance'),
    ('shuttle_run', 'Shuttle Run', 'cardio', 'Back and forth running');

-- Strongman movements
INSERT INTO movement_definitions (canonical_name, display_name, category, description) VALUES
    ('farmers_carry', 'Farmers Carry', 'strongman', 'Walking while carrying weights'),
    ('yoke_carry', 'Yoke Carry', 'strongman', 'Walking with yoke on shoulders'),
    ('sandbag_carry', 'Sandbag Carry', 'strongman', 'Carrying sandbag'),
    ('atlas_stone', 'Atlas Stone', 'strongman', 'Lifting atlas stones'),
    ('tire_flip', 'Tire Flip', 'strongman', 'Flipping large tires'),
    ('sled_push', 'Sled Push', 'strongman', 'Pushing weighted sled'),
    ('sled_pull', 'Sled Pull', 'strongman', 'Pulling weighted sled'),
    ('sandbag_clean', 'Sandbag Clean', 'strongman', 'Cleaning sandbag to shoulder'),
    ('stone_to_shoulder', 'Stone to Shoulder', 'strongman', 'Lifting stone to shoulder');

-- -----------------------------------------------------------------------------
-- Seed Data: Movement Aliases
-- -----------------------------------------------------------------------------

-- Toes-to-Bar aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('t2b'), ('ttb'), ('toes-to-bar'), ('toes to bar'), ('t-2-b')
) AS aliases(alias)
WHERE canonical_name = 'toes_to_bar';

-- Chest-to-Bar Pull-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('c2b'), ('ctb'), ('chest-to-bar'), ('chest to bar'), ('c-2-b'), ('chest to bar pull-up'), ('chest-to-bar pull-up')
) AS aliases(alias)
WHERE canonical_name = 'chest_to_bar_pull_up';

-- Handstand Push-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('hspu'), ('handstand pushup'), ('handstand push up'), ('hs push-up'), ('hspus')
) AS aliases(alias)
WHERE canonical_name = 'handstand_push_up';

-- Muscle-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('mu'), ('muscle up'), ('muscleup'), ('mus')
) AS aliases(alias)
WHERE canonical_name = 'muscle_up';

-- Bar Muscle-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('bmu'), ('bar mu'), ('bar muscle up'), ('bar muscleup')
) AS aliases(alias)
WHERE canonical_name = 'bar_muscle_up';

-- Ring Muscle-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('rmu'), ('ring mu'), ('ring muscle up'), ('ring muscleup')
) AS aliases(alias)
WHERE canonical_name = 'ring_muscle_up';

-- Double-under aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('du'), ('dub'), ('dubs'), ('double under'), ('doubleunder'), ('double-unders'), ('dus')
) AS aliases(alias)
WHERE canonical_name = 'double_under';

-- Pull-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('pu'), ('pullup'), ('pull up'), ('pullups'), ('pull-ups')
) AS aliases(alias)
WHERE canonical_name = 'pull_up';

-- Clean & Jerk aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('c&j'), ('cnj'), ('c+j'), ('clean jerk'), ('clean-jerk'), ('clean n jerk')
) AS aliases(alias)
WHERE canonical_name = 'clean_and_jerk';

-- Thruster aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('thr'), ('thrusters')
) AS aliases(alias)
WHERE canonical_name = 'thruster';

-- Kettlebell Swing aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('kbs'), ('kb swing'), ('kettlebell swings'), ('kb swings'), ('russian swing'), ('american swing')
) AS aliases(alias)
WHERE canonical_name = 'kettlebell_swing';

-- Wall Ball aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('wb'), ('wall balls'), ('wbs'), ('wall-ball'), ('wallball')
) AS aliases(alias)
WHERE canonical_name = 'wall_ball';

-- Box Jump aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('bj'), ('box jumps'), ('boxjump'), ('box-jump')
) AS aliases(alias)
WHERE canonical_name = 'box_jump';

-- Burpee aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('burpees'), ('brp')
) AS aliases(alias)
WHERE canonical_name = 'burpee';

-- Row aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('rowing'), ('erg'), ('rower'), ('c2 row'), ('concept2 row')
) AS aliases(alias)
WHERE canonical_name = 'row';

-- Run aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('running'), ('sprint')
) AS aliases(alias)
WHERE canonical_name = 'run';

-- Assault Bike aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('aab'), ('ab'), ('air bike'), ('airbike'), ('assault air bike'), ('fan bike')
) AS aliases(alias)
WHERE canonical_name = 'assault_bike';

-- GHD Sit-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('ghd'), ('ghd situp'), ('ghd sit up'), ('ghdsu')
) AS aliases(alias)
WHERE canonical_name = 'ghd_sit_up';

-- Rope Climb aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('rc'), ('rope climbs'), ('rope-climb')
) AS aliases(alias)
WHERE canonical_name = 'rope_climb';

-- Deadlift aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('dl'), ('deadlifts'), ('dead lift'), ('deads')
) AS aliases(alias)
WHERE canonical_name = 'deadlift';

-- Sumo Deadlift High Pull aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('sdhp'), ('sumo deadlift hp'), ('sumo high pull')
) AS aliases(alias)
WHERE canonical_name = 'sumo_deadlift_high_pull';

-- Power Clean aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('pc'), ('pwr clean'), ('pwr cln'), ('power cleans')
) AS aliases(alias)
WHERE canonical_name = 'power_clean';

-- Power Snatch aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('ps'), ('pwr snatch'), ('pwr sn'), ('power snatches')
) AS aliases(alias)
WHERE canonical_name = 'power_snatch';

-- Push Press aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('pp'), ('push presses')
) AS aliases(alias)
WHERE canonical_name = 'push_press';

-- Strict Press aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('sp'), ('shoulder press'), ('press'), ('overhead press'), ('ohp')
) AS aliases(alias)
WHERE canonical_name = 'strict_press';

-- Pistol aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('pistols'), ('pistol squat'), ('pistol squats'), ('single leg squat')
) AS aliases(alias)
WHERE canonical_name = 'pistol';

-- Knees-to-Elbows aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('k2e'), ('kte'), ('knees to elbows'), ('knees-to-elbow')
) AS aliases(alias)
WHERE canonical_name = 'knees_to_elbows';

-- Sit-up aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('su'), ('situp'), ('sit up'), ('situps'), ('sit ups'), ('sit-ups')
) AS aliases(alias)
WHERE canonical_name = 'sit_up';

-- Air Squat aliases
INSERT INTO movement_aliases (movement_definition_id, alias)
SELECT id, alias
FROM movement_definitions, (VALUES
    ('as'), ('airsquat'), ('air squats'), ('bodyweight squat'), ('bw squat')
) AS aliases(alias)
WHERE canonical_name = 'air_squat';
