import React from 'react';
import type { RepSchemeDisplayProps } from '../../../types/workoutPreview';
import { getRepSchemeLabel } from '../../../utils/workoutFormatters';
import { REP_SCHEME_COLORS } from '../../../utils/workoutStyles';

/**
 * Visual representation of rep scheme with boxes and arrows
 * Shows patterns like [21] -> [15] -> [9] for descending ladder
 */
export const RepSchemeDisplay: React.FC<RepSchemeDisplayProps> = ({
  scheme,
  compact = false,
  className = '',
}) => {
  const label = getRepSchemeLabel(scheme.type);
  const labelColor = REP_SCHEME_COLORS[scheme.type] || REP_SCHEME_COLORS.custom;

  if (compact) {
    // Compact mode: just show the pattern as text
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        <span className={`text-sm font-medium ${labelColor}`}>{label}:</span>
        <span className="text-sm text-gray-700">{scheme.reps.join('-')}</span>
      </div>
    );
  }

  return (
    <div className={`${className}`}>
      <div className={`text-xs font-medium mb-2 ${labelColor}`}>{label}</div>
      <div className="flex items-center gap-1 flex-wrap">
        {scheme.reps.map((rep, index) => (
          <React.Fragment key={index}>
            {index > 0 && (
              <span className="text-gray-400 mx-0.5" aria-hidden="true">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </span>
            )}
            <span
              className="inline-flex items-center justify-center min-w-[2rem] px-2 py-1 bg-gray-100 border border-gray-300 rounded text-sm font-medium text-gray-800"
              aria-label={`${rep} reps`}
            >
              {rep}
            </span>
          </React.Fragment>
        ))}
      </div>
    </div>
  );
};
