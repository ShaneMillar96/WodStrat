import React, { useState, useMemo } from 'react';
import type { ParsingIssue } from '../../types/parsingError';

export interface ParsingWarningBannerProps {
  /** List of warnings to display */
  warnings: ParsingIssue[];
  /** Callback when banner is dismissed */
  onDismiss?: () => void;
  /** Callback when "Fix all" is clicked */
  onFixAll?: () => void;
  /** Callback when individual warning is dismissed */
  onDismissWarning?: (index: number) => void;
  /** Whether fixes can be auto-applied */
  canAutoFix?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Warning icon component
 */
const WarningIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
    <path
      fillRule="evenodd"
      d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z"
      clipRule="evenodd"
    />
  </svg>
);

/**
 * Chevron icon for expand/collapse
 */
const ChevronIcon: React.FC<{ className?: string; expanded?: boolean }> = ({
  className,
  expanded = false,
}) => (
  <svg
    className={`${className} transition-transform duration-200 ${expanded ? 'rotate-180' : ''}`}
    viewBox="0 0 20 20"
    fill="currentColor"
    aria-hidden="true"
  >
    <path
      fillRule="evenodd"
      d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z"
      clipRule="evenodd"
    />
  </svg>
);

/**
 * Close/X icon
 */
const CloseIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg className={className} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
    <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
  </svg>
);

/**
 * ParsingWarningBanner - Collapsible banner for non-blocking warnings
 *
 * Features:
 * - Collapsible summary (e.g., "3 warnings found")
 * - Individual warning items with dismiss buttons
 * - "Dismiss all" and "Fix all" action buttons
 * - Smooth expand/collapse animation
 */
export const ParsingWarningBanner: React.FC<ParsingWarningBannerProps> = ({
  warnings,
  onDismiss,
  onFixAll,
  onDismissWarning,
  canAutoFix = false,
  className = '',
}) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const [dismissedIndexes, setDismissedIndexes] = useState<Set<number>>(new Set());

  // Filter out dismissed warnings
  const visibleWarnings = useMemo(() => {
    return warnings.filter((_, index) => !dismissedIndexes.has(index));
  }, [warnings, dismissedIndexes]);

  // Return null if no visible warnings
  if (visibleWarnings.length === 0) {
    return null;
  }

  // Handle individual warning dismissal
  const handleDismissWarning = (index: number) => {
    // Find the original index in the warnings array
    const originalIndex = warnings.findIndex(
      (_, i) => !dismissedIndexes.has(i) &&
        warnings.slice(0, i + 1).filter((_, j) => !dismissedIndexes.has(j)).length ===
        visibleWarnings.slice(0, visibleWarnings.indexOf(warnings[i]) + 1).length
    );

    // Add to dismissed set
    setDismissedIndexes((prev) => new Set([...prev, index]));

    // Call callback if provided
    if (onDismissWarning) {
      onDismissWarning(originalIndex !== -1 ? originalIndex : index);
    }
  };

  // Handle dismiss all
  const handleDismissAll = () => {
    // Dismiss all remaining warnings
    const allIndexes = new Set<number>();
    warnings.forEach((_, index) => allIndexes.add(index));
    setDismissedIndexes(allIndexes);

    // Call callback
    if (onDismiss) {
      onDismiss();
    }
  };

  // Toggle expanded state
  const toggleExpanded = () => {
    setIsExpanded((prev) => !prev);
  };

  // Check if any warning has a suggestion (for Fix all button)
  const hasFixableWarnings = canAutoFix && visibleWarnings.some((w) => w.suggestion);

  return (
    <div
      className={`rounded-md border border-yellow-300 bg-yellow-50 overflow-hidden ${className}`}
      role="region"
      aria-label="Parsing warnings"
    >
      {/* Header */}
      <button
        type="button"
        onClick={toggleExpanded}
        className="w-full flex items-center justify-between p-3 cursor-pointer hover:bg-yellow-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-yellow-400"
        aria-expanded={isExpanded}
        aria-controls="warning-list"
      >
        <div className="flex items-center gap-2">
          <WarningIcon className="h-5 w-5 text-yellow-500 flex-shrink-0" />
          <span className="text-sm font-medium text-yellow-800">
            {visibleWarnings.length} warning{visibleWarnings.length !== 1 ? 's' : ''} found
          </span>
        </div>
        <ChevronIcon className="h-4 w-4 text-yellow-600" expanded={isExpanded} />
      </button>

      {/* Warning list (expandable) */}
      {isExpanded && (
        <div
          id="warning-list"
          className="border-t border-yellow-200 divide-y divide-yellow-100"
        >
          {visibleWarnings.map((warning, displayIndex) => {
            // Find original index for this warning
            let originalIndex = 0;
            let count = 0;
            for (let i = 0; i < warnings.length; i++) {
              if (!dismissedIndexes.has(i)) {
                if (count === displayIndex) {
                  originalIndex = i;
                  break;
                }
                count++;
              }
            }

            return (
              <div
                key={`${warning.code}-${originalIndex}`}
                className="flex items-start justify-between p-3 text-sm text-yellow-700"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium">{warning.message}</p>
                  {warning.suggestion && (
                    <p className="text-yellow-600 mt-0.5 text-xs italic">
                      Suggestion: {warning.suggestion}
                    </p>
                  )}
                  {warning.line !== null && (
                    <span className="inline-block mt-1 px-1.5 py-0.5 text-xs bg-yellow-100 text-yellow-700 rounded">
                      Line {warning.line}
                    </span>
                  )}
                </div>
                {onDismissWarning && (
                  <button
                    type="button"
                    onClick={() => handleDismissWarning(originalIndex)}
                    className="ml-2 p-1 text-yellow-600 hover:text-yellow-800 hover:bg-yellow-200 rounded focus:outline-none focus:ring-2 focus:ring-yellow-400"
                    aria-label={`Dismiss warning: ${warning.message}`}
                  >
                    <CloseIcon className="h-4 w-4" />
                  </button>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Action buttons */}
      <div className="flex items-center gap-2 px-3 py-2 bg-yellow-100 border-t border-yellow-200">
        <button
          type="button"
          onClick={handleDismissAll}
          className="text-xs text-yellow-700 hover:text-yellow-900 font-medium focus:outline-none focus:underline"
        >
          Dismiss all
        </button>
        {hasFixableWarnings && onFixAll && (
          <>
            <span className="text-yellow-400">|</span>
            <button
              type="button"
              onClick={onFixAll}
              className="text-xs text-yellow-700 hover:text-yellow-900 font-medium focus:outline-none focus:underline"
            >
              Fix all
            </button>
          </>
        )}
      </div>
    </div>
  );
};
