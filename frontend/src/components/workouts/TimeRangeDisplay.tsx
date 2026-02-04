import React from 'react';

export interface TimeRangeDisplayProps {
  /** Minimum time in seconds */
  minSeconds: number;
  /** Maximum time in seconds */
  maxSeconds: number;
  /** Formatted range string */
  formattedRange: string;
  /** Time cap in seconds (optional, for context) */
  timeCapSeconds?: number | null;
  /** Additional CSS classes */
  className?: string;
}

/**
 * TimeRangeDisplay - Visual time range representation
 *
 * Features:
 * - Shows formatted time range prominently
 * - Visual bar showing range relative to time cap (if provided)
 * - Min and max time labels
 */
export const TimeRangeDisplay: React.FC<TimeRangeDisplayProps> = ({
  minSeconds,
  maxSeconds,
  formattedRange,
  timeCapSeconds,
  className = '',
}) => {
  // Calculate bar widths as percentage of time cap or max estimate
  const reference = timeCapSeconds ?? maxSeconds * 1.2;
  const minWidth = Math.min((minSeconds / reference) * 100, 100);
  const maxWidth = Math.min((maxSeconds / reference) * 100, 100);

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className={`space-y-3 ${className}`}>
      {/* Main range display */}
      <div className="text-center">
        <p className="text-3xl font-bold text-gray-900">{formattedRange}</p>
        <p className="text-sm text-gray-500 mt-1">Estimated completion time</p>
      </div>

      {/* Visual bar */}
      <div className="relative h-8 bg-gray-100 rounded-lg overflow-hidden">
        {/* Min bar */}
        <div
          className="absolute top-0 left-0 h-full bg-green-400 opacity-50 transition-all duration-300"
          style={{ width: `${minWidth}%` }}
        />
        {/* Max bar */}
        <div
          className="absolute top-0 left-0 h-full bg-blue-400 opacity-50 transition-all duration-300"
          style={{ width: `${maxWidth}%` }}
        />

        {/* Labels */}
        <div className="absolute inset-0 flex items-center justify-between px-3">
          <span className="text-xs font-medium text-gray-700 bg-white/80 px-1 rounded">
            {formatTime(minSeconds)}
          </span>
          <span className="text-xs font-medium text-gray-700 bg-white/80 px-1 rounded">
            {formatTime(maxSeconds)}
          </span>
        </div>
      </div>

      {/* Time cap indicator */}
      {timeCapSeconds && (
        <div className="flex justify-end">
          <span className="text-xs text-gray-500">
            Time cap: {formatTime(timeCapSeconds)}
          </span>
        </div>
      )}
    </div>
  );
};
