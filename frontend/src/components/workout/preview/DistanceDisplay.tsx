import React from 'react';
import type { DistanceDisplayProps } from '../../../types/workoutPreview';

/**
 * Map of distance unit codes to display labels
 */
const DISTANCE_UNIT_LABELS: Record<string, string> = {
  M: 'm',
  Km: 'km',
  Ft: 'ft',
  Mi: 'mi',
  Cal: 'cal',
};

/**
 * Component for displaying formatted distance
 */
export const DistanceDisplay: React.FC<DistanceDisplayProps> = ({
  distanceValue,
  distanceUnit,
  className = '',
}) => {
  if (distanceValue === null) return null;

  const unitLabel = distanceUnit ? (DISTANCE_UNIT_LABELS[distanceUnit] || distanceUnit.toLowerCase()) : 'm';

  return (
    <span className={`text-gray-600 ${className}`}>
      <span className="font-medium">{distanceValue}</span>
      <span className="ml-0.5">{unitLabel}</span>
    </span>
  );
};
