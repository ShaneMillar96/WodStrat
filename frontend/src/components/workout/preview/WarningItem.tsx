import React from 'react';
import type { WarningItemProps } from '../../../types/workoutPreview';
import { SEVERITY_LABELS } from '../../../types/parsingError';

/**
 * Badge variant colors for severity levels
 */
const SEVERITY_COLORS = {
  error: 'bg-red-100 text-red-800',
  warning: 'bg-yellow-100 text-yellow-800',
  info: 'bg-blue-100 text-blue-800',
};

/**
 * Individual warning display with optional "Apply fix" action
 */
export const WarningItem: React.FC<WarningItemProps> = ({
  warning,
  index,
  onApplySuggestion,
  className = '',
}) => {
  const severityColor = SEVERITY_COLORS[warning.severity] || SEVERITY_COLORS.info;
  const severityLabel = SEVERITY_LABELS[warning.severity];

  const handleApplySuggestion = () => {
    if (onApplySuggestion && warning.suggestion) {
      onApplySuggestion(index, warning.suggestion);
    }
  };

  return (
    <div
      className={`flex items-start gap-3 p-3 bg-white rounded-lg border border-gray-200 ${className}`}
    >
      {/* Severity badge */}
      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${severityColor}`}>
        {severityLabel}
      </span>

      {/* Message content */}
      <div className="flex-1 min-w-0">
        <p className="text-sm text-gray-700">{warning.message}</p>
        {warning.line && (
          <p className="text-xs text-gray-500 mt-0.5">
            Line {warning.line}
          </p>
        )}
        {warning.suggestion && (
          <p className="text-xs text-gray-500 mt-1">
            <span className="font-medium">Suggestion:</span> {warning.suggestion}
          </p>
        )}
      </div>

      {/* Apply fix button */}
      {warning.suggestion && onApplySuggestion && (
        <button
          type="button"
          onClick={handleApplySuggestion}
          className="flex-shrink-0 text-xs font-medium text-blue-600 hover:text-blue-800 hover:underline focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded"
        >
          Apply fix
        </button>
      )}
    </div>
  );
};
