import React from 'react';
import type { TimeDomainBadgeProps } from '../../../types/workoutPreview';
import { TIME_DOMAIN_STYLES, TIME_DOMAIN_LABELS, SIZE_STYLES } from '../../../utils/workoutStyles';

/**
 * Badge component for displaying workout time domain (Short/Medium/Long)
 */
export const TimeDomainBadge: React.FC<TimeDomainBadgeProps> = ({
  domain,
  size = 'md',
  className = '',
}) => {
  const domainStyle = TIME_DOMAIN_STYLES[domain] || TIME_DOMAIN_STYLES.medium;
  const label = TIME_DOMAIN_LABELS[domain] || domain;
  const sizeStyle = SIZE_STYLES[size];

  return (
    <span
      className={`inline-flex items-center font-medium rounded ${domainStyle} ${sizeStyle.text} ${sizeStyle.padding} ${className}`}
    >
      {label}
    </span>
  );
};
