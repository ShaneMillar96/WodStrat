import React, { useState } from 'react';
import type { WorkoutVolumeLoadResult, MovementVolumeLoad } from '../../../types/volumeLoad';
import { SectionSkeleton, PercentileIndicator, Badge } from '../../ui';
import { LoadClassificationBadge } from '../../workouts';

export interface VolumeTabProps {
  /** Volume load data */
  volumeLoad?: WorkoutVolumeLoadResult;
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

interface MovementVolumeCardProps {
  movement: MovementVolumeLoad;
  isExpanded: boolean;
  onToggle: () => void;
}

const MovementVolumeCard: React.FC<MovementVolumeCardProps> = ({
  movement,
  isExpanded,
  onToggle,
}) => {
  return (
    <div className="border-b border-gray-100 last:border-b-0">
      {/* Row header - clickable */}
      <button
        onClick={onToggle}
        className="w-full px-4 py-3 flex items-center justify-between hover:bg-gray-50 transition-colors text-left"
        aria-expanded={isExpanded}
        aria-controls={`volume-details-${movement.movementDefinitionId}`}
      >
        <div className="flex items-center gap-3 min-w-0 flex-1">
          <span className="font-medium text-gray-900 truncate">
            {movement.movementName}
          </span>
        </div>

        <div className="flex items-center gap-3 flex-shrink-0">
          <span className="text-sm font-medium text-gray-900 tabular-nums">
            {movement.volumeLoadFormatted}
          </span>
          <LoadClassificationBadge
            classification={movement.loadClassification}
            size="sm"
            showLabel={false}
          />
          <svg
            className={`h-5 w-5 text-gray-400 transition-transform ${
              isExpanded ? 'rotate-180' : ''
            }`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </div>
      </button>

      {/* Expanded content */}
      {isExpanded && (
        <div
          id={`volume-details-${movement.movementDefinitionId}`}
          className="px-4 pb-4 bg-gray-50 space-y-3"
        >
          {/* Volume Details */}
          <div>
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
              Volume Details
            </div>
            <p className="text-sm text-gray-700">
              {movement.weight} {movement.weightUnit} x {movement.reps} reps
              {movement.rounds > 1 && ` x ${movement.rounds} rounds`}
            </p>
          </div>

          {/* Athlete Percentile */}
          <div className="flex items-center gap-4">
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Your Percentile
              </div>
              <PercentileIndicator
                percentile={movement.athleteBenchmarkPercentile}
                size="md"
                showValue
              />
            </div>
          </div>

          {/* Benchmark Used */}
          {movement.benchmarkUsed && (
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Based On
              </div>
              <p className="text-sm text-gray-600">{movement.benchmarkUsed}</p>
            </div>
          )}

          {/* Recommended Weight / Scaling */}
          {movement.recommendedWeight !== null && movement.recommendedWeightFormatted && (
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Recommended Scaling
              </div>
              <div className="flex items-center gap-2">
                <Badge variant="blue" size="sm" rounded>
                  Scale to {movement.recommendedWeightFormatted}
                </Badge>
              </div>
            </div>
          )}

          {/* Tip */}
          {movement.tip && (
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Tip
              </div>
              <p className="text-sm text-primary-700 font-medium">{movement.tip}</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

/**
 * VolumeTab - Display volume breakdown with scaling recommendations
 *
 * Features:
 * - Shows overallAssessment text at top
 * - List of movements with:
 *   - Volume load formatted
 *   - Load classification badge
 *   - athleteBenchmarkPercentile with visual indicator
 *   - benchmarkUsed text
 *   - recommendedWeight / recommendedWeightFormatted scaling recommendation
 *   - tip text
 */
export const VolumeTab: React.FC<VolumeTabProps> = ({
  volumeLoad,
  isLoading = false,
  className = '',
}) => {
  const [expandedMovement, setExpandedMovement] = useState<number | null>(null);

  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={5} className={className} />;
  }

  if (!volumeLoad?.movementVolumes || volumeLoad.movementVolumes.length === 0) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-6 text-center ${className}`}>
        <p className="text-gray-500">No volume data available.</p>
        <p className="text-sm text-gray-400 mt-1">
          This workout may not have weighted movements.
        </p>
      </div>
    );
  }

  const toggleMovement = (movementId: number) => {
    setExpandedMovement(expandedMovement === movementId ? null : movementId);
  };

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Overall Assessment */}
      {volumeLoad.overallAssessment && (
        <div className="rounded-lg border border-gray-200 bg-white p-4">
          <div className="flex items-start gap-3">
            <svg
              className="w-5 h-5 text-orange-500 flex-shrink-0 mt-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
              />
            </svg>
            <div>
              <h4 className="text-sm font-medium text-gray-900">Overall Assessment</h4>
              <p className="text-sm text-gray-700 mt-1">{volumeLoad.overallAssessment}</p>
            </div>
          </div>
        </div>
      )}

      {/* Movement Volume List */}
      <div className="rounded-lg border border-gray-200 bg-white overflow-hidden">
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-gray-900">Movement Volume</h3>
            <span className="text-sm text-gray-600">
              Total: {volumeLoad.totalVolumeLoadFormatted}
            </span>
          </div>
        </div>

        <div className="divide-y divide-gray-100">
          {volumeLoad.movementVolumes.map((movement) => (
            <MovementVolumeCard
              key={movement.movementDefinitionId}
              movement={movement}
              isExpanded={expandedMovement === movement.movementDefinitionId}
              onToggle={() => toggleMovement(movement.movementDefinitionId)}
            />
          ))}
        </div>
      </div>
    </div>
  );
};
