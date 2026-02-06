import React, { useState } from 'react';
import type { WorkoutPacingResult, MovementPacing } from '../../../types/pacing';
import { PacingBadge, SectionSkeleton, PercentileIndicator } from '../../ui';

export interface PacingTabProps {
  /** Pacing data */
  pacing?: WorkoutPacingResult;
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

interface MovementPacingCardProps {
  movement: MovementPacing;
  isExpanded: boolean;
  onToggle: () => void;
}

const MovementPacingCard: React.FC<MovementPacingCardProps> = ({
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
        aria-controls={`pacing-details-${movement.movementDefinitionId}`}
      >
        <div className="flex items-center gap-3 min-w-0 flex-1">
          <span className="font-medium text-gray-900 truncate">
            {movement.movementName}
          </span>
        </div>

        <div className="flex items-center gap-3 flex-shrink-0">
          <PacingBadge pacingLevel={movement.pacingLevel} size="sm" />
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
          id={`pacing-details-${movement.movementDefinitionId}`}
          className="px-4 pb-4 bg-gray-50 space-y-3"
        >
          {/* Guidance text */}
          <div>
            <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
              Guidance
            </div>
            <p className="text-sm text-gray-700">{movement.guidanceText}</p>
          </div>

          {/* Athlete Percentile */}
          <div className="flex items-center gap-4">
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Your Percentile
              </div>
              <PercentileIndicator
                percentile={movement.athletePercentile}
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

          {/* Target Pace (cardio movements) */}
          {movement.isCardio && movement.targetPace && (
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Target Pace
              </div>
              <div className="flex items-baseline gap-2">
                <span className="text-lg font-semibold text-gray-900">
                  {movement.targetPace.displayPrimary}
                </span>
                {movement.targetPace.displaySecondary && (
                  <span className="text-sm text-gray-500">
                    ({movement.targetPace.displaySecondary})
                  </span>
                )}
              </div>
            </div>
          )}

          {/* Recommended Sets (non-cardio movements) */}
          {!movement.isCardio && movement.recommendedSets && movement.recommendedSets.length > 0 && (
            <div>
              <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                Recommended Sets
              </div>
              <div className="flex items-center gap-1.5">
                {movement.recommendedSets.map((reps, index) => (
                  <React.Fragment key={index}>
                    <span className="inline-flex items-center justify-center min-w-[28px] h-7 rounded-md bg-primary-100 text-primary-700 text-sm font-medium px-2">
                      {reps}
                    </span>
                    {index < movement.recommendedSets.length - 1 && (
                      <span className="text-gray-400">-</span>
                    )}
                  </React.Fragment>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

/**
 * PacingTab - Display detailed pacing information for each movement
 *
 * Features:
 * - List of movements with expandable details
 * - Shows pacingLevel badge (existing)
 * - Shows athletePercentile with visual indicator
 * - Shows benchmarkUsed text
 * - Shows guidanceText and recommendedSets
 */
export const PacingTab: React.FC<PacingTabProps> = ({
  pacing,
  isLoading = false,
  className = '',
}) => {
  const [expandedMovement, setExpandedMovement] = useState<number | null>(null);

  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={5} className={className} />;
  }

  if (!pacing?.movementPacing || pacing.movementPacing.length === 0) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-6 text-center ${className}`}>
        <p className="text-gray-500">No pacing data available.</p>
      </div>
    );
  }

  const toggleMovement = (movementId: number) => {
    setExpandedMovement(expandedMovement === movementId ? null : movementId);
  };

  return (
    <div className={`rounded-lg border border-gray-200 bg-white overflow-hidden ${className}`}>
      <div className="p-4 border-b border-gray-200 bg-gray-50">
        <h3 className="font-semibold text-gray-900">Movement Pacing</h3>
        <p className="text-sm text-gray-600 mt-1">
          Personalized pacing recommendations based on your benchmarks
        </p>
      </div>

      <div className="divide-y divide-gray-100">
        {pacing.movementPacing.map((movement) => (
          <MovementPacingCard
            key={movement.movementDefinitionId}
            movement={movement}
            isExpanded={expandedMovement === movement.movementDefinitionId}
            onToggle={() => toggleMovement(movement.movementDefinitionId)}
          />
        ))}
      </div>
    </div>
  );
};
