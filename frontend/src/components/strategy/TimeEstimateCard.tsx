import React from 'react';
import type { TimeEstimateResult } from '../../types/timeEstimate';
import { SectionSkeleton, SectionError } from '../ui';
import { ConfidenceLevelIndicator } from '../workouts';

export interface TimeEstimateCardProps {
  /** Time estimate data */
  timeEstimate?: TimeEstimateResult;
  /** Loading state */
  isLoading?: boolean;
  /** Error state */
  error?: Error | null;
  /** Additional CSS classes */
  className?: string;
}

/**
 * TimeEstimateCard - Compact card showing time estimate
 */
export const TimeEstimateCard: React.FC<TimeEstimateCardProps> = ({
  timeEstimate,
  isLoading = false,
  error,
  className = '',
}) => {
  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/2" lines={2} className={className} />;
  }

  if (error || !timeEstimate) {
    return (
      <SectionError
        title="Time Estimate"
        message="Add benchmark data to see personalized time estimates."
        className={className}
      />
    );
  }

  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-5 ${className}`}>
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <span className="text-xl" role="img" aria-label="Stopwatch">&#x23F1;</span>
          <h3 className="font-semibold text-gray-900">Time Estimate</h3>
        </div>
        <ConfidenceLevelIndicator level={timeEstimate.confidenceLevel} size="sm" />
      </div>

      <div className="text-center py-2">
        <p className="text-3xl font-bold text-gray-900">
          {timeEstimate.formattedRange}
        </p>
        <p className="text-sm text-gray-500 mt-1">
          {timeEstimate.workoutType} estimate
        </p>
      </div>

      {timeEstimate.factorsSummary && (
        <p className="text-xs text-gray-600 mt-3 pt-3 border-t border-gray-100">
          {timeEstimate.factorsSummary}
        </p>
      )}
    </div>
  );
};
