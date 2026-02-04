import React, { useState } from 'react';
import type { TimeEstimateResult, EmomFeasibility } from '../../types/timeEstimate';
import { ConfidenceLevelIndicator } from './ConfidenceLevelIndicator';
import { TimeRangeDisplay } from './TimeRangeDisplay';
import { AmrapEstimateDisplay } from './AmrapEstimateDisplay';
import { EmomFeasibilityTimeline } from './EmomFeasibilityTimeline';
import { RestRecommendationCard } from './RestRecommendationCard';

export interface TimeEstimateSummaryProps {
  /** Time estimate result data */
  timeEstimate: TimeEstimateResult;
  /** EMOM feasibility data (only for EMOM workouts) */
  emomFeasibility?: EmomFeasibility[];
  /** Time cap in seconds (for visual context) */
  timeCapSeconds?: number | null;
  /** Whether to show rest recommendations */
  showRestRecommendations?: boolean;
  /** Whether to expand rest recommendations by default */
  expandRestRecommendations?: boolean;
  /** Loading state indicator */
  isLoading?: boolean;
  /** Loading state for EMOM feasibility */
  isLoadingEmom?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * TimeEstimateSummary - Complete time estimate display
 *
 * Features:
 * - Workout-type-specific estimate display
 * - Confidence level indicator
 * - Factors summary
 * - EMOM feasibility timeline (for EMOM workouts)
 * - Collapsible rest recommendations section
 * - Calculation timestamp
 */
export const TimeEstimateSummary: React.FC<TimeEstimateSummaryProps> = ({
  timeEstimate,
  emomFeasibility,
  timeCapSeconds,
  showRestRecommendations = true,
  expandRestRecommendations = false,
  isLoading = false,
  isLoadingEmom = false,
  className = '',
}) => {
  const [showRest, setShowRest] = useState(expandRestRecommendations);

  const hasRestRecommendations = timeEstimate.restRecommendations?.length > 0;
  const isEmom = timeEstimate.workoutType === 'Emom';
  const isAmrap = timeEstimate.workoutType === 'Amrap';
  const isForTime = timeEstimate.workoutType === 'ForTime';

  if (isLoading) {
    return (
      <div className={`animate-pulse ${className}`}>
        <div className="h-6 bg-gray-200 rounded w-1/3 mb-4" />
        <div className="h-32 bg-gray-200 rounded mb-4" />
        <div className="h-16 bg-gray-200 rounded" />
      </div>
    );
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">
          Time Estimate
        </h3>
        <div className="flex items-center gap-3">
          <ConfidenceLevelIndicator level={timeEstimate.confidenceLevel} size="sm" />
          <div className="text-xs text-gray-500">
            Calculated {new Date(timeEstimate.calculatedAt).toLocaleString()}
          </div>
        </div>
      </div>

      {/* Main Estimate Card */}
      <div className="rounded-lg border border-gray-200 bg-gradient-to-r from-gray-50 to-white p-5">
        {/* Workout-type-specific display */}
        {isForTime && (
          <TimeRangeDisplay
            minSeconds={timeEstimate.minEstimate}
            maxSeconds={timeEstimate.maxEstimate}
            formattedRange={timeEstimate.formattedRange}
            timeCapSeconds={timeCapSeconds}
          />
        )}

        {isAmrap && (
          <AmrapEstimateDisplay
            minEstimate={timeEstimate.minEstimate}
            maxEstimate={timeEstimate.maxEstimate}
            formattedRange={timeEstimate.formattedRange}
            timeCapSeconds={timeCapSeconds}
          />
        )}

        {isEmom && (
          <div className="text-center">
            <p className="text-2xl font-bold text-gray-900">{timeEstimate.formattedRange}</p>
            <p className="text-sm text-gray-500 mt-1">EMOM completion estimate</p>
          </div>
        )}

        {/* Factors summary */}
        {timeEstimate.factorsSummary && (
          <div className="mt-4 pt-4 border-t border-gray-200">
            <p className="text-sm text-gray-700">
              <span className="font-medium">Factors: </span>
              {timeEstimate.factorsSummary}
            </p>
          </div>
        )}
      </div>

      {/* EMOM Feasibility Timeline */}
      {isEmom && emomFeasibility && emomFeasibility.length > 0 && (
        <EmomFeasibilityTimeline
          feasibility={emomFeasibility}
          isLoading={isLoadingEmom}
        />
      )}

      {/* Rest Recommendations */}
      {showRestRecommendations && hasRestRecommendations && (
        <div>
          <button
            onClick={() => setShowRest(!showRest)}
            className="flex items-center gap-2 w-full text-left text-sm font-semibold text-gray-700 uppercase tracking-wide hover:text-gray-900"
          >
            <svg
              className={`h-4 w-4 transition-transform duration-200 ${showRest ? 'rotate-180' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
            Rest Recommendations ({timeEstimate.restRecommendations.length})
          </button>

          {showRest && (
            <div className="mt-3 space-y-3">
              {timeEstimate.restRecommendations.map((rec, index) => (
                <RestRecommendationCard
                  key={index}
                  recommendation={rec}
                  sequenceNumber={index + 1}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
