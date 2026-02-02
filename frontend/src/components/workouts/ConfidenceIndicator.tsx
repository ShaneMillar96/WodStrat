import React from 'react';

export interface ConfidenceIndicatorProps {
  /** Confidence score (0-1) */
  confidence: number;
  /** Show text label */
  showLabel?: boolean;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Additional CSS classes */
  className?: string;
}

/**
 * Size-based styling configuration
 */
const sizeStyles = {
  sm: { bar: 'h-1.5', text: 'text-xs', maxWidth: 'max-w-[120px]' },
  md: { bar: 'h-2', text: 'text-sm', maxWidth: 'max-w-[200px]' },
  lg: { bar: 'h-3', text: 'text-base', maxWidth: 'max-w-[280px]' },
};

/**
 * Visual confidence score indicator with color-coded progress bar
 *
 * Color coding:
 * - Green (>80%): High confidence
 * - Yellow (60-80%): Medium confidence
 * - Red (<60%): Low confidence
 */
export const ConfidenceIndicator: React.FC<ConfidenceIndicatorProps> = ({
  confidence,
  showLabel = true,
  size = 'md',
  className = '',
}) => {
  // Determine color based on confidence level
  let barColorClass = 'bg-red-500';
  let textColorClass = 'text-red-700';

  if (confidence > 0.8) {
    barColorClass = 'bg-green-500';
    textColorClass = 'text-green-700';
  } else if (confidence > 0.6) {
    barColorClass = 'bg-yellow-500';
    textColorClass = 'text-yellow-700';
  }

  const percentage = Math.round(confidence * 100);
  const styles = sizeStyles[size];

  return (
    <div className={`flex items-center gap-2 ${className}`}>
      {showLabel && (
        <span className={`text-gray-500 ${styles.text}`}>Parse confidence:</span>
      )}
      <div
        className={`flex-1 ${styles.bar} bg-gray-200 rounded-full overflow-hidden ${styles.maxWidth}`}
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`Parse confidence: ${percentage}%`}
      >
        <div
          className={`h-full transition-all duration-300 ${barColorClass}`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      <span className={`font-medium ${textColorClass} ${styles.text}`}>
        {percentage}%
      </span>
    </div>
  );
};
