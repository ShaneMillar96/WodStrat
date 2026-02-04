import React from 'react';

export interface AmrapEstimateDisplayProps {
  /** Minimum estimate (total reps equivalent) */
  minEstimate: number;
  /** Maximum estimate (total reps equivalent) */
  maxEstimate: number;
  /** Formatted range string (e.g., "5 rounds + 12 reps to 6 rounds + 3 reps") */
  formattedRange: string;
  /** Time cap in seconds */
  timeCapSeconds?: number | null;
  /** Additional CSS classes */
  className?: string;
}

/**
 * AmrapEstimateDisplay - Display AMRAP rounds/reps estimate
 *
 * Features:
 * - Shows formatted rounds + reps range
 * - Visual representation of expected work
 * - Time cap context display
 */
export const AmrapEstimateDisplay: React.FC<AmrapEstimateDisplayProps> = ({
  minEstimate: _minEstimate,
  maxEstimate: _maxEstimate,
  formattedRange,
  timeCapSeconds,
  className = '',
}) => {
  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    return `${mins} min`;
  };

  return (
    <div className={`space-y-3 ${className}`}>
      {/* Main range display */}
      <div className="text-center">
        <p className="text-2xl font-bold text-gray-900">{formattedRange}</p>
        <p className="text-sm text-gray-500 mt-1">Expected rounds/reps</p>
      </div>

      {/* Time cap info */}
      {timeCapSeconds && (
        <div className="flex justify-center">
          <div className="inline-flex items-center gap-2 px-3 py-1.5 bg-blue-50 text-blue-700 rounded-full text-sm">
            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span>{formatTime(timeCapSeconds)} AMRAP</span>
          </div>
        </div>
      )}
    </div>
  );
};
