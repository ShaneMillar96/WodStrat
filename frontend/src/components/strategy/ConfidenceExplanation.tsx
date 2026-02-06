import React from 'react';
import type { StrategyConfidence } from '../../types/strategyInsights';
import { ConfidenceBadge } from '../ui';

export interface ConfidenceExplanationProps {
  /** Strategy confidence data */
  confidence: StrategyConfidence;
  /** Additional CSS classes */
  className?: string;
}

/**
 * ConfidenceExplanation - Display confidence level with explanation text
 *
 * Features:
 * - Shows confidence badge with percentage
 * - Displays explanation text
 * - Lists missing benchmarks that would improve confidence
 */
export const ConfidenceExplanation: React.FC<ConfidenceExplanationProps> = ({
  confidence,
  className = '',
}) => {
  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-4 ${className}`}>
      <div className="flex items-start gap-3">
        <ConfidenceBadge
          level={confidence.level}
          percentage={confidence.percentage}
          showPercentage
          size="lg"
        />
        <div className="flex-1 min-w-0">
          <p className="text-sm text-gray-700">{confidence.explanation}</p>
          {confidence.missingBenchmarks.length > 0 && (
            <p className="text-xs text-gray-500 mt-2">
              Add benchmarks for improved accuracy: {confidence.missingBenchmarks.slice(0, 3).join(', ')}
              {confidence.missingBenchmarks.length > 3 && ` and ${confidence.missingBenchmarks.length - 3} more`}
            </p>
          )}
        </div>
      </div>
    </div>
  );
};
