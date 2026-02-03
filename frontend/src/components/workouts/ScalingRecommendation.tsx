import React from 'react';
import type { LoadClassification } from '../../types/volumeLoad';
import { LOAD_CLASSIFICATION_COLORS } from '../../types/volumeLoad';

export interface ScalingRecommendationProps {
  /** The actionable tip text */
  tip: string;
  /** Recommended weight value (null if RX is appropriate) */
  recommendedWeight: number | null;
  /** Formatted recommended weight string */
  recommendedWeightFormatted: string | null;
  /** Load classification for context coloring */
  classification: LoadClassification;
  /** Whether to show in compact mode */
  compact?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * ScalingRecommendation - Display scaling tip and recommended weight
 *
 * Features:
 * - Shows actionable tip text
 * - Displays recommended weight when scaling is suggested
 * - Color-coded background based on load classification
 * - Compact mode for inline display
 */
export const ScalingRecommendation: React.FC<ScalingRecommendationProps> = ({
  tip,
  recommendedWeight: _recommendedWeight,
  recommendedWeightFormatted,
  classification,
  compact = false,
  className = '',
}) => {
  const colors = LOAD_CLASSIFICATION_COLORS[classification];

  if (compact) {
    return (
      <div className={`flex items-center gap-2 text-sm ${className}`}>
        <svg
          className={`h-4 w-4 flex-shrink-0 ${colors.text}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <span className="text-gray-600">{tip}</span>
      </div>
    );
  }

  return (
    <div
      className={`rounded-md p-3 ${colors.bg} ${colors.border} border ${className}`}
    >
      <div className="flex items-start gap-3">
        <svg
          className={`h-5 w-5 flex-shrink-0 mt-0.5 ${colors.text}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
          />
        </svg>
        <div className="flex-1">
          <p className={`text-sm font-medium ${colors.text}`}>Scaling Tip</p>
          <p className="mt-1 text-sm text-gray-700">{tip}</p>
          {recommendedWeightFormatted && (
            <p className="mt-2 text-sm font-medium text-gray-900">
              Recommended: {recommendedWeightFormatted}
            </p>
          )}
        </div>
      </div>
    </div>
  );
};
