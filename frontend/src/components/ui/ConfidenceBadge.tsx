import React from 'react';
import { Badge } from './Badge';
import type { StrategyConfidenceLevel } from '../../types/strategyInsights';

export interface ConfidenceBadgeProps {
  /** Confidence level */
  level: StrategyConfidenceLevel;
  /** Confidence percentage (optional) */
  percentage?: number;
  /** Size of the badge */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show percentage */
  showPercentage?: boolean;
  /** Additional CSS classes */
  className?: string;
}

const CONFIDENCE_BADGE_VARIANTS: Record<StrategyConfidenceLevel, 'success' | 'warning' | 'error'> = {
  High: 'success',
  Medium: 'warning',
  Low: 'error',
};

/**
 * ConfidenceBadge - Badge showing strategy confidence level
 */
export const ConfidenceBadge: React.FC<ConfidenceBadgeProps> = ({
  level,
  percentage,
  size = 'md',
  showPercentage = false,
  className = '',
}) => {
  const variant = CONFIDENCE_BADGE_VARIANTS[level];
  const displayText = showPercentage && percentage !== undefined
    ? `${level} (${percentage}%)`
    : level;

  return (
    <Badge variant={variant} size={size} rounded className={className}>
      {displayText}
    </Badge>
  );
};
