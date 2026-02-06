import React from 'react';
import { Badge } from './Badge';
import type { AlertSeverity } from '../../types/strategyInsights';
import { ALERT_SEVERITY_BADGE_VARIANTS } from '../../types/strategyInsights';

export interface SeverityBadgeProps {
  /** Alert severity */
  severity: AlertSeverity;
  /** Size of the badge */
  size?: 'sm' | 'md' | 'lg';
  /** Additional CSS classes */
  className?: string;
}

/**
 * SeverityBadge - Badge for alert severity indication
 */
export const SeverityBadge: React.FC<SeverityBadgeProps> = ({
  severity,
  size = 'sm',
  className = '',
}) => {
  const variant = ALERT_SEVERITY_BADGE_VARIANTS[severity];

  return (
    <Badge variant={variant} size={size} rounded className={className}>
      {severity}
    </Badge>
  );
};
