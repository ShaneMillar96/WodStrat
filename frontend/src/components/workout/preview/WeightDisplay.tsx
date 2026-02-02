import React from 'react';
import type { WeightDisplayProps } from '../../../types/workoutPreview';

/**
 * Component for displaying weight with optional male/female differentiation
 */
export const WeightDisplay: React.FC<WeightDisplayProps> = ({
  loadValue,
  loadValueFemale,
  loadUnit,
  className = '',
}) => {
  if (loadValue === null) return null;

  // Format the unit (lowercase for display)
  const unit = loadUnit?.toLowerCase() || 'lb';

  // If we have both male and female values, show them together
  if (loadValueFemale !== null && loadValueFemale !== undefined && loadValueFemale !== loadValue) {
    return (
      <span className={`text-gray-600 ${className}`}>
        <span className="font-medium">{loadValue}/{loadValueFemale}</span>
        <span className="ml-0.5">{unit}</span>
        <span className="ml-1 text-xs text-gray-500">(RX)</span>
      </span>
    );
  }

  // Single weight value
  return (
    <span className={`text-gray-600 ${className}`}>
      <span className="font-medium">{loadValue}</span>
      <span className="ml-0.5">{unit}</span>
    </span>
  );
};
