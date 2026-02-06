import React from 'react';
import type { TimeEstimateResult, EmomFeasibility } from '../../../types/timeEstimate';
import type { WorkoutType } from '../../../types/workout';
import { CONFIDENCE_LEVEL_COLORS } from '../../../types/timeEstimate';
import { SectionSkeleton, Badge } from '../../ui';
import { RestRecommendationsList } from '../RestRecommendationsList';
import { EmomFeasibilityTable } from '../EmomFeasibilityTable';
import { AmrapExtraRepsDisplay } from '../AmrapExtraRepsDisplay';

export interface TimingTabProps {
  /** Time estimate data */
  timeEstimate?: TimeEstimateResult;
  /** EMOM feasibility data (may be separate or included in timeEstimate) */
  emomFeasibility?: EmomFeasibility[];
  /** Workout type for conditional rendering */
  workoutType: WorkoutType;
  /** Benchmark coverage count */
  benchmarkCoverageCount?: number;
  /** Total movement count */
  totalMovementCount?: number;
  /** Average percentile */
  averagePercentile?: number | null;
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * TimingTab - Display time estimates, rest recommendations, and EMOM feasibility
 *
 * Features:
 * - Shows time range with confidence level
 * - Shows estimateType indicator (Time vs RoundsReps)
 * - Shows benchmarkCoverageCount / totalMovementCount coverage
 * - Shows averagePercentile indicator
 *
 * Conditional Rendering:
 * - ForTime workouts: Show RestRecommendationsList
 * - EMOM workouts: Show EmomFeasibilityTable
 * - AMRAP workouts: Show AmrapExtraRepsDisplay with minExtraReps/maxExtraReps
 */
export const TimingTab: React.FC<TimingTabProps> = ({
  timeEstimate,
  emomFeasibility,
  workoutType,
  benchmarkCoverageCount,
  totalMovementCount,
  averagePercentile,
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={5} className={className} />;
  }

  if (!timeEstimate) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-6 text-center ${className}`}>
        <p className="text-gray-500">No timing data available.</p>
      </div>
    );
  }

  const confidenceColors = CONFIDENCE_LEVEL_COLORS[timeEstimate.confidenceLevel];
  const isAmrap = workoutType === 'Amrap';
  const isEmom = workoutType === 'Emom';
  const isForTime = workoutType === 'ForTime';

  // For AMRAP, try to extract extra reps from the formatted range if available
  // The API returns something like "5 rounds + 12 reps to 6 rounds + 3 reps"
  const amrapMinExtraReps = (timeEstimate as TimeEstimateResult & { minExtraReps?: number }).minExtraReps;
  const amrapMaxExtraReps = (timeEstimate as TimeEstimateResult & { maxExtraReps?: number }).maxExtraReps;
  const hasAmrapExtraReps =
    isAmrap &&
    amrapMinExtraReps !== undefined &&
    amrapMinExtraReps !== null &&
    amrapMaxExtraReps !== undefined &&
    amrapMaxExtraReps !== null;

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Time Estimate Card */}
      <div className={`rounded-lg border p-4 ${confidenceColors.border} ${confidenceColors.bg}`}>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h4 className="text-sm font-medium text-gray-500 uppercase tracking-wide">
              {timeEstimate.estimateType === 'RoundsReps' ? 'Estimated Score' : 'Estimated Time'}
            </h4>
            <p className={`text-2xl font-bold mt-1 ${confidenceColors.text}`}>
              {timeEstimate.formattedRange}
            </p>
            <div className="flex items-center gap-2 mt-2">
              <Badge
                variant={
                  timeEstimate.confidenceLevel === 'High'
                    ? 'success'
                    : timeEstimate.confidenceLevel === 'Medium'
                      ? 'warning'
                      : 'error'
                }
                size="sm"
                rounded
              >
                {timeEstimate.confidenceLevel} Confidence
              </Badge>
              <Badge variant="gray" size="sm" rounded>
                {timeEstimate.estimateType === 'RoundsReps' ? 'Rounds + Reps' : 'Time'}
              </Badge>
            </div>
          </div>

          {/* Coverage Stats */}
          {(benchmarkCoverageCount !== undefined || averagePercentile !== undefined) && (
            <div className="text-right text-sm">
              {benchmarkCoverageCount !== undefined && totalMovementCount !== undefined && (
                <p className="text-gray-600">
                  <span className="font-medium">{benchmarkCoverageCount}/{totalMovementCount}</span>{' '}
                  movements covered
                </p>
              )}
              {averagePercentile !== undefined && averagePercentile !== null && (
                <p className="text-gray-600 mt-1">
                  Avg. percentile:{' '}
                  <span className="font-medium">{Math.round(averagePercentile)}%</span>
                </p>
              )}
            </div>
          )}
        </div>

        {/* Factors Summary */}
        {timeEstimate.factorsSummary && (
          <p className="text-sm text-gray-700 mt-3 pt-3 border-t border-gray-200">
            {timeEstimate.factorsSummary}
          </p>
        )}
      </div>

      {/* Conditional Rendering based on workout type */}

      {/* AMRAP: Show extra reps display */}
      {hasAmrapExtraReps && (
        <AmrapExtraRepsDisplay
          minExtraReps={amrapMinExtraReps!}
          maxExtraReps={amrapMaxExtraReps!}
          formattedRange={timeEstimate.formattedRange}
        />
      )}

      {/* EMOM: Show feasibility table */}
      {isEmom && emomFeasibility && emomFeasibility.length > 0 && (
        <EmomFeasibilityTable
          feasibility={emomFeasibility}
          isLoading={false}
        />
      )}

      {/* ForTime: Show rest recommendations */}
      {isForTime && timeEstimate.restRecommendations && timeEstimate.restRecommendations.length > 0 && (
        <RestRecommendationsList
          recommendations={timeEstimate.restRecommendations}
          compact={false}
        />
      )}

      {/* Show rest recommendations for other workout types if available */}
      {!isForTime &&
        !isEmom &&
        timeEstimate.restRecommendations &&
        timeEstimate.restRecommendations.length > 0 && (
          <RestRecommendationsList
            recommendations={timeEstimate.restRecommendations}
            compact={true}
          />
        )}
    </div>
  );
};
