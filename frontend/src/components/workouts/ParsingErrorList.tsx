import React from 'react';
import type { ParsingIssue, ParsingIssueSeverity } from '../../types/parsingError';

export interface ParsingErrorListProps {
  /** List of parsing errors to display */
  errors: ParsingIssue[];
  /** Callback when a line number is clicked */
  onLineClick?: (lineNumber: number) => void;
  /** Callback when "Apply fix" is clicked for a suggestion */
  onApplySuggestion?: (issue: ParsingIssue, lineNumber: number | null) => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Severity icon components
 */
const SeverityIcons: Record<ParsingIssueSeverity, React.FC<{ className?: string }>> = {
  error: ({ className }) => (
    <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
      <path
        fillRule="evenodd"
        d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
        clipRule="evenodd"
      />
    </svg>
  ),
  warning: ({ className }) => (
    <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
      <path
        fillRule="evenodd"
        d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z"
        clipRule="evenodd"
      />
    </svg>
  ),
  info: ({ className }) => (
    <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
      <path
        fillRule="evenodd"
        d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a.75.75 0 000 1.5h.253a.25.25 0 01.244.304l-.459 2.066A1.75 1.75 0 0010.747 15H11a.75.75 0 000-1.5h-.253a.25.25 0 01-.244-.304l.459-2.066A1.75 1.75 0 009.253 9H9z"
        clipRule="evenodd"
      />
    </svg>
  ),
};

/**
 * Styles for each severity level
 */
const severityStyles: Record<ParsingIssueSeverity, { container: string; icon: string }> = {
  error: {
    container: 'border-red-200 bg-red-50',
    icon: 'text-red-500',
  },
  warning: {
    container: 'border-yellow-200 bg-yellow-50',
    icon: 'text-yellow-500',
  },
  info: {
    container: 'border-blue-200 bg-blue-50',
    icon: 'text-blue-500',
  },
};

/**
 * Priority order for sorting by severity
 */
const severityPriority: Record<ParsingIssueSeverity, number> = {
  error: 0,
  warning: 1,
  info: 2,
};

/**
 * Individual error item component
 */
interface ParsingErrorItemProps {
  issue: ParsingIssue;
  onLineClick?: (lineNumber: number) => void;
  onApplySuggestion?: (issue: ParsingIssue, lineNumber: number | null) => void;
}

const ParsingErrorItem: React.FC<ParsingErrorItemProps> = ({
  issue,
  onLineClick,
  onApplySuggestion,
}) => {
  const styles = severityStyles[issue.severity];
  const Icon = SeverityIcons[issue.severity];

  const handleLineClick = () => {
    if (issue.line !== null && onLineClick) {
      onLineClick(issue.line);
    }
  };

  const handleApplyFix = () => {
    if (onApplySuggestion && issue.suggestion) {
      onApplySuggestion(issue, issue.line);
    }
  };

  return (
    <div
      className={`flex items-start gap-3 p-3 rounded-md border ${styles.container}`}
      role="alert"
    >
      <Icon className={`h-5 w-5 flex-shrink-0 mt-0.5 ${styles.icon}`} />
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          {issue.line !== null && (
            <button
              type="button"
              onClick={handleLineClick}
              className="inline-flex items-center px-2 py-0.5 text-xs font-medium rounded cursor-pointer bg-gray-100 text-gray-700 hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-gray-400"
              aria-label={`Go to line ${issue.line}`}
            >
              Line {issue.line}
            </button>
          )}
          <span className="text-sm text-gray-900 font-medium">{issue.message}</span>
        </div>
        {issue.suggestion && (
          <div className="mt-1 flex items-center gap-2 flex-wrap">
            <span className="text-sm text-gray-600 italic">
              Suggestion: {issue.suggestion}
            </span>
            {onApplySuggestion && (
              <button
                type="button"
                onClick={handleApplyFix}
                className="text-xs text-primary-600 hover:text-primary-800 underline cursor-pointer focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-primary-400"
              >
                Apply fix
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

/**
 * ParsingErrorList - Displays parsing errors with severity, line links, and suggestions
 *
 * Features:
 * - Groups errors by severity (errors first, then warnings)
 * - Shows severity icon (X for error, warning triangle for warning)
 * - Displays clickable line numbers that scroll/focus to that line
 * - Shows suggestion text with optional "Apply fix" button
 */
export const ParsingErrorList: React.FC<ParsingErrorListProps> = ({
  errors,
  onLineClick,
  onApplySuggestion,
  className = '',
}) => {
  // Return null if no errors
  if (!errors || errors.length === 0) {
    return null;
  }

  // Sort errors by severity (error > warning > info)
  const sortedErrors = [...errors].sort(
    (a, b) => severityPriority[a.severity] - severityPriority[b.severity]
  );

  return (
    <div className={`space-y-2 ${className}`} role="region" aria-label="Parsing errors">
      {sortedErrors.map((issue, index) => (
        <ParsingErrorItem
          key={`${issue.code}-${issue.line ?? 'global'}-${index}`}
          issue={issue}
          onLineClick={onLineClick}
          onApplySuggestion={onApplySuggestion}
        />
      ))}
    </div>
  );
};
