import React, { useState } from 'react';
import type { StrategyConfidenceLevel } from '../../types/strategyInsights';
import { Button } from '../ui';

export interface MissingBenchmarksCTAProps {
  /** List of missing benchmark names */
  missingBenchmarks: string[];
  /** Current confidence level */
  confidenceLevel: StrategyConfidenceLevel;
  /** Callback to navigate to benchmarks page */
  onAddBenchmarks?: () => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * MissingBenchmarksCTA - Call-to-action for adding missing benchmarks
 *
 * Features:
 * - Shows only when missingBenchmarks.length > 0
 * - Displays list of benchmark names that would improve confidence
 * - Expandable list for many benchmarks
 * - Button to navigate to add benchmarks page
 */
export const MissingBenchmarksCTA: React.FC<MissingBenchmarksCTAProps> = ({
  missingBenchmarks,
  confidenceLevel,
  onAddBenchmarks,
  className = '',
}) => {
  const [isExpanded, setIsExpanded] = useState(false);

  if (missingBenchmarks.length === 0) {
    return null;
  }

  const displayLimit = 5;
  const hasMore = missingBenchmarks.length > displayLimit;
  const displayedBenchmarks = isExpanded
    ? missingBenchmarks
    : missingBenchmarks.slice(0, displayLimit);

  // Get background color based on confidence level
  const bgColors: Record<StrategyConfidenceLevel, { bg: string; border: string; icon: string }> = {
    High: {
      bg: 'bg-green-50',
      border: 'border-green-200',
      icon: 'text-green-500',
    },
    Medium: {
      bg: 'bg-yellow-50',
      border: 'border-yellow-200',
      icon: 'text-yellow-500',
    },
    Low: {
      bg: 'bg-red-50',
      border: 'border-red-200',
      icon: 'text-red-500',
    },
  };

  const colors = bgColors[confidenceLevel];

  return (
    <div className={`rounded-lg border ${colors.border} ${colors.bg} p-4 ${className}`}>
      <div className="flex items-start gap-3">
        <div className={`flex-shrink-0 ${colors.icon}`}>
          <svg
            className="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>

        <div className="flex-1 min-w-0">
          <h4 className="text-sm font-medium text-gray-900">
            Improve Your Strategy Accuracy
          </h4>
          <p className="text-sm text-gray-600 mt-1">
            Add the following benchmarks to get more personalized recommendations:
          </p>

          <ul className="mt-2 space-y-1">
            {displayedBenchmarks.map((benchmark) => (
              <li
                key={benchmark}
                className="flex items-center gap-2 text-sm text-gray-700"
              >
                <svg
                  className="w-4 h-4 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 5l7 7-7 7"
                  />
                </svg>
                {benchmark}
              </li>
            ))}
          </ul>

          {hasMore && (
            <button
              type="button"
              onClick={() => setIsExpanded(!isExpanded)}
              className="mt-2 text-sm text-primary-600 hover:text-primary-700 font-medium"
            >
              {isExpanded
                ? 'Show less'
                : `Show ${missingBenchmarks.length - displayLimit} more`}
            </button>
          )}

          {onAddBenchmarks && (
            <div className="mt-3">
              <Button
                variant="primary"
                size="sm"
                onClick={onAddBenchmarks}
              >
                Add Benchmarks
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
