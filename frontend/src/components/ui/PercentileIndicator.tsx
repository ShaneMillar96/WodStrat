import React from 'react';

export interface PercentileIndicatorProps {
  /** Percentile value (0-100) */
  percentile: number | null;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show numeric value */
  showValue?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Get color classes based on percentile value
 * Red (0-25) -> Yellow (25-50) -> Green (50-75) -> Blue (75-100)
 */
const getPercentileColors = (percentile: number): { bg: string; fill: string; text: string } => {
  if (percentile < 25) {
    return {
      bg: 'bg-red-100',
      fill: 'bg-red-500',
      text: 'text-red-700',
    };
  }
  if (percentile < 50) {
    return {
      bg: 'bg-yellow-100',
      fill: 'bg-yellow-500',
      text: 'text-yellow-700',
    };
  }
  if (percentile < 75) {
    return {
      bg: 'bg-green-100',
      fill: 'bg-green-500',
      text: 'text-green-700',
    };
  }
  return {
    bg: 'bg-blue-100',
    fill: 'bg-blue-500',
    text: 'text-blue-700',
  };
};

/**
 * PercentileIndicator - Reusable component to visualize athlete percentile
 *
 * Features:
 * - Horizontal bar with filled portion based on percentile
 * - Color gradient: red (0-25) -> yellow (25-50) -> green (50-75) -> blue (75-100)
 * - Optional numeric display
 * - Shows "N/A" when percentile is null
 */
export const PercentileIndicator: React.FC<PercentileIndicatorProps> = ({
  percentile,
  size = 'md',
  showValue = true,
  className = '',
}) => {
  const sizeStyles = {
    sm: {
      bar: 'h-1.5',
      text: 'text-xs',
      width: 'w-16',
      gap: 'gap-1.5',
    },
    md: {
      bar: 'h-2',
      text: 'text-sm',
      width: 'w-20',
      gap: 'gap-2',
    },
    lg: {
      bar: 'h-3',
      text: 'text-base',
      width: 'w-24',
      gap: 'gap-2.5',
    },
  };

  const styles = sizeStyles[size];

  if (percentile === null || percentile === undefined) {
    return (
      <div className={`flex items-center ${styles.gap} ${className}`}>
        <div className={`${styles.width} ${styles.bar} bg-gray-200 rounded-full overflow-hidden`}>
          <div className="h-full w-0" />
        </div>
        {showValue && (
          <span className={`${styles.text} text-gray-400 font-medium`}>N/A</span>
        )}
      </div>
    );
  }

  const colors = getPercentileColors(percentile);
  const clampedPercentile = Math.max(0, Math.min(100, percentile));

  return (
    <div className={`flex items-center ${styles.gap} ${className}`}>
      <div
        className={`${styles.width} ${styles.bar} ${colors.bg} rounded-full overflow-hidden`}
        role="progressbar"
        aria-valuenow={clampedPercentile}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`Percentile: ${Math.round(clampedPercentile)}%`}
      >
        <div
          className={`h-full ${colors.fill} transition-all duration-300`}
          style={{ width: `${clampedPercentile}%` }}
        />
      </div>
      {showValue && (
        <span className={`${styles.text} ${colors.text} font-medium tabular-nums`}>
          {Math.round(clampedPercentile)}%
        </span>
      )}
    </div>
  );
};
