import React from 'react';
import type { WorkoutHeaderProps } from '../../../types/workoutPreview';
import { WorkoutTypeTag } from '../../workouts/WorkoutTypeTag';
import { TimeDomainBadge } from './TimeDomainBadge';
import { ConfidenceIndicator } from './ConfidenceIndicator';
import type { WorkoutType } from '../../../types/workout';

/**
 * Header section for the parsed workout preview
 * Shows workout type badge, time domain badge, and confidence indicator
 */
export const WorkoutHeader: React.FC<WorkoutHeaderProps> = ({
  workoutType,
  timeDomain,
  confidence,
  name,
  parsedDescription,
  errorCount = 0,
  unrecognizedCount = 0,
  totalMovements = 0,
  className = '',
}) => {
  return (
    <div className={`${className}`}>
      {/* Top row: badges and confidence */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-2 flex-wrap">
          {/* Workout type badge */}
          <WorkoutTypeTag type={workoutType as WorkoutType} size="lg" />

          {/* Time domain badge */}
          <TimeDomainBadge domain={timeDomain} size="md" />
        </div>

        {/* Confidence indicator */}
        <ConfidenceIndicator
          score={confidence}
          showTooltip={true}
          showLabel={true}
          size="md"
          errorCount={errorCount}
          unrecognizedCount={unrecognizedCount}
          totalMovements={totalMovements}
        />
      </div>

      {/* Optional name */}
      {name && (
        <h3 className="text-lg font-semibold text-gray-900 mt-3">{name}</h3>
      )}

      {/* Optional parsed description */}
      {parsedDescription && (
        <p className="text-gray-600 mt-2">{parsedDescription}</p>
      )}
    </div>
  );
};
