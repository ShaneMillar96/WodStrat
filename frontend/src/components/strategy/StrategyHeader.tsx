import React from 'react';
import type { StrategyInsightsResult } from '../../types/strategyInsights';
import { DifficultyBar, ConfidenceBadge, Skeleton } from '../ui';

export interface StrategyHeaderProps {
  /** Workout name */
  workoutName: string;
  /** Strategy insights data */
  insights?: StrategyInsightsResult;
  /** Workout description (original text) */
  description?: string;
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * StrategyHeader - Top section of strategy page
 * Displays workout name, difficulty score, and confidence level
 */
export const StrategyHeader: React.FC<StrategyHeaderProps> = ({
  workoutName,
  insights,
  description,
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-6 ${className}`}>
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1">
            <Skeleton height={32} width="50%" className="mb-2" />
            <Skeleton height={20} width="75%" />
          </div>
          <Skeleton height={48} width={120} />
        </div>
        <div className="mt-4">
          <Skeleton height={16} width="40%" className="mb-2" />
          <Skeleton height={12} width="100%" />
        </div>
      </div>
    );
  }

  const difficulty = insights?.difficultyScore;
  const confidence = insights?.strategyConfidence;

  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-6 ${className}`}>
      {/* Top row: Workout name and confidence */}
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          <h1 className="text-2xl font-bold text-gray-900 truncate sm:text-3xl">
            {workoutName}
          </h1>
          {description && (
            <p className="mt-1 text-gray-600 line-clamp-2">{description}</p>
          )}
        </div>
        {confidence && (
          <div className="flex-shrink-0 text-right">
            <div className="text-xs text-gray-500 mb-1">Strategy Confidence</div>
            <ConfidenceBadge
              level={confidence.level}
              percentage={confidence.percentage}
              showPercentage
              size="lg"
            />
          </div>
        )}
      </div>

      {/* Difficulty score bar */}
      {difficulty && (
        <div className="mt-4 pt-4 border-t border-gray-100">
          <div className="text-sm font-medium text-gray-500 mb-2">
            Difficulty Rating
          </div>
          <DifficultyBar
            score={difficulty.score}
            label={difficulty.label}
            showScore
            showLabel
          />
          {difficulty.description && (
            <p className="mt-2 text-sm text-gray-600">{difficulty.description}</p>
          )}
        </div>
      )}
    </div>
  );
};
