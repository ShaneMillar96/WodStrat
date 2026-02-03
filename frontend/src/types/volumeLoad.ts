/**
 * Load classification levels as returned from the API
 */
export type LoadClassification = 'High' | 'Moderate' | 'Low';

/**
 * Volume load data for a single movement
 */
export interface MovementVolumeLoad {
  /** Reference to the movement definition */
  movementDefinitionId: number;
  /** Display name of the movement */
  movementName: string;
  /** Weight used for this movement */
  weight: number;
  /** Unit of weight (kg, lb) */
  weightUnit: string;
  /** Number of repetitions per round */
  reps: number;
  /** Number of rounds in the workout */
  rounds: number;
  /** Total volume load (weight x reps x rounds) */
  volumeLoad: number;
  /** Formatted volume load string (e.g., "2,150 kg") */
  volumeLoadFormatted: string;
  /** Classification of the load relative to athlete's capacity */
  loadClassification: LoadClassification;
  /** Name of the benchmark used for comparison */
  benchmarkUsed: string;
  /** Athlete's percentile for this benchmark (null if no benchmark data) */
  athleteBenchmarkPercentile: number | null;
  /** Actionable tip for this movement */
  tip: string;
  /** Recommended scaled weight (null if RX is appropriate) */
  recommendedWeight: number | null;
  /** Formatted recommended weight (e.g., "34 kg (80% of RX)") */
  recommendedWeightFormatted: string | null;
}

/**
 * Complete volume load analysis for a workout
 */
export interface WorkoutVolumeLoadResult {
  /** The workout's unique identifier */
  workoutId: number;
  /** Optional workout name */
  workoutName: string;
  /** Volume load analysis for each movement */
  movementVolumes: MovementVolumeLoad[];
  /** Sum of all movement volume loads */
  totalVolumeLoad: number;
  /** Formatted total volume load (e.g., "5,430 kg") */
  totalVolumeLoadFormatted: string;
  /** Overall assessment text based on combined analysis */
  overallAssessment: string;
  /** Timestamp when the calculation was performed */
  calculatedAt: string;
}

/**
 * Request payload for calculating volume load
 */
export interface CalculateVolumeLoadRequest {
  /** The athlete's unique identifier */
  athleteId: number;
  /** The workout's unique identifier */
  workoutId: number;
}

/**
 * Badge variants mapping for load classification
 */
export const LOAD_CLASSIFICATION_BADGE_VARIANTS: Record<LoadClassification, 'error' | 'warning' | 'success'> = {
  High: 'error',
  Moderate: 'warning',
  Low: 'success',
};

/**
 * Display labels for load classification
 */
export const LOAD_CLASSIFICATION_LABELS: Record<LoadClassification, string> = {
  High: 'High Load',
  Moderate: 'Moderate Load',
  Low: 'Low Load',
};

/**
 * Color mapping for load classification (Tailwind classes)
 */
export const LOAD_CLASSIFICATION_COLORS: Record<LoadClassification, { bg: string; text: string; border: string }> = {
  High: {
    bg: 'bg-red-50',
    text: 'text-red-700',
    border: 'border-red-200',
  },
  Moderate: {
    bg: 'bg-yellow-50',
    text: 'text-yellow-700',
    border: 'border-yellow-200',
  },
  Low: {
    bg: 'bg-green-50',
    text: 'text-green-700',
    border: 'border-green-200',
  },
};
