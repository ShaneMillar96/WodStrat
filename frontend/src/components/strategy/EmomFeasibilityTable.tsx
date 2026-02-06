import React from 'react';
import type { EmomFeasibility } from '../../types/timeEstimate';
import { EMOM_FEASIBILITY_COLORS } from '../../types/timeEstimate';

export interface EmomFeasibilityTableProps {
  /** EMOM feasibility data */
  feasibility: EmomFeasibility[];
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Get feasibility status for styling
 */
const getFeasibilityStatus = (item: EmomFeasibility): 'feasible' | 'warning' | 'notFeasible' => {
  if (!item.isFeasible) return 'notFeasible';
  if (item.bufferSeconds < 10) return 'warning';
  return 'feasible';
};

/**
 * Format seconds to MM:SS display
 */
const formatSeconds = (seconds: number): string => {
  const mins = Math.floor(seconds / 60);
  const secs = Math.round(seconds % 60);
  if (mins > 0) {
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }
  return `${secs}s`;
};

/**
 * EmomFeasibilityTable - Table view of EMOM feasibility data
 *
 * Features:
 * - Table with columns: Minute, Work, Est. Time, Buffer, Status, Recommendation
 * - Row coloring based on feasibility status
 * - Complementary to existing EmomFeasibilityTimeline (visual vs tabular)
 */
export const EmomFeasibilityTable: React.FC<EmomFeasibilityTableProps> = ({
  feasibility,
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white overflow-hidden ${className}`}>
        <div className="p-4 border-b border-gray-200 bg-gray-50">
          <div className="h-5 w-1/3 bg-gray-200 rounded animate-pulse" />
        </div>
        <div className="p-4 space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-10 bg-gray-100 rounded animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  if (feasibility.length === 0) {
    return null;
  }

  // Calculate summary stats
  const feasibleCount = feasibility.filter((f) => f.isFeasible).length;
  const warningCount = feasibility.filter(
    (f) => f.isFeasible && f.bufferSeconds < 10
  ).length;
  const notFeasibleCount = feasibility.filter((f) => !f.isFeasible).length;

  return (
    <div className={`rounded-lg border border-gray-200 bg-white overflow-hidden ${className}`}>
      {/* Header */}
      <div className="p-4 border-b border-gray-200 bg-gray-50">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <svg
              className="w-5 h-5 text-purple-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
              />
            </svg>
            <h4 className="font-medium text-gray-900">EMOM Minute-by-Minute Analysis</h4>
          </div>
          <div className="flex items-center gap-3 text-xs">
            <span className="flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-green-500" />
              <span className="text-gray-600">{feasibleCount} feasible</span>
            </span>
            {warningCount > 0 && (
              <span className="flex items-center gap-1">
                <span className="w-2 h-2 rounded-full bg-yellow-500" />
                <span className="text-gray-600">{warningCount} tight</span>
              </span>
            )}
            {notFeasibleCount > 0 && (
              <span className="flex items-center gap-1">
                <span className="w-2 h-2 rounded-full bg-red-500" />
                <span className="text-gray-600">{notFeasibleCount} at risk</span>
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
              >
                Min
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
              >
                Work
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
              >
                Est. Time
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
              >
                Buffer
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
              >
                Status
              </th>
              <th
                scope="col"
                className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider hidden sm:table-cell"
              >
                Recommendation
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {feasibility.map((item) => {
              const status = getFeasibilityStatus(item);
              const colors = EMOM_FEASIBILITY_COLORS[status];

              return (
                <tr key={item.minute} className={colors.bg}>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span className="text-sm font-medium text-gray-900">
                      {item.minute}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-sm text-gray-900">{item.prescribedWork}</span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span className="text-sm text-gray-900 tabular-nums">
                      {formatSeconds(item.estimatedCompletionSeconds)}
                    </span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span className={`text-sm font-medium tabular-nums ${colors.text}`}>
                      {item.bufferSeconds > 0 ? '+' : ''}
                      {formatSeconds(item.bufferSeconds)}
                    </span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium ${colors.bg} ${colors.text} ${colors.border} border`}
                    >
                      <span className={`w-1.5 h-1.5 rounded-full ${colors.indicator}`} />
                      {status === 'feasible' && 'OK'}
                      {status === 'warning' && 'Tight'}
                      {status === 'notFeasible' && 'At Risk'}
                    </span>
                  </td>
                  <td className="px-4 py-3 hidden sm:table-cell">
                    <span className="text-sm text-gray-600">{item.recommendation}</span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
};
