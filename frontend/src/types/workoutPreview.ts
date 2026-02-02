import type { ParsedWorkout, ParsedMovement } from './workout';
import type { ParsingIssue } from './parsingError';

/**
 * Time domain classification based on workout duration
 */
export type TimeDomain = 'short' | 'medium' | 'long';

/**
 * Rep scheme type classification
 */
export type RepSchemeType = 'descending' | 'ascending' | 'fixed' | 'custom';

/**
 * Rep scheme information
 */
export interface RepScheme {
  type: RepSchemeType;
  reps: number[];
}

/**
 * Volume summary for a single movement
 */
export interface MovementVolume {
  movementName: string;
  totalReps: number;
  totalDistance?: number;
  totalCalories?: number;
}

/**
 * Parsed workout preview props (enhanced)
 */
export interface ParsedWorkoutPreviewProps {
  workout: ParsedWorkout;
  onSave?: () => void;
  onEdit?: () => void;
  onAcceptSuggestion?: (warningIndex: number, suggestion: string) => void;
  showRawText?: boolean;
  compact?: boolean;
  className?: string;
}

/**
 * Movement card props (enhanced)
 */
export interface MovementCardProps {
  movement: ParsedMovement;
  sequenceNumber: number;
  repScheme?: number[];
  onEdit?: () => void;
  showError?: boolean;
  showCategoryIcon?: boolean;
  className?: string;
}

/**
 * Confidence indicator props (enhanced)
 */
export interface ConfidenceIndicatorProps {
  score: number;
  showTooltip?: boolean;
  showLabel?: boolean;
  size?: 'sm' | 'md' | 'lg';
  errorCount?: number;
  unrecognizedCount?: number;
  totalMovements?: number;
  className?: string;
}

/**
 * Volume summary props
 */
export interface VolumeSummaryProps {
  volumes: MovementVolume[];
  showBars?: boolean;
  className?: string;
}

/**
 * Time domain badge props
 */
export interface TimeDomainBadgeProps {
  domain: TimeDomain;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

/**
 * Rep scheme display props
 */
export interface RepSchemeDisplayProps {
  scheme: RepScheme;
  compact?: boolean;
  className?: string;
}

/**
 * Warning item props
 */
export interface WarningItemProps {
  warning: ParsingIssue;
  index: number;
  onApplySuggestion?: (index: number, suggestion: string) => void;
  className?: string;
}

/**
 * Warnings list props
 */
export interface WarningsListProps {
  warnings: ParsingIssue[];
  onApplySuggestion?: (index: number, suggestion: string) => void;
  className?: string;
}

/**
 * Confidence tooltip props
 */
export interface ConfidenceTooltipProps {
  confidence: number;
  errorCount: number;
  unrecognizedCount: number;
  totalMovements: number;
  children: React.ReactNode;
}

/**
 * Volume bar props
 */
export interface VolumeBarProps {
  movementName: string;
  totalReps: number;
  maxReps: number;
  className?: string;
}

/**
 * Movement category icon props
 */
export interface MovementCategoryIconProps {
  category: string | null;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

/**
 * Weight display props
 */
export interface WeightDisplayProps {
  loadValue: number | null;
  loadValueFemale?: number | null;
  loadUnit: string | null;
  className?: string;
}

/**
 * Distance display props
 */
export interface DistanceDisplayProps {
  distanceValue: number | null;
  distanceUnit: string | null;
  className?: string;
}

/**
 * Calorie display props
 */
export interface CalorieDisplayProps {
  calories: number | null;
  caloriesFemale?: number | null;
  className?: string;
}

/**
 * Unrecognized movement badge props
 */
export interface UnrecognizedMovementBadgeProps {
  originalText: string;
  className?: string;
}

/**
 * Workout header props
 */
export interface WorkoutHeaderProps {
  workoutType: string;
  timeDomain: TimeDomain;
  confidence: number;
  name?: string | null;
  parsedDescription?: string | null;
  errorCount?: number;
  unrecognizedCount?: number;
  totalMovements?: number;
  className?: string;
}

/**
 * Workout metadata props (enhanced)
 */
export interface WorkoutMetadataProps {
  timeCapFormatted: string | null;
  roundCount: number | null;
  intervalDurationFormatted: string | null;
  compact?: boolean;
  className?: string;
}
