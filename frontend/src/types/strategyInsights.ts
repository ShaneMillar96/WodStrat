/**
 * Difficulty label as returned from the API
 */
export type DifficultyLabel = 'Easy' | 'Moderate' | 'Challenging' | 'Very Hard';

/**
 * Confidence level for strategy
 */
export type StrategyConfidenceLevel = 'High' | 'Medium' | 'Low';

/**
 * Risk alert severity
 */
export type AlertSeverity = 'High' | 'Medium' | 'Low';

/**
 * Breakdown of factors contributing to difficulty score
 */
export interface DifficultyBreakdown {
  /** Contribution from pacing analysis (0-10) */
  pacingFactor: number;
  /** Contribution from volume load analysis (0-10) */
  volumeFactor: number;
  /** Contribution from time estimate analysis (0-10) */
  timeFactor: number;
  /** Modifier based on athlete experience level */
  experienceModifier: number;
}

/**
 * Overall difficulty assessment for a workout
 */
export interface DifficultyScore {
  /** Difficulty score on a 1-10 scale */
  score: number;
  /** Human-readable label */
  label: DifficultyLabel;
  /** Detailed description of the difficulty assessment */
  description: string;
  /** Breakdown of contributing factors */
  breakdown: DifficultyBreakdown;
}

/**
 * Confidence assessment for the strategy
 */
export interface StrategyConfidence {
  /** Confidence level (High, Medium, Low) */
  level: StrategyConfidenceLevel;
  /** Confidence percentage (0-100) */
  percentage: number;
  /** Explanation of the confidence level */
  explanation: string;
  /** List of benchmarks that would improve confidence if recorded */
  missingBenchmarks: string[];
}

/**
 * Key movement requiring special attention
 */
export interface KeyFocusMovement {
  /** Name of the movement */
  movementName: string;
  /** Reason why this movement requires focus */
  reason: string;
  /** Strategic recommendation for the movement */
  recommendation: string;
  /** Priority ranking (1 = highest priority) */
  priority: number;
}

/**
 * Risk alert for a workout
 */
export interface RiskAlert {
  /** Type of risk alert */
  alertType: string;
  /** Alert severity (High, Medium, Low) */
  severity: AlertSeverity;
  /** Short title for the alert */
  title: string;
  /** Detailed message explaining the risk */
  message: string;
  /** Movements affected by this alert */
  affectedMovements: string[];
  /** Recommended action to address the alert */
  suggestedAction: string;
}

/**
 * Complete strategy insights for a workout
 */
export interface StrategyInsightsResult {
  /** The workout's unique identifier */
  workoutId: number;
  /** Display name of the workout */
  workoutName: string;
  /** Overall difficulty assessment */
  difficultyScore: DifficultyScore;
  /** Confidence assessment for the strategy */
  strategyConfidence: StrategyConfidence;
  /** Key movements requiring special attention */
  keyFocusMovements: KeyFocusMovement[];
  /** Risk alerts and recommendations */
  riskAlerts: RiskAlert[];
  /** Timestamp when the insights were calculated */
  calculatedAt: string;
}

/**
 * Badge variants for difficulty labels
 */
export const DIFFICULTY_LABEL_BADGE_VARIANTS: Record<DifficultyLabel, 'success' | 'blue' | 'warning' | 'error'> = {
  Easy: 'success',
  Moderate: 'blue',
  Challenging: 'warning',
  'Very Hard': 'error',
};

/**
 * Badge variants for alert severity
 */
export const ALERT_SEVERITY_BADGE_VARIANTS: Record<AlertSeverity, 'error' | 'warning' | 'blue'> = {
  High: 'error',
  Medium: 'warning',
  Low: 'blue',
};

/**
 * Color mapping for alert severity (Tailwind classes)
 */
export const ALERT_SEVERITY_COLORS: Record<AlertSeverity, { bg: string; text: string; border: string; icon: string }> = {
  High: {
    bg: 'bg-red-50',
    text: 'text-red-700',
    border: 'border-red-200',
    icon: 'text-red-400',
  },
  Medium: {
    bg: 'bg-yellow-50',
    text: 'text-yellow-700',
    border: 'border-yellow-200',
    icon: 'text-yellow-400',
  },
  Low: {
    bg: 'bg-blue-50',
    text: 'text-blue-700',
    border: 'border-blue-200',
    icon: 'text-blue-400',
  },
};
