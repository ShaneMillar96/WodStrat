import React from 'react';
import type { WorkoutVolumeLoadResult } from '../../types/volumeLoad';
import { MovementVolumeCard } from './MovementVolumeCard';
import { Badge } from '../ui';

export interface VolumeLoadSummaryProps {
  /** Volume load analysis result */
  volumeLoad: WorkoutVolumeLoadResult;
  /** Whether to show movement breakdown */
  showMovementBreakdown?: boolean;
  /** Whether to expand all movement cards by default */
  expandAllMovements?: boolean;
  /** Loading state indicator */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * VolumeLoadSummary - Complete volume load analysis display
 *
 * Features:
 * - Total volume load with formatted display
 * - Overall assessment text
 * - Optional per-movement breakdown using MovementVolumeCard
 * - Visual summary of load distribution
 * - Timestamp of calculation
 */
export const VolumeLoadSummary: React.FC<VolumeLoadSummaryProps> = ({
  volumeLoad,
  showMovementBreakdown = true,
  expandAllMovements = false,
  isLoading = false,
  className = '',
}) => {
  // Count movements by classification
  const classificationCounts = volumeLoad.movementVolumes.reduce(
    (acc, m) => {
      acc[m.loadClassification]++;
      return acc;
    },
    { High: 0, Moderate: 0, Low: 0 } as Record<string, number>
  );

  if (isLoading) {
    return (
      <div className={`animate-pulse ${className}`}>
        <div className="h-6 bg-gray-200 rounded w-1/3 mb-4" />
        <div className="h-24 bg-gray-200 rounded mb-4" />
        <div className="space-y-3">
          <div className="h-20 bg-gray-200 rounded" />
          <div className="h-20 bg-gray-200 rounded" />
        </div>
      </div>
    );
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">
          Volume Load Analysis
        </h3>
        <div className="text-xs text-gray-500">
          Calculated {new Date(volumeLoad.calculatedAt).toLocaleString()}
        </div>
      </div>

      {/* Summary Card */}
      <div className="rounded-lg border border-gray-200 bg-gradient-to-r from-gray-50 to-white p-5">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-500">Total Volume Load</p>
            <p className="text-3xl font-bold text-gray-900">
              {volumeLoad.totalVolumeLoadFormatted}
            </p>
          </div>

          {/* Classification distribution */}
          <div className="flex gap-2">
            {classificationCounts.High > 0 && (
              <Badge variant="error" size="sm">
                {classificationCounts.High} High
              </Badge>
            )}
            {classificationCounts.Moderate > 0 && (
              <Badge variant="warning" size="sm">
                {classificationCounts.Moderate} Moderate
              </Badge>
            )}
            {classificationCounts.Low > 0 && (
              <Badge variant="success" size="sm">
                {classificationCounts.Low} Low
              </Badge>
            )}
          </div>
        </div>

        {/* Overall assessment */}
        {volumeLoad.overallAssessment && (
          <div className="mt-4 pt-4 border-t border-gray-200">
            <p className="text-sm text-gray-700">
              <span className="font-medium">Assessment: </span>
              {volumeLoad.overallAssessment}
            </p>
          </div>
        )}
      </div>

      {/* Movement breakdown */}
      {showMovementBreakdown && volumeLoad.movementVolumes.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
            Per-Movement Breakdown
          </h4>
          <div className="space-y-3">
            {volumeLoad.movementVolumes.map((movement, index) => (
              <MovementVolumeCard
                key={movement.movementDefinitionId}
                movement={movement}
                sequenceNumber={index + 1}
                defaultExpanded={expandAllMovements}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
