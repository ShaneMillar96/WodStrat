import React from 'react';
import type { DifficultyLabel } from '../../types/strategyInsights';
import { DIFFICULTY_LABEL_BADGE_VARIANTS } from '../../types/strategyInsights';
import { Badge } from './Badge';

export interface DifficultyBarProps {
  /** Difficulty score (1-10) */
  score: number;
  /** Difficulty label */
  label: DifficultyLabel;
  /** Whether to show the numeric score */
  showScore?: boolean;
  /** Whether to show the label badge */
  showLabel?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * DifficultyBar - Visual progress bar for difficulty score
 *
 * Features:
 * - 10-segment progress bar with color gradient
 * - Optional numeric score display
 * - Optional label badge
 */
export const DifficultyBar: React.FC<DifficultyBarProps> = ({
  score,
  label,
  showScore = true,
  showLabel = true,
  className = '',
}) => {
  // Clamp score between 1 and 10
  const clampedScore = Math.max(1, Math.min(10, score));

  // Determine color based on score
  const getSegmentColor = (index: number) => {
    if (index >= clampedScore) return 'bg-gray-200';
    if (clampedScore <= 3) return 'bg-green-500';
    if (clampedScore <= 5) return 'bg-blue-500';
    if (clampedScore <= 7) return 'bg-yellow-500';
    return 'bg-red-500';
  };

  return (
    <div className={`flex items-center gap-3 ${className}`}>
      {/* Progress bar */}
      <div className="flex gap-0.5 flex-1">
        {Array.from({ length: 10 }).map((_, index) => (
          <div
            key={index}
            className={`h-3 flex-1 rounded-sm transition-colors ${getSegmentColor(index)}`}
          />
        ))}
      </div>

      {/* Score display */}
      {showScore && (
        <span className="text-lg font-bold text-gray-900 w-12 text-right">
          {clampedScore}/10
        </span>
      )}

      {/* Label badge */}
      {showLabel && (
        <Badge variant={DIFFICULTY_LABEL_BADGE_VARIANTS[label]} size="sm" rounded>
          {label}
        </Badge>
      )}
    </div>
  );
};
