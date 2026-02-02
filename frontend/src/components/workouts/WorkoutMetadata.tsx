import React from 'react';
import type { Workout, ParsedWorkout } from '../../types/workout';

export interface WorkoutMetadataProps {
  /** Workout data (saved or parsed) */
  workout: Workout | ParsedWorkout;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Display workout metadata including time cap, round count, and interval duration
 */
export const WorkoutMetadata: React.FC<WorkoutMetadataProps> = ({
  workout,
  className = '',
}) => {
  const items: string[] = [];

  if (workout.timeCapFormatted) {
    items.push(workout.timeCapFormatted);
  }

  if (workout.roundCount) {
    items.push(`${workout.roundCount} rounds`);
  }

  if (workout.intervalDurationFormatted) {
    items.push(`Every ${workout.intervalDurationFormatted}`);
  }

  if (items.length === 0) {
    return null;
  }

  return (
    <div className={`flex items-center gap-2 text-sm font-medium text-gray-600 ${className}`}>
      {items.map((item, index) => (
        <React.Fragment key={index}>
          {index > 0 && <span className="text-gray-300">|</span>}
          <span>{item}</span>
        </React.Fragment>
      ))}
    </div>
  );
};
