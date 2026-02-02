import React from 'react';
import type { WarningsListProps } from '../../../types/workoutPreview';
import { WarningItem } from './WarningItem';

/**
 * Container component for displaying a list of parsing warnings
 */
export const WarningsList: React.FC<WarningsListProps> = ({
  warnings,
  onApplySuggestion,
  className = '',
}) => {
  if (warnings.length === 0) {
    return null;
  }

  // Group warnings by severity
  const errors = warnings.filter(w => w.severity === 'error');
  const warningsOnly = warnings.filter(w => w.severity === 'warning');
  const infos = warnings.filter(w => w.severity === 'info');

  return (
    <div className={`${className}`}>
      <h4 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wide flex items-center gap-2">
        <svg className="w-4 h-4 text-yellow-500" fill="currentColor" viewBox="0 0 20 20">
          <path
            fillRule="evenodd"
            d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
            clipRule="evenodd"
          />
        </svg>
        Parsing Issues ({warnings.length})
      </h4>

      <div className="space-y-2">
        {/* Errors first */}
        {errors.map((warning, idx) => (
          <WarningItem
            key={`error-${idx}`}
            warning={warning}
            index={warnings.indexOf(warning)}
            onApplySuggestion={onApplySuggestion}
          />
        ))}

        {/* Then warnings */}
        {warningsOnly.map((warning, idx) => (
          <WarningItem
            key={`warning-${idx}`}
            warning={warning}
            index={warnings.indexOf(warning)}
            onApplySuggestion={onApplySuggestion}
          />
        ))}

        {/* Then info */}
        {infos.map((warning, idx) => (
          <WarningItem
            key={`info-${idx}`}
            warning={warning}
            index={warnings.indexOf(warning)}
            onApplySuggestion={onApplySuggestion}
          />
        ))}
      </div>
    </div>
  );
};
