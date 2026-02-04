import React from 'react';
import { Badge } from '../ui';
import type { ConfidenceLevel } from '../../types/timeEstimate';
import {
  CONFIDENCE_LEVEL_BADGE_VARIANTS,
  CONFIDENCE_LEVEL_LABELS,
} from '../../types/timeEstimate';

export interface ConfidenceLevelIndicatorProps {
  /** The confidence level */
  level: ConfidenceLevel;
  /** Size of the indicator */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show the label text */
  showLabel?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * ConfidenceLevelIndicator - Visual indicator for estimate confidence
 *
 * Features:
 * - Color-coded badge based on confidence (green=High, yellow=Medium, red=Low)
 * - Optional label text
 * - Uses existing Badge component for consistency
 */
export const ConfidenceLevelIndicator: React.FC<ConfidenceLevelIndicatorProps> = ({
  level,
  size = 'md',
  showLabel = true,
  className = '',
}) => {
  const variant = CONFIDENCE_LEVEL_BADGE_VARIANTS[level];
  const label = CONFIDENCE_LEVEL_LABELS[level];

  return (
    <Badge
      variant={variant}
      size={size}
      rounded
      className={className}
    >
      {showLabel ? label : level}
    </Badge>
  );
};
