import React from 'react';
import type { WorkoutMetadataProps } from '../../../types/workoutPreview';

/**
 * Icon components for metadata items
 */
const ClockIcon = () => (
  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
);

const RefreshIcon = () => (
  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
  </svg>
);

const IntervalIcon = () => (
  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
  </svg>
);

/**
 * Enhanced workout metadata display with icons
 * Shows time cap, round count, and interval duration
 */
export const WorkoutMetadata: React.FC<WorkoutMetadataProps> = ({
  timeCapFormatted,
  roundCount,
  intervalDurationFormatted,
  compact = false,
  className = '',
}) => {
  const items: { icon: React.ReactNode; label: string; value: string }[] = [];

  if (timeCapFormatted) {
    items.push({
      icon: <ClockIcon />,
      label: 'Time Cap',
      value: timeCapFormatted,
    });
  }

  if (roundCount) {
    items.push({
      icon: <RefreshIcon />,
      label: 'Rounds',
      value: `${roundCount} rounds`,
    });
  }

  if (intervalDurationFormatted) {
    items.push({
      icon: <IntervalIcon />,
      label: 'Interval',
      value: `Every ${intervalDurationFormatted}`,
    });
  }

  if (items.length === 0) {
    return null;
  }

  if (compact) {
    // Compact mode: inline display without icons
    return (
      <div className={`flex items-center gap-2 text-sm font-medium text-gray-600 ${className}`}>
        {items.map((item, index) => (
          <React.Fragment key={index}>
            {index > 0 && <span className="text-gray-300">|</span>}
            <span>{item.value}</span>
          </React.Fragment>
        ))}
      </div>
    );
  }

  return (
    <div className={`flex items-center gap-4 flex-wrap ${className}`}>
      {items.map((item, index) => (
        <div
          key={index}
          className="flex items-center gap-1.5 text-sm"
          title={item.label}
        >
          <span className="text-gray-400">{item.icon}</span>
          <span className="font-medium text-gray-700">{item.value}</span>
        </div>
      ))}
    </div>
  );
};
