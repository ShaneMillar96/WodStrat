import React from 'react';
import { Badge } from '../ui';
import type { BadgeVariant, BadgeSize } from '../ui/Badge';
import type { WorkoutType } from '../../types/workout';
import { WORKOUT_TYPE_LABELS, WORKOUT_TYPE_COLORS } from '../../types/workout';

export interface WorkoutTypeTagProps {
  /** Workout type value */
  type: WorkoutType;
  /** Size of the badge */
  size?: BadgeSize;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Consistent badge component for workout types with color mapping
 */
export const WorkoutTypeTag: React.FC<WorkoutTypeTagProps> = ({
  type,
  size = 'md',
  className = '',
}) => {
  const variant = WORKOUT_TYPE_COLORS[type] as BadgeVariant;
  const label = WORKOUT_TYPE_LABELS[type];

  return (
    <Badge variant={variant} size={size} className={className}>
      {label}
    </Badge>
  );
};
