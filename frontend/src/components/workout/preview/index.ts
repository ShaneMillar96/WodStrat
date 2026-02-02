/**
 * Barrel exports for workout preview components
 */

// Main container
export { ParsedWorkoutPreview } from './ParsedWorkoutPreview';

// Section components
export { WorkoutHeader } from './WorkoutHeader';
export { WorkoutMetadata } from './WorkoutMetadata';

// Movement components
export { MovementList } from './MovementList';
export { MovementCard } from './MovementCard';

// Composite components
export { RepSchemeDisplay } from './RepSchemeDisplay';
export { VolumeSummary } from './VolumeSummary';
export { ConfidenceIndicator } from './ConfidenceIndicator';
export { ConfidenceTooltip } from './ConfidenceTooltip';
export { WarningsList } from './WarningsList';
export { WarningItem } from './WarningItem';

// Atomic components
export { TimeDomainBadge } from './TimeDomainBadge';
export { MovementCategoryIcon } from './MovementCategoryIcon';
export { WeightDisplay } from './WeightDisplay';
export { DistanceDisplay } from './DistanceDisplay';
export { CalorieDisplay } from './CalorieDisplay';
export { UnrecognizedMovementBadge } from './UnrecognizedMovementBadge';
export { VolumeBar } from './VolumeBar';

// Re-export types for convenience
export type {
  ParsedWorkoutPreviewProps,
  MovementCardProps,
  ConfidenceIndicatorProps,
  VolumeSummaryProps,
  TimeDomainBadgeProps,
  RepSchemeDisplayProps,
  WarningItemProps,
  WarningsListProps,
  ConfidenceTooltipProps,
  VolumeBarProps,
  MovementCategoryIconProps,
  WeightDisplayProps,
  DistanceDisplayProps,
  CalorieDisplayProps,
  UnrecognizedMovementBadgeProps,
  WorkoutHeaderProps,
  WorkoutMetadataProps,
  TimeDomain,
  RepScheme,
  RepSchemeType,
  MovementVolume,
} from '../../../types/workoutPreview';
