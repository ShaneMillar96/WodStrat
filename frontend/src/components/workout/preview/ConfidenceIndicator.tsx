import React from 'react';
import type { ConfidenceIndicatorProps } from '../../../types/workoutPreview';
import { ConfidenceTooltip } from './ConfidenceTooltip';
import { getConfidenceLevel } from '../../../utils/workoutFormatters';
import { CONFIDENCE_STYLES, SIZE_STYLES } from '../../../utils/workoutStyles';

/**
 * Size-based styling configuration for the progress bar
 */
const barSizeStyles = {
  sm: { bar: 'h-1.5', maxWidth: 'max-w-[120px]' },
  md: { bar: 'h-2', maxWidth: 'max-w-[200px]' },
  lg: { bar: 'h-3', maxWidth: 'max-w-[280px]' },
};

/**
 * Visual confidence score indicator with color-coded progress bar
 * Enhanced with optional tooltip showing confidence breakdown
 *
 * Color coding:
 * - Green (>80%): High confidence
 * - Yellow (60-80%): Medium confidence
 * - Red (<60%): Low confidence
 */
export const ConfidenceIndicator: React.FC<ConfidenceIndicatorProps> = ({
  score,
  showTooltip = false,
  showLabel = true,
  size = 'md',
  errorCount = 0,
  unrecognizedCount = 0,
  totalMovements = 0,
  className = '',
}) => {
  const level = getConfidenceLevel(score);
  const styles = CONFIDENCE_STYLES[level];
  const percentage = Math.round(score * 100);
  const sizeStyle = SIZE_STYLES[size];
  const barStyle = barSizeStyles[size];

  const indicator = (
    <div className={`flex items-center gap-2 ${className}`}>
      {showLabel && (
        <span className={`text-gray-500 ${sizeStyle.text}`}>Parse confidence:</span>
      )}
      <div
        className={`flex-1 ${barStyle.bar} bg-gray-200 rounded-full overflow-hidden ${barStyle.maxWidth}`}
        role="progressbar"
        aria-valuenow={percentage}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`Parse confidence: ${percentage}%`}
      >
        <div
          className={`h-full transition-all duration-300 ${styles.barColor}`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      <span className={`font-medium ${styles.color} ${sizeStyle.text}`}>
        {percentage}%
      </span>
    </div>
  );

  if (showTooltip) {
    return (
      <ConfidenceTooltip
        confidence={score}
        errorCount={errorCount}
        unrecognizedCount={unrecognizedCount}
        totalMovements={totalMovements}
      >
        {indicator}
      </ConfidenceTooltip>
    );
  }

  return indicator;
};
