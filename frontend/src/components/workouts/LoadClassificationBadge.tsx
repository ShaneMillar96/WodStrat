import React from 'react';
import { Badge } from '../ui';
import type { LoadClassification } from '../../types/volumeLoad';
import {
  LOAD_CLASSIFICATION_BADGE_VARIANTS,
  LOAD_CLASSIFICATION_LABELS,
} from '../../types/volumeLoad';

export interface LoadClassificationBadgeProps {
  /** The load classification level */
  classification: LoadClassification;
  /** Size of the badge */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show the label text */
  showLabel?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * LoadClassificationBadge - Visual indicator for load classification
 *
 * Features:
 * - Color-coded badge based on classification (red=High, yellow=Moderate, green=Low)
 * - Optional label text
 * - Uses existing Badge component for consistency
 */
export const LoadClassificationBadge: React.FC<LoadClassificationBadgeProps> = ({
  classification,
  size = 'md',
  showLabel = true,
  className = '',
}) => {
  const variant = LOAD_CLASSIFICATION_BADGE_VARIANTS[classification];
  const label = LOAD_CLASSIFICATION_LABELS[classification];

  return (
    <Badge
      variant={variant}
      size={size}
      rounded
      className={className}
    >
      {showLabel ? label : classification}
    </Badge>
  );
};
