import React from 'react';

export interface AmrapExtraRepsDisplayProps {
  /** Minimum extra reps */
  minExtraReps: number;
  /** Maximum extra reps */
  maxExtraReps: number;
  /** Formatted range string */
  formattedRange: string;
  /** Additional CSS classes */
  className?: string;
}

/**
 * AmrapExtraRepsDisplay - Display AMRAP extra reps range estimate
 *
 * Features:
 * - Shows min and max extra reps in a visually appealing format
 * - Displays the formatted range
 * - Designed for AMRAP workout type timing information
 */
export const AmrapExtraRepsDisplay: React.FC<AmrapExtraRepsDisplayProps> = ({
  minExtraReps,
  maxExtraReps,
  formattedRange,
  className = '',
}) => {
  return (
    <div className={`rounded-lg border border-blue-200 bg-blue-50 p-4 ${className}`}>
      <div className="flex items-center gap-2 mb-2">
        <svg
          className="w-5 h-5 text-blue-600"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M13 10V3L4 14h7v7l9-11h-7z"
          />
        </svg>
        <h4 className="font-medium text-blue-900">AMRAP Target</h4>
      </div>

      <div className="flex items-baseline gap-2">
        <span className="text-2xl font-bold text-blue-700 tabular-nums">
          {formattedRange}
        </span>
      </div>

      <div className="mt-2 text-sm text-blue-700">
        <p>
          Expected to complete{' '}
          <span className="font-medium">{minExtraReps}</span>
          {minExtraReps !== maxExtraReps && (
            <>
              {' '}to{' '}
              <span className="font-medium">{maxExtraReps}</span>
            </>
          )}{' '}
          extra reps into the next round
        </p>
      </div>
    </div>
  );
};
