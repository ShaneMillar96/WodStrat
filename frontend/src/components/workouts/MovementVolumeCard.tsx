import React, { useState } from 'react';
import type { MovementVolumeLoad } from '../../types/volumeLoad';
import { LoadClassificationBadge } from './LoadClassificationBadge';
import { ScalingRecommendation } from './ScalingRecommendation';

export interface MovementVolumeCardProps {
  /** Volume load data for the movement */
  movement: MovementVolumeLoad;
  /** Sequence number for display */
  sequenceNumber?: number;
  /** Whether to show scaling recommendation */
  showScalingTip?: boolean;
  /** Whether the card is expanded */
  defaultExpanded?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * MovementVolumeCard - Display volume load details for a single movement
 *
 * Features:
 * - Movement name with sequence number
 * - Volume calculation breakdown (weight x reps x rounds)
 * - Load classification badge
 * - Benchmark percentile indicator
 * - Expandable scaling recommendation
 */
export const MovementVolumeCard: React.FC<MovementVolumeCardProps> = ({
  movement,
  sequenceNumber,
  showScalingTip = true,
  defaultExpanded = false,
  className = '',
}) => {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  const hasScalingTip = movement.tip && movement.tip.length > 0;
  const showExpander = showScalingTip && hasScalingTip;

  return (
    <div
      className={`rounded-lg border border-gray-200 bg-white overflow-hidden ${className}`}
    >
      {/* Header */}
      <div
        className={`p-4 ${showExpander ? 'cursor-pointer hover:bg-gray-50' : ''}`}
        onClick={() => showExpander && setIsExpanded(!isExpanded)}
        role={showExpander ? 'button' : undefined}
        aria-expanded={showExpander ? isExpanded : undefined}
        tabIndex={showExpander ? 0 : undefined}
        onKeyDown={(e) => {
          if (showExpander && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            setIsExpanded(!isExpanded);
          }
        }}
      >
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1 min-w-0">
            {/* Movement name and sequence */}
            <div className="flex items-center gap-2">
              {sequenceNumber !== undefined && (
                <span className="text-sm text-gray-400 w-6 flex-shrink-0">
                  {sequenceNumber}.
                </span>
              )}
              <h4 className="font-medium text-gray-900 truncate">
                {movement.movementName}
              </h4>
            </div>

            {/* Volume calculation */}
            <div className="mt-1 text-sm text-gray-600">
              <span className="font-medium">{movement.weight} {movement.weightUnit}</span>
              {' x '}
              <span>{movement.reps} reps</span>
              {movement.rounds > 1 && (
                <>
                  {' x '}
                  <span>{movement.rounds} rounds</span>
                </>
              )}
            </div>

            {/* Benchmark info */}
            {movement.benchmarkUsed && (
              <div className="mt-1 text-xs text-gray-500">
                Based on: {movement.benchmarkUsed}
                {movement.athleteBenchmarkPercentile !== null && (
                  <span className="ml-2">
                    (Your percentile: {Math.round(movement.athleteBenchmarkPercentile * 100)}%)
                  </span>
                )}
              </div>
            )}
          </div>

          {/* Right side: volume and classification */}
          <div className="flex flex-col items-end gap-2 flex-shrink-0">
            <span className="text-lg font-bold text-gray-900">
              {movement.volumeLoadFormatted}
            </span>
            <LoadClassificationBadge
              classification={movement.loadClassification}
              size="sm"
            />
          </div>

          {/* Expand icon */}
          {showExpander && (
            <svg
              className={`h-5 w-5 text-gray-400 flex-shrink-0 transition-transform duration-200 ${
                isExpanded ? 'rotate-180' : ''
              }`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          )}
        </div>
      </div>

      {/* Expandable scaling recommendation */}
      {showExpander && isExpanded && (
        <div className="border-t border-gray-200 p-4 bg-gray-50">
          <ScalingRecommendation
            tip={movement.tip}
            recommendedWeight={movement.recommendedWeight}
            recommendedWeightFormatted={movement.recommendedWeightFormatted}
            classification={movement.loadClassification}
          />
        </div>
      )}
    </div>
  );
};
