import React from 'react';
import type { TimeEstimateResult } from '../../types/timeEstimate';
import type { WorkoutVolumeLoadResult } from '../../types/volumeLoad';
import { Skeleton } from '../ui';

export interface QuickMetricsRowProps {
  /** Time estimate data */
  timeEstimate?: TimeEstimateResult;
  /** Volume load data */
  volumeLoad?: WorkoutVolumeLoadResult;
  /** Benchmark coverage count (from time estimate or other source) */
  benchmarkCoverageCount?: number;
  /** Total movement count */
  totalMovementCount?: number;
  /** Average percentile across movements */
  averagePercentile?: number | null;
  /** Loading states */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

interface MetricCardProps {
  label: string;
  value: string | React.ReactNode;
  subValue?: string;
  isLoading?: boolean;
}

const MetricCard: React.FC<MetricCardProps> = ({
  label,
  value,
  subValue,
  isLoading = false,
}) => {
  if (isLoading) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-4">
        <Skeleton height={12} width="60%" className="mb-2" />
        <Skeleton height={24} width="80%" />
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4">
      <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
        {label}
      </p>
      <p className="text-xl font-semibold text-gray-900">{value}</p>
      {subValue && (
        <p className="text-xs text-gray-500 mt-0.5">{subValue}</p>
      )}
    </div>
  );
};

/**
 * QuickMetricsRow - Horizontal row of key metrics at a glance
 *
 * Features:
 * - 4-column grid showing key metrics
 * - Time Estimate (formattedRange)
 * - Total Volume (totalVolumeLoadFormatted)
 * - Coverage (benchmarkCoverageCount / totalMovementCount)
 * - Average Percentile (averagePercentile)
 */
export const QuickMetricsRow: React.FC<QuickMetricsRowProps> = ({
  timeEstimate,
  volumeLoad,
  benchmarkCoverageCount,
  totalMovementCount,
  averagePercentile,
  isLoading = false,
  className = '',
}) => {
  // Calculate coverage string
  const coverageString = benchmarkCoverageCount !== undefined && totalMovementCount !== undefined
    ? `${benchmarkCoverageCount}/${totalMovementCount}`
    : volumeLoad?.movementVolumes
      ? `${volumeLoad.movementVolumes.filter((m) => m.benchmarkUsed).length}/${volumeLoad.movementVolumes.length}`
      : '-';

  // Format average percentile
  const percentileDisplay = averagePercentile !== null && averagePercentile !== undefined
    ? `${Math.round(averagePercentile)}%`
    : '-';

  // Get percentile color
  const getPercentileColor = (percentile: number | null | undefined): string => {
    if (percentile === null || percentile === undefined) return 'text-gray-900';
    if (percentile < 25) return 'text-red-600';
    if (percentile < 50) return 'text-yellow-600';
    if (percentile < 75) return 'text-green-600';
    return 'text-blue-600';
  };

  return (
    <div className={`grid grid-cols-2 lg:grid-cols-4 gap-3 ${className}`}>
      <MetricCard
        label="Time Estimate"
        value={timeEstimate?.formattedRange || '-'}
        subValue={timeEstimate?.confidenceLevel ? `${timeEstimate.confidenceLevel} confidence` : undefined}
        isLoading={isLoading}
      />
      <MetricCard
        label="Total Volume"
        value={volumeLoad?.totalVolumeLoadFormatted || '-'}
        isLoading={isLoading}
      />
      <MetricCard
        label="Benchmark Coverage"
        value={coverageString}
        subValue="movements with data"
        isLoading={isLoading}
      />
      <MetricCard
        label="Avg. Percentile"
        value={
          <span className={getPercentileColor(averagePercentile)}>
            {percentileDisplay}
          </span>
        }
        subValue={averagePercentile !== null && averagePercentile !== undefined ? 'vs. population' : undefined}
        isLoading={isLoading}
      />
    </div>
  );
};
