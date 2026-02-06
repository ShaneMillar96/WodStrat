/**
 * Pacing level classification as returned from the API
 */
export type PacingLevel = 'Light' | 'Moderate' | 'Heavy';

/**
 * Pacing data for a single movement
 */
export interface MovementPacing {
  /** Reference to the movement definition */
  movementDefinitionId: number;
  /** Display name of the movement */
  movementName: string;
  /** Recommended pacing level */
  pacingLevel: PacingLevel;
  /** Athlete's benchmark percentile for this movement (null if no data) */
  athletePercentile: number | null;
  /** Guidance text for the movement */
  guidanceText: string;
  /** Recommended set breakdown (e.g., [7, 7, 7] for "break into 7s") */
  recommendedSets: number[];
  /** Benchmark used for calculation (null if none) */
  benchmarkUsed: string | null;
}

/**
 * Complete pacing analysis for a workout
 */
export interface WorkoutPacingResult {
  /** The workout's unique identifier */
  workoutId: number;
  /** Display name of the workout */
  workoutName: string;
  /** Pacing analysis for each movement */
  movementPacing: MovementPacing[];
  /** Overall strategy notes */
  overallStrategyNotes: string;
  /** Timestamp when the calculation was performed */
  calculatedAt: string;
}

/**
 * Badge variants mapping for pacing level
 */
export const PACING_LEVEL_BADGE_VARIANTS: Record<PacingLevel, 'success' | 'warning' | 'error'> = {
  Light: 'success',
  Moderate: 'warning',
  Heavy: 'error',
};

/**
 * Display labels for pacing level
 */
export const PACING_LEVEL_LABELS: Record<PacingLevel, string> = {
  Light: 'Light Pace',
  Moderate: 'Moderate Pace',
  Heavy: 'Heavy Pace',
};

/**
 * Color mapping for pacing level (Tailwind classes)
 */
export const PACING_LEVEL_COLORS: Record<PacingLevel, { bg: string; text: string; border: string }> = {
  Light: {
    bg: 'bg-green-50',
    text: 'text-green-700',
    border: 'border-green-200',
  },
  Moderate: {
    bg: 'bg-yellow-50',
    text: 'text-yellow-700',
    border: 'border-yellow-200',
  },
  Heavy: {
    bg: 'bg-red-50',
    text: 'text-red-700',
    border: 'border-red-200',
  },
};
