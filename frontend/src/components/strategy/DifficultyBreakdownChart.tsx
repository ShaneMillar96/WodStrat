import React from 'react';
import type { DifficultyBreakdown } from '../../types/strategyInsights';

export interface DifficultyBreakdownChartProps {
  /** Breakdown of difficulty factors */
  breakdown: DifficultyBreakdown;
  /** Overall difficulty score */
  totalScore: number;
  /** Additional CSS classes */
  className?: string;
}

interface FactorConfig {
  key: keyof DifficultyBreakdown;
  label: string;
  color: string;
  bgColor: string;
  textColor: string;
}

const FACTOR_CONFIGS: FactorConfig[] = [
  {
    key: 'pacingFactor',
    label: 'Pacing',
    color: 'bg-blue-500',
    bgColor: 'bg-blue-50',
    textColor: 'text-blue-700',
  },
  {
    key: 'volumeFactor',
    label: 'Volume',
    color: 'bg-orange-500',
    bgColor: 'bg-orange-50',
    textColor: 'text-orange-700',
  },
  {
    key: 'timeFactor',
    label: 'Time',
    color: 'bg-purple-500',
    bgColor: 'bg-purple-50',
    textColor: 'text-purple-700',
  },
  {
    key: 'experienceModifier',
    label: 'Experience',
    color: 'bg-gray-500',
    bgColor: 'bg-gray-50',
    textColor: 'text-gray-700',
  },
];

/**
 * DifficultyBreakdownChart - Visualize the difficulty score breakdown
 *
 * Features:
 * - Horizontal stacked bar showing each factor's contribution
 * - Color-coded segments: pacing (blue), volume (orange), time (purple), experience (gray)
 * - Labels showing each factor's value
 */
export const DifficultyBreakdownChart: React.FC<DifficultyBreakdownChartProps> = ({
  breakdown,
  totalScore,
  className = '',
}) => {
  // Calculate the sum of absolute values for the stacked bar
  const absSum =
    Math.abs(breakdown.pacingFactor) +
    Math.abs(breakdown.volumeFactor) +
    Math.abs(breakdown.timeFactor) +
    Math.abs(breakdown.experienceModifier);

  // If all factors are 0, show an empty state
  if (absSum === 0) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-4 ${className}`}>
        <h4 className="text-sm font-medium text-gray-700 mb-3">Difficulty Breakdown</h4>
        <p className="text-sm text-gray-500">No difficulty data available</p>
      </div>
    );
  }

  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-4 ${className}`}>
      <div className="flex items-center justify-between mb-3">
        <h4 className="text-sm font-medium text-gray-700">Difficulty Breakdown</h4>
        <span className="text-sm font-semibold text-gray-900">
          Score: {totalScore.toFixed(1)}/10
        </span>
      </div>

      {/* Stacked bar chart */}
      <div
        className="h-6 rounded-full overflow-hidden flex bg-gray-100"
        role="img"
        aria-label={`Difficulty breakdown: Pacing ${breakdown.pacingFactor.toFixed(1)}, Volume ${breakdown.volumeFactor.toFixed(1)}, Time ${breakdown.timeFactor.toFixed(1)}, Experience modifier ${breakdown.experienceModifier.toFixed(1)}`}
      >
        {FACTOR_CONFIGS.map((config) => {
          const value = breakdown[config.key];
          const absValue = Math.abs(value);
          const widthPercent = (absValue / absSum) * 100;

          if (widthPercent < 1) return null;

          return (
            <div
              key={config.key}
              className={`${config.color} flex items-center justify-center transition-all duration-300`}
              style={{ width: `${widthPercent}%` }}
              title={`${config.label}: ${value.toFixed(1)}`}
            >
              {widthPercent >= 15 && (
                <span className="text-xs font-medium text-white truncate px-1">
                  {value.toFixed(1)}
                </span>
              )}
            </div>
          );
        })}
      </div>

      {/* Legend */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 mt-3">
        {FACTOR_CONFIGS.map((config) => {
          const value = breakdown[config.key];
          return (
            <div
              key={config.key}
              className={`flex items-center gap-2 rounded px-2 py-1 ${config.bgColor}`}
            >
              <div className={`w-2.5 h-2.5 rounded-sm ${config.color}`} />
              <div className="flex-1 min-w-0">
                <span className={`text-xs font-medium ${config.textColor}`}>
                  {config.label}
                </span>
                <span className={`text-xs ${config.textColor} ml-1 opacity-75`}>
                  {value >= 0 ? '+' : ''}
                  {value.toFixed(1)}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
