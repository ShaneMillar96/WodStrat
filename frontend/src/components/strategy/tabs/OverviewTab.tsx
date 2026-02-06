import React from 'react';
import type { StrategyInsightsResult } from '../../../types/strategyInsights';
import type { WorkoutPacingResult } from '../../../types/pacing';
import { KeyFocusSection } from '../KeyFocusSection';
import { AlertsSection } from '../AlertsSection';
import { ConfidenceExplanation } from '../ConfidenceExplanation';
import { SectionSkeleton } from '../../ui';

export interface OverviewTabProps {
  /** Strategy insights data */
  insights?: StrategyInsightsResult;
  /** Pacing data for strategy notes */
  pacing?: WorkoutPacingResult;
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * OverviewTab - Display overview content including key focus, alerts, and confidence
 *
 * Features:
 * - Shows KeyFocusSection (existing)
 * - Shows AlertsSection (existing, with alertType display)
 * - Shows confidence explanation text from strategyConfidence.explanation
 * - Shows overall strategy notes from pacing data
 */
export const OverviewTab: React.FC<OverviewTabProps> = ({
  insights,
  pacing,
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return (
      <div className={`space-y-4 ${className}`}>
        <SectionSkeleton titleWidth="w-1/3" lines={4} />
        <SectionSkeleton titleWidth="w-1/4" lines={3} />
        <SectionSkeleton titleWidth="w-1/2" lines={2} />
      </div>
    );
  }

  const hasContent =
    insights?.keyFocusMovements?.length ||
    insights?.riskAlerts?.length ||
    insights?.strategyConfidence ||
    pacing?.overallStrategyNotes;

  if (!hasContent) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-6 text-center ${className}`}>
        <p className="text-gray-500">No overview data available.</p>
      </div>
    );
  }

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Confidence Explanation - Show prominently at top */}
      {insights?.strategyConfidence && (
        <ConfidenceExplanation confidence={insights.strategyConfidence} />
      )}

      {/* Key Focus Section */}
      <KeyFocusSection
        movements={insights?.keyFocusMovements}
        isLoading={false}
      />

      {/* Alerts Section */}
      <AlertsSection
        alerts={insights?.riskAlerts}
        isLoading={false}
      />

      {/* Overall Strategy Notes */}
      {pacing?.overallStrategyNotes && (
        <div className="rounded-lg border border-gray-200 bg-white p-5">
          <div className="flex items-center gap-2 mb-3">
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
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <h3 className="font-semibold text-gray-900">Strategy Notes</h3>
          </div>
          <p className="text-gray-700">{pacing.overallStrategyNotes}</p>
        </div>
      )}
    </div>
  );
};
