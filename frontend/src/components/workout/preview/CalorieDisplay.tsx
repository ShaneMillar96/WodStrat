import React from 'react';
import type { CalorieDisplayProps } from '../../../types/workoutPreview';

/**
 * Component for displaying calorie count with optional male/female differentiation
 */
export const CalorieDisplay: React.FC<CalorieDisplayProps> = ({
  calories,
  caloriesFemale,
  className = '',
}) => {
  if (calories === null) return null;

  // If we have both male and female values, show them together
  if (caloriesFemale !== null && caloriesFemale !== undefined && caloriesFemale !== calories) {
    return (
      <span className={`text-gray-600 ${className}`}>
        <span className="font-medium">{calories}/{caloriesFemale}</span>
        <span className="ml-0.5">cal</span>
      </span>
    );
  }

  // Single calorie value
  return (
    <span className={`text-gray-600 ${className}`}>
      <span className="font-medium">{calories}</span>
      <span className="ml-0.5">cal</span>
    </span>
  );
};
