import React, { useState } from 'react';
import type { EmomFeasibility } from '../../types/timeEstimate';
import { EMOM_FEASIBILITY_COLORS } from '../../types/timeEstimate';

export interface EmomFeasibilityTimelineProps {
  /** Array of per-minute feasibility data */
  feasibility: EmomFeasibility[];
  /** Whether to show detailed view */
  showDetails?: boolean;
  /** Whether the component is in loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * EmomFeasibilityTimeline - Visual EMOM minute-by-minute feasibility
 *
 * Features:
 * - Horizontal timeline with color-coded minutes
 * - Green = feasible (> 10s buffer), Yellow = tight (< 10s buffer), Red = not feasible (> 60s)
 * - Expandable detail view per minute
 * - Summary statistics
 */
export const EmomFeasibilityTimeline: React.FC<EmomFeasibilityTimelineProps> = ({
  feasibility,
  showDetails = true,
  isLoading = false,
  className = '',
}) => {
  const [expandedMinute, setExpandedMinute] = useState<number | null>(null);

  if (isLoading) {
    return (
      <div className={`animate-pulse ${className}`}>
        <div className="h-6 bg-gray-200 rounded w-1/3 mb-4" />
        <div className="flex gap-1">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="h-12 w-12 bg-gray-200 rounded" />
          ))}
        </div>
      </div>
    );
  }

  // Categorize minutes by feasibility
  const getMinuteColors = (minute: EmomFeasibility) => {
    if (!minute.isFeasible) {
      return EMOM_FEASIBILITY_COLORS.notFeasible;
    }
    // Warning if buffer is less than 10 seconds
    if (minute.bufferSeconds < 10) {
      return EMOM_FEASIBILITY_COLORS.warning;
    }
    return EMOM_FEASIBILITY_COLORS.feasible;
  };

  const feasibleCount = feasibility.filter(m => m.isFeasible).length;
  const warningCount = feasibility.filter(m => m.isFeasible && m.bufferSeconds < 10).length;
  const notFeasibleCount = feasibility.filter(m => !m.isFeasible).length;

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header with summary */}
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">
          Minute-by-Minute Feasibility
        </h4>
        <div className="flex gap-2 text-xs">
          {feasibleCount > 0 && (
            <span className="text-green-600">{feasibleCount} feasible</span>
          )}
          {warningCount > 0 && (
            <span className="text-yellow-600">{warningCount} tight</span>
          )}
          {notFeasibleCount > 0 && (
            <span className="text-red-600">{notFeasibleCount} over</span>
          )}
        </div>
      </div>

      {/* Timeline */}
      <div className="flex flex-wrap gap-1">
        {feasibility.map((minute) => {
          const colors = getMinuteColors(minute);
          const isExpanded = expandedMinute === minute.minute;

          return (
            <button
              key={minute.minute}
              onClick={() => setExpandedMinute(isExpanded ? null : minute.minute)}
              className={`
                relative flex flex-col items-center justify-center
                w-12 h-12 rounded-lg border transition-all duration-200
                ${colors.bg} ${colors.border} ${colors.text}
                hover:scale-105 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-blue-500
                ${isExpanded ? 'ring-2 ring-blue-500' : ''}
              `}
              aria-label={`Minute ${minute.minute}: ${minute.isFeasible ? 'Feasible' : 'Not feasible'}`}
              aria-expanded={isExpanded}
            >
              <span className="text-xs font-medium">Min {minute.minute}</span>
              <span className="text-xs">{Math.round(minute.estimatedCompletionSeconds)}s</span>

              {/* Feasibility indicator dot */}
              <div className={`absolute -top-1 -right-1 w-3 h-3 rounded-full ${colors.indicator}`} />
            </button>
          );
        })}
      </div>

      {/* Expanded detail view */}
      {showDetails && expandedMinute !== null && (
        <div className="rounded-lg border border-gray-200 bg-gray-50 p-4">
          {feasibility
            .filter(m => m.minute === expandedMinute)
            .map(minute => {
              const colors = getMinuteColors(minute);
              return (
                <div key={minute.minute} className="space-y-2">
                  <div className="flex items-center justify-between">
                    <h5 className="font-medium text-gray-900">Minute {minute.minute}</h5>
                    <span className={`px-2 py-0.5 rounded text-xs font-medium ${colors.bg} ${colors.text}`}>
                      {minute.isFeasible ? 'Feasible' : 'Not Feasible'}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-gray-500">Prescribed Work</p>
                      <p className="font-medium text-gray-900">{minute.prescribedWork}</p>
                    </div>
                    <div>
                      <p className="text-gray-500">Estimated Time</p>
                      <p className="font-medium text-gray-900">{minute.estimatedCompletionSeconds}s</p>
                    </div>
                    <div>
                      <p className="text-gray-500">Buffer</p>
                      <p className={`font-medium ${minute.bufferSeconds >= 10 ? 'text-green-600' : minute.bufferSeconds >= 0 ? 'text-yellow-600' : 'text-red-600'}`}>
                        {minute.bufferSeconds >= 0 ? `${minute.bufferSeconds}s rest` : `${Math.abs(minute.bufferSeconds)}s over`}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-500">Recommendation</p>
                      <p className="font-medium text-gray-900">{minute.recommendation}</p>
                    </div>
                  </div>
                </div>
              );
            })}
        </div>
      )}

      {/* Legend */}
      <div className="flex gap-4 text-xs text-gray-500">
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-green-500" />
          <span>Feasible (&gt;10s buffer)</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-yellow-500" />
          <span>Tight (&lt;10s buffer)</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-red-500" />
          <span>Over time</span>
        </div>
      </div>
    </div>
  );
};
