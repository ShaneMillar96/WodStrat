import React from 'react';
import type { RestRecommendation } from '../../types/timeEstimate';

export interface RestRecommendationCardProps {
  /** Rest recommendation data */
  recommendation: RestRecommendation;
  /** Sequence number for display */
  sequenceNumber?: number;
  /** Whether to show in compact mode */
  compact?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * RestRecommendationCard - Display rest recommendation callout
 *
 * Features:
 * - Shows movement after which to rest
 * - Displays suggested rest duration
 * - Explains reasoning for the recommendation
 * - Compact mode for inline display
 */
export const RestRecommendationCard: React.FC<RestRecommendationCardProps> = ({
  recommendation,
  sequenceNumber,
  compact = false,
  className = '',
}) => {
  if (compact) {
    return (
      <div className={`flex items-center gap-2 text-sm ${className}`}>
        <svg
          className="h-4 w-4 flex-shrink-0 text-blue-500"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <span className="text-gray-600">
          Rest {recommendation.restRange} after {recommendation.afterMovement}
        </span>
      </div>
    );
  }

  return (
    <div className={`rounded-lg border border-blue-200 bg-blue-50 p-4 ${className}`}>
      <div className="flex items-start gap-3">
        {/* Icon */}
        <div className="flex-shrink-0 mt-0.5">
          <svg
            className="h-5 w-5 text-blue-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M8 12h.01M12 12h.01M16 12h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>

        <div className="flex-1">
          {/* Header */}
          <div className="flex items-center gap-2">
            {sequenceNumber !== undefined && (
              <span className="text-xs text-blue-500 font-medium">#{sequenceNumber}</span>
            )}
            <p className="text-sm font-medium text-blue-800">
              Rest after {recommendation.afterMovement}
            </p>
          </div>

          {/* Duration */}
          <div className="mt-2 flex items-center gap-2">
            <span className="text-lg font-bold text-blue-900">
              {recommendation.restRange}
            </span>
            <span className="text-sm text-blue-600">
              ({recommendation.suggestedRestSeconds} seconds)
            </span>
          </div>

          {/* Reasoning */}
          <p className="mt-2 text-sm text-blue-700">
            {recommendation.reasoning}
          </p>
        </div>
      </div>
    </div>
  );
};
