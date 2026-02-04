/**
 * Confidence level for time estimates
 */
export type ConfidenceLevel = 'High' | 'Medium' | 'Low';

/**
 * Estimate type based on workout type
 */
export type EstimateType = 'Time' | 'RoundsReps';

/**
 * Rest recommendation after a movement
 */
export interface RestRecommendation {
  /** The movement after which to rest */
  afterMovement: string;
  /** Suggested rest duration in seconds */
  suggestedRestSeconds: number;
  /** Formatted rest range (e.g., "10-15 seconds") */
  restRange: string;
  /** Reasoning for the rest recommendation */
  reasoning: string;
}

/**
 * Time estimate result for a workout
 */
export interface TimeEstimateResult {
  /** The workout's unique identifier */
  workoutId: number;
  /** Workout name for display */
  workoutName: string;
  /** Type of workout (ForTime, Amrap, Emom, etc.) */
  workoutType: string;
  /** Type of estimate (Time or RoundsReps) */
  estimateType: EstimateType;
  /** Minimum estimate (seconds for Time, total reps for RoundsReps) */
  minEstimate: number;
  /** Maximum estimate (seconds for Time, total reps for RoundsReps) */
  maxEstimate: number;
  /** Formatted range string (e.g., "8:30 - 10:15" or "5 rounds + 12 reps to 6 rounds + 3 reps") */
  formattedRange: string;
  /** Confidence level of the estimate */
  confidenceLevel: ConfidenceLevel;
  /** Summary of factors affecting the estimate */
  factorsSummary: string;
  /** List of rest recommendations */
  restRecommendations: RestRecommendation[];
  /** Timestamp when the calculation was performed */
  calculatedAt: string;
}

/**
 * EMOM feasibility for a single minute
 */
export interface EmomFeasibility {
  /** The minute number (1-based) */
  minute: number;
  /** Description of prescribed work for this minute */
  prescribedWork: string;
  /** Estimated time to complete in seconds */
  estimatedCompletionSeconds: number;
  /** Whether the minute is feasible (completion < 60 seconds) */
  isFeasible: boolean;
  /** Buffer seconds remaining (60 - completion time) */
  bufferSeconds: number;
  /** Recommendation for this minute */
  recommendation: string;
}

/**
 * Request payload for calculating time estimate
 */
export interface CalculateTimeEstimateRequest {
  /** The athlete's unique identifier */
  athleteId: number;
  /** The workout's unique identifier */
  workoutId: number;
}

/**
 * Badge variants mapping for confidence level
 */
export const CONFIDENCE_LEVEL_BADGE_VARIANTS: Record<ConfidenceLevel, 'success' | 'warning' | 'error'> = {
  High: 'success',
  Medium: 'warning',
  Low: 'error',
};

/**
 * Display labels for confidence level
 */
export const CONFIDENCE_LEVEL_LABELS: Record<ConfidenceLevel, string> = {
  High: 'High Confidence',
  Medium: 'Medium Confidence',
  Low: 'Low Confidence',
};

/**
 * Color mapping for confidence level (Tailwind classes)
 */
export const CONFIDENCE_LEVEL_COLORS: Record<ConfidenceLevel, { bg: string; text: string; border: string }> = {
  High: {
    bg: 'bg-green-50',
    text: 'text-green-700',
    border: 'border-green-200',
  },
  Medium: {
    bg: 'bg-yellow-50',
    text: 'text-yellow-700',
    border: 'border-yellow-200',
  },
  Low: {
    bg: 'bg-red-50',
    text: 'text-red-700',
    border: 'border-red-200',
  },
};

/**
 * Color mapping for EMOM feasibility
 */
export const EMOM_FEASIBILITY_COLORS = {
  feasible: {
    bg: 'bg-green-100',
    text: 'text-green-800',
    border: 'border-green-300',
    indicator: 'bg-green-500',
  },
  notFeasible: {
    bg: 'bg-red-100',
    text: 'text-red-800',
    border: 'border-red-300',
    indicator: 'bg-red-500',
  },
  warning: {
    bg: 'bg-yellow-100',
    text: 'text-yellow-800',
    border: 'border-yellow-300',
    indicator: 'bg-yellow-500',
  },
};
