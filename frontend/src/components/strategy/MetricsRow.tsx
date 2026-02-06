import React from 'react';
import { TimeEstimateCard } from './TimeEstimateCard';
import { VolumeLoadCard } from './VolumeLoadCard';
import type { TimeEstimateResult } from '../../types/timeEstimate';
import type { WorkoutVolumeLoadResult } from '../../types/volumeLoad';

export interface MetricsRowProps {
  /** Time estimate data */
  timeEstimate?: TimeEstimateResult;
  /** Volume load data */
  volumeLoad?: WorkoutVolumeLoadResult;
  /** Loading states */
  isTimeEstimateLoading?: boolean;
  isVolumeLoadLoading?: boolean;
  /** Errors */
  timeEstimateError?: Error | null;
  volumeLoadError?: Error | null;
  /** Additional CSS classes */
  className?: string;
}

/**
 * MetricsRow - Side-by-side time estimate and volume load cards
 */
export const MetricsRow: React.FC<MetricsRowProps> = ({
  timeEstimate,
  volumeLoad,
  isTimeEstimateLoading = false,
  isVolumeLoadLoading = false,
  timeEstimateError,
  volumeLoadError,
  className = '',
}) => {
  return (
    <div className={`grid gap-4 md:grid-cols-2 ${className}`}>
      <TimeEstimateCard
        timeEstimate={timeEstimate}
        isLoading={isTimeEstimateLoading}
        error={timeEstimateError}
      />
      <VolumeLoadCard
        volumeLoad={volumeLoad}
        isLoading={isVolumeLoadLoading}
        error={volumeLoadError}
      />
    </div>
  );
};
