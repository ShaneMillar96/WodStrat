import React from 'react';
import { Badge } from '../ui';
import { TOTAL_BENCHMARK_COUNT } from '../../types';

export interface BenchmarkSummaryProps {
  /** Number of recorded benchmarks */
  recordedCount: number;
  /** Total number of benchmarks */
  totalCount?: number;
  /** Whether the minimum requirement is met */
  meetsMinimumRequirement: boolean;
  /** Minimum required benchmarks */
  minimumRequired: number;
  /** Whether data is loading */
  isLoading?: boolean;
}

/**
 * Header component showing benchmark completion summary
 * Displays X/23 recorded and minimum requirement status
 */
export const BenchmarkSummary: React.FC<BenchmarkSummaryProps> = ({
  recordedCount,
  totalCount = TOTAL_BENCHMARK_COUNT,
  meetsMinimumRequirement,
  minimumRequired,
  isLoading = false,
}) => {
  const percentComplete = Math.round((recordedCount / totalCount) * 100);

  if (isLoading) {
    return (
      <div className="mb-6 rounded-lg border border-gray-200 bg-white p-4">
        <div className="animate-pulse space-y-3">
          <div className="h-6 w-48 rounded bg-gray-200" />
          <div className="h-4 w-32 rounded bg-gray-200" />
          <div className="h-2 w-full rounded bg-gray-200" />
        </div>
      </div>
    );
  }

  return (
    <div className="mb-6 rounded-lg border border-gray-200 bg-white p-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        {/* Progress info */}
        <div>
          <h2 className="text-lg font-semibold text-gray-900">
            Benchmarks Recorded
          </h2>
          <p className="mt-1 text-sm text-gray-600">
            <span className="font-medium text-gray-900">
              {recordedCount} of {totalCount}
            </span>{' '}
            benchmarks completed ({percentComplete}%)
          </p>
        </div>

        {/* Requirement status */}
        <div className="flex items-center gap-2">
          {meetsMinimumRequirement ? (
            <Badge variant="success" size="lg" rounded>
              <svg
                className="mr-1.5 h-4 w-4"
                fill="currentColor"
                viewBox="0 0 20 20"
                aria-hidden="true"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                  clipRule="evenodd"
                />
              </svg>
              Minimum met
            </Badge>
          ) : (
            <Badge variant="warning" size="lg" rounded>
              <svg
                className="mr-1.5 h-4 w-4"
                fill="currentColor"
                viewBox="0 0 20 20"
                aria-hidden="true"
              >
                <path
                  fillRule="evenodd"
                  d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z"
                  clipRule="evenodd"
                />
              </svg>
              Need {minimumRequired - recordedCount} more
            </Badge>
          )}
        </div>
      </div>

      {/* Progress bar */}
      <div className="mt-4">
        <div className="h-2 w-full overflow-hidden rounded-full bg-gray-200">
          <div
            className={`h-full rounded-full transition-all duration-300 ${
              meetsMinimumRequirement ? 'bg-green-500' : 'bg-yellow-500'
            }`}
            style={{ width: `${percentComplete}%` }}
            role="progressbar"
            aria-valuenow={recordedCount}
            aria-valuemin={0}
            aria-valuemax={totalCount}
            aria-label={`${recordedCount} of ${totalCount} benchmarks completed`}
          />
        </div>
        <p className="mt-1 text-xs text-gray-500">
          Minimum {minimumRequired} benchmarks required for workout recommendations
        </p>
      </div>
    </div>
  );
};
