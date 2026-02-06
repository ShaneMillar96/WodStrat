import React from 'react';
import { Badge } from './Badge';
import type { PacingLevel } from '../../types/pacing';
import { PACING_LEVEL_BADGE_VARIANTS, PACING_LEVEL_LABELS } from '../../types/pacing';

export interface PacingBadgeProps {
  /** Pacing level */
  pacingLevel: PacingLevel;
  /** Size of the badge */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show full label */
  showLabel?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * PacingBadge - Badge for pacing level indication
 */
export const PacingBadge: React.FC<PacingBadgeProps> = ({
  pacingLevel,
  size = 'md',
  showLabel = true,
  className = '',
}) => {
  const variant = PACING_LEVEL_BADGE_VARIANTS[pacingLevel];
  const label = showLabel ? PACING_LEVEL_LABELS[pacingLevel] : pacingLevel;

  return (
    <Badge variant={variant} size={size} rounded className={className}>
      {label}
    </Badge>
  );
};
