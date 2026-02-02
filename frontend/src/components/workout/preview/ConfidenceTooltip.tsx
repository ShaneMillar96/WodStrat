import React, { useState } from 'react';
import type { ConfidenceTooltipProps } from '../../../types/workoutPreview';
import { getConfidenceLevel } from '../../../utils/workoutFormatters';
import { CONFIDENCE_STYLES } from '../../../utils/workoutStyles';

/**
 * Tooltip component explaining confidence score calculation
 * Uses native HTML/CSS tooltip since Radix UI Tooltip is not installed
 */
export const ConfidenceTooltip: React.FC<ConfidenceTooltipProps> = ({
  confidence,
  errorCount,
  unrecognizedCount,
  totalMovements,
  children,
}) => {
  const [isVisible, setIsVisible] = useState(false);
  const level = getConfidenceLevel(confidence);
  const styles = CONFIDENCE_STYLES[level];
  const percentage = Math.round(confidence * 100);

  // Calculate recognized percentage
  const recognizedCount = totalMovements - unrecognizedCount;
  const recognizedPercentage = totalMovements > 0
    ? Math.round((recognizedCount / totalMovements) * 100)
    : 0;

  return (
    <div
      className="relative inline-block"
      onMouseEnter={() => setIsVisible(true)}
      onMouseLeave={() => setIsVisible(false)}
      onFocus={() => setIsVisible(true)}
      onBlur={() => setIsVisible(false)}
    >
      <div tabIndex={0} role="button" aria-describedby="confidence-tooltip">
        {children}
      </div>
      {isVisible && (
        <div
          id="confidence-tooltip"
          role="tooltip"
          className="absolute z-50 bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-3 py-2 bg-gray-900 text-white text-xs rounded-lg shadow-lg min-w-[200px]"
        >
          <div className="font-semibold mb-2">Parse Confidence: {percentage}%</div>
          <div className="space-y-1 text-gray-300">
            <div className="flex justify-between">
              <span>Movements recognized:</span>
              <span className={recognizedPercentage === 100 ? 'text-green-400' : 'text-yellow-400'}>
                {recognizedCount}/{totalMovements}
              </span>
            </div>
            {errorCount > 0 && (
              <div className="flex justify-between">
                <span>Parsing errors:</span>
                <span className="text-red-400">{errorCount}</span>
              </div>
            )}
            {unrecognizedCount > 0 && (
              <div className="flex justify-between">
                <span>Unrecognized:</span>
                <span className="text-yellow-400">{unrecognizedCount}</span>
              </div>
            )}
          </div>
          <div className={`mt-2 pt-2 border-t border-gray-700 ${styles.color}`}>
            {level === 'high' && 'High confidence - ready to save'}
            {level === 'medium' && 'Medium confidence - review suggested'}
            {level === 'low' && 'Low confidence - corrections needed'}
          </div>
          {/* Arrow */}
          <div className="absolute top-full left-1/2 transform -translate-x-1/2 -mt-px">
            <div className="border-8 border-transparent border-t-gray-900" />
          </div>
        </div>
      )}
    </div>
  );
};
