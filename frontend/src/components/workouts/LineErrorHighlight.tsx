import React, { useState, useRef, useEffect, useMemo } from 'react';
import type { LineError, ParsingIssueSeverity } from '../../types/parsingError';

export interface LineErrorHighlightProps {
  /** The original workout text */
  text: string;
  /** Line errors to highlight */
  lineErrors: LineError[];
  /** The line currently being focused (1-indexed) */
  focusedLine?: number | null;
  /** Whether the display is read-only or editable */
  readOnly?: boolean;
  /** Callback when text changes (if editable) */
  onChange?: (text: string) => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Background styles for each severity level
 */
const severityLineStyles: Record<ParsingIssueSeverity, string> = {
  error: 'bg-red-50 border-l-4 border-red-400',
  warning: 'bg-yellow-50 border-l-4 border-yellow-400',
  info: 'bg-blue-50 border-l-4 border-blue-400',
};

/**
 * LineErrorHighlight - Displays workout text with visual error highlighting
 *
 * Features:
 * - Renders text line-by-line with line numbers
 * - Highlights lines with errors using background colors based on severity
 * - Shows hover tooltips with error details
 * - Scrolls to and highlights focused line
 * - Optional edit mode for inline corrections
 */
export const LineErrorHighlight: React.FC<LineErrorHighlightProps> = ({
  text,
  lineErrors,
  focusedLine = null,
  readOnly = true,
  onChange,
  className = '',
}) => {
  const [hoveredLine, setHoveredLine] = useState<number | null>(null);
  const lineRefs = useRef<Map<number, HTMLDivElement>>(new Map());
  const containerRef = useRef<HTMLDivElement>(null);

  // Create a map of line number -> LineError for O(1) lookup
  const lineErrorMap = useMemo(() => {
    const map = new Map<number, LineError>();
    lineErrors.forEach((lineError) => {
      map.set(lineError.lineNumber, lineError);
    });
    return map;
  }, [lineErrors]);

  // Split text into lines
  const lines = useMemo(() => text.split('\n'), [text]);

  // Scroll to focused line
  useEffect(() => {
    if (focusedLine !== null && lineRefs.current.has(focusedLine)) {
      const lineElement = lineRefs.current.get(focusedLine);
      if (lineElement && containerRef.current) {
        lineElement.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
      }
    }
  }, [focusedLine]);

  // Set ref for a line
  const setLineRef = (lineNumber: number, element: HTMLDivElement | null) => {
    if (element) {
      lineRefs.current.set(lineNumber, element);
    } else {
      lineRefs.current.delete(lineNumber);
    }
  };

  // Handle text change for editable mode
  const handleTextChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    if (onChange && !readOnly) {
      onChange(e.target.value);
    }
  };

  // Get tooltip content for a line
  const getTooltipContent = (lineError: LineError): string => {
    const messages = lineError.issues.map((issue) => {
      let content = issue.message;
      if (issue.suggestion) {
        content += ` (Suggestion: ${issue.suggestion})`;
      }
      return content;
    });
    return messages.join('\n');
  };

  // If not read-only, show a textarea with basic highlighting info
  if (!readOnly) {
    return (
      <div className={`relative ${className}`}>
        <textarea
          value={text}
          onChange={handleTextChange}
          className="w-full rounded-lg border border-gray-300 font-mono text-sm p-3 min-h-[200px] focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          rows={Math.max(lines.length, 5)}
        />
        {lineErrors.length > 0 && (
          <div className="mt-2 text-xs text-gray-500">
            {lineErrors.length} line(s) with issues. Switch to preview mode to see highlighted lines.
          </div>
        )}
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className={`rounded-lg border border-gray-300 overflow-hidden font-mono text-sm ${className}`}
      role="region"
      aria-label="Workout text with error highlighting"
    >
      <div className="max-h-[400px] overflow-auto">
        {lines.map((line, index) => {
          const lineNumber = index + 1;
          const lineError = lineErrorMap.get(lineNumber);
          const isFocused = focusedLine === lineNumber;
          const isHovered = hoveredLine === lineNumber;

          // Determine line styling
          let lineStyle = 'border-l-4 border-transparent';
          if (lineError) {
            lineStyle = severityLineStyles[lineError.highestSeverity];
          }

          // Add focus ring if focused
          const focusStyle = isFocused ? 'ring-2 ring-primary-500 ring-inset' : '';

          return (
            <div
              key={lineNumber}
              ref={(el) => setLineRef(lineNumber, el)}
              className={`flex items-start min-h-[1.75rem] ${lineStyle} ${focusStyle} relative`}
              onMouseEnter={() => lineError && setHoveredLine(lineNumber)}
              onMouseLeave={() => setHoveredLine(null)}
            >
              {/* Line number gutter */}
              <div className="w-10 flex-shrink-0 text-right pr-2 py-0.5 text-gray-400 bg-gray-50 select-none border-r border-gray-200">
                {lineNumber}
              </div>

              {/* Line content */}
              <div className="flex-1 px-3 py-0.5 whitespace-pre-wrap break-all">
                {line || '\u00A0' /* Non-breaking space for empty lines */}
              </div>

              {/* Tooltip for error lines */}
              {lineError && isHovered && (
                <div
                  className="absolute z-10 left-12 top-full mt-1 p-2 bg-gray-900 text-white text-xs rounded shadow-lg max-w-xs whitespace-pre-wrap"
                  role="tooltip"
                >
                  {getTooltipContent(lineError)}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};
