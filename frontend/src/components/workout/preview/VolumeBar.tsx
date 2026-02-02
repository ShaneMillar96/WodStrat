import React from 'react';
import type { VolumeBarProps } from '../../../types/workoutPreview';
import { calculateVolumePercentage } from '../../../utils/volumeCalculations';

/**
 * Individual volume bar showing reps for a movement with visual bar chart
 */
export const VolumeBar: React.FC<VolumeBarProps> = ({
  movementName,
  totalReps,
  maxReps,
  className = '',
}) => {
  const percentage = calculateVolumePercentage(totalReps, maxReps);

  return (
    <div className={`${className}`}>
      <div className="flex justify-between items-center mb-1">
        <span className="text-sm text-gray-700 truncate" title={movementName}>
          {movementName}
        </span>
        <span className="text-sm font-medium text-gray-900 ml-2 whitespace-nowrap">
          {totalReps} reps
        </span>
      </div>
      <div
        className="h-2 bg-gray-200 rounded-full overflow-hidden"
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`${movementName}: ${totalReps} reps (${percentage}% of max)`}
      >
        <div
          className="h-full bg-blue-500 transition-all duration-300"
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
};
