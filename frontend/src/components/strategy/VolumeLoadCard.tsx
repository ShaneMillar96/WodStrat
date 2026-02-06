import React from 'react';
import type { WorkoutVolumeLoadResult } from '../../types/volumeLoad';
import { SectionSkeleton, SectionError, Badge } from '../ui';

export interface VolumeLoadCardProps {
  /** Volume load data */
  volumeLoad?: WorkoutVolumeLoadResult;
  /** Loading state */
  isLoading?: boolean;
  /** Error state */
  error?: Error | null;
  /** Additional CSS classes */
  className?: string;
}

/**
 * VolumeLoadCard - Compact card showing volume load summary
 */
export const VolumeLoadCard: React.FC<VolumeLoadCardProps> = ({
  volumeLoad,
  isLoading = false,
  error,
  className = '',
}) => {
  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={2} className={className} />;
  }

  if (error || !volumeLoad) {
    return (
      <SectionError
        title="Volume Load"
        message="Add benchmark data to see personalized volume analysis."
        className={className}
      />
    );
  }

  // Count classifications
  const counts = volumeLoad.movementVolumes.reduce(
    (acc, m) => {
      acc[m.loadClassification]++;
      return acc;
    },
    { High: 0, Moderate: 0, Low: 0 } as Record<string, number>
  );

  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-5 ${className}`}>
      <div className="flex items-center gap-2 mb-3">
        <span className="text-xl" role="img" aria-label="Chart">&#x1F4CA;</span>
        <h3 className="font-semibold text-gray-900">Volume Load</h3>
      </div>

      <div className="text-center py-2">
        <p className="text-3xl font-bold text-gray-900">
          {volumeLoad.totalVolumeLoadFormatted}
        </p>
        <p className="text-sm text-gray-500 mt-1">total volume</p>
      </div>

      {/* Classification summary */}
      <div className="flex justify-center gap-2 mt-3 pt-3 border-t border-gray-100">
        {counts.High > 0 && (
          <Badge variant="error" size="sm">{counts.High} High</Badge>
        )}
        {counts.Moderate > 0 && (
          <Badge variant="warning" size="sm">{counts.Moderate} Mod</Badge>
        )}
        {counts.Low > 0 && (
          <Badge variant="success" size="sm">{counts.Low} Low</Badge>
        )}
      </div>
    </div>
  );
};
