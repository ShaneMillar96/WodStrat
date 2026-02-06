import React from 'react';
import type { RestRecommendation } from '../../types/timeEstimate';

export interface RestRecommendationsListProps {
  /** Array of rest recommendations */
  recommendations: RestRecommendation[];
  /** Whether to show compact view */
  compact?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * RestRecommendationsList - Display list of rest recommendations for ForTime workouts
 *
 * Features:
 * - Shows after-movement rest recommendations
 * - Displays rest duration and reasoning
 * - Compact or full view mode
 */
export const RestRecommendationsList: React.FC<RestRecommendationsListProps> = ({
  recommendations,
  compact = false,
  className = '',
}) => {
  if (recommendations.length === 0) {
    return null;
  }

  return (
    <div className={`rounded-lg border border-gray-200 bg-white ${className}`}>
      <div className="p-4 border-b border-gray-200 bg-gray-50">
        <div className="flex items-center gap-2">
          <svg
            className="w-5 h-5 text-gray-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <h4 className="font-medium text-gray-900">Rest Recommendations</h4>
        </div>
      </div>

      <div className="divide-y divide-gray-100">
        {recommendations.map((rec, index) => (
          <div
            key={`${rec.afterMovement}-${index}`}
            className="p-4"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900">
                  After {rec.afterMovement}
                </p>
                {!compact && rec.reasoning && (
                  <p className="text-sm text-gray-600 mt-1">{rec.reasoning}</p>
                )}
              </div>
              <div className="flex-shrink-0 text-right">
                <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-sm font-medium text-green-800">
                  {rec.restRange}
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
