import React from 'react';
import type { VolumeSummaryProps } from '../../../types/workoutPreview';
import { VolumeBar } from './VolumeBar';
import { getMaxVolume } from '../../../utils/volumeCalculations';

/**
 * Summary panel showing total volume per movement with optional visual bars
 */
export const VolumeSummary: React.FC<VolumeSummaryProps> = ({
  volumes,
  showBars = true,
  className = '',
}) => {
  if (volumes.length === 0) {
    return null;
  }

  const maxReps = getMaxVolume(volumes);

  return (
    <div className={`bg-gray-50 rounded-lg p-4 ${className}`}>
      <h4 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wide">
        Volume Summary
      </h4>
      <div className="space-y-3">
        {volumes.map((volume, index) => (
          <div key={index}>
            {showBars ? (
              <VolumeBar
                movementName={volume.movementName}
                totalReps={volume.totalReps}
                maxReps={maxReps}
              />
            ) : (
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-700">{volume.movementName}</span>
                <span className="font-medium text-gray-900">{volume.totalReps} reps</span>
              </div>
            )}
            {/* Show distance if available */}
            {volume.totalDistance !== undefined && (
              <div className="text-xs text-gray-500 mt-0.5 ml-0">
                + {volume.totalDistance} total distance
              </div>
            )}
            {/* Show calories if available */}
            {volume.totalCalories !== undefined && (
              <div className="text-xs text-gray-500 mt-0.5 ml-0">
                + {volume.totalCalories} total cal
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};
