-- =============================================================================
-- Migration: V6__create_users_table.sql
-- Description: Creates users table for authentication and establishes
--              one-to-one relationship with athletes
-- Author: database-executor agent
-- Ticket: WOD-9
-- =============================================================================

-- -----------------------------------------------------------------------------
-- STEP 1: Clean existing test data
-- -----------------------------------------------------------------------------

-- Delete all athlete benchmarks (child records first due to FK)
DELETE FROM athlete_benchmarks;

-- Delete all athletes
DELETE FROM athletes;

-- Reset sequences to start fresh
-- Note: Sequences retained '_new' suffix from V5 migration rename
ALTER SEQUENCE athletes_new_id_seq RESTART WITH 1;
ALTER SEQUENCE athlete_benchmarks_new_id_seq RESTART WITH 1;

-- -----------------------------------------------------------------------------
-- STEP 2: Create users table
-- -----------------------------------------------------------------------------

CREATE TABLE users (
    -- Primary key (auto-incrementing INT)
    id SERIAL PRIMARY KEY,

    -- Authentication fields
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,

    -- Account status
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT chk_users_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- Table comment
COMMENT ON TABLE users IS 'User accounts for authentication in WodStrat';

-- Column comments
COMMENT ON COLUMN users.id IS 'Unique auto-incrementing identifier for the user';
COMMENT ON COLUMN users.email IS 'User email address (used for login)';
COMMENT ON COLUMN users.password_hash IS 'BCrypt hashed password';
COMMENT ON COLUMN users.is_active IS 'Account active status (false = deactivated)';
COMMENT ON COLUMN users.created_at IS 'Record creation timestamp';
COMMENT ON COLUMN users.updated_at IS 'Last update timestamp';

-- -----------------------------------------------------------------------------
-- STEP 3: Create indexes for users table
-- -----------------------------------------------------------------------------

-- Index for email lookups (login queries) - already covered by unique constraint
-- but explicit index for clarity
CREATE INDEX idx_users_email ON users(email);

-- Index for active users
CREATE INDEX idx_users_is_active ON users(is_active) WHERE is_active = TRUE;

-- Composite index for login query pattern
CREATE INDEX idx_users_email_active ON users(email, is_active) WHERE is_active = TRUE;

-- -----------------------------------------------------------------------------
-- STEP 4: Add foreign key constraint from athletes to users
-- -----------------------------------------------------------------------------

ALTER TABLE athletes
    ADD CONSTRAINT fk_athletes_user
    FOREIGN KEY (user_id) REFERENCES users(id)
    ON DELETE CASCADE;

-- -----------------------------------------------------------------------------
-- STEP 5: Add unique constraint for one-to-one relationship
-- -----------------------------------------------------------------------------

ALTER TABLE athletes
    ADD CONSTRAINT uq_athletes_user_id UNIQUE (user_id);

-- Update column comment
COMMENT ON COLUMN athletes.user_id IS 'Foreign key to users table (one athlete per user)';
