import React from 'react';

export interface SkeletonProps {
  /** Width of the skeleton */
  width?: string | number;
  /** Height of the skeleton */
  height?: string | number;
  /** Whether to show as a circle */
  circle?: boolean;
  /** Additional class names */
  className?: string;
  /** Number of skeleton lines to render */
  count?: number;
}

/**
 * Skeleton loading placeholder component
 */
export const Skeleton: React.FC<SkeletonProps> = ({
  width,
  height,
  circle = false,
  className = '',
  count = 1,
}) => {
  const baseStyles = 'animate-pulse bg-gray-200';
  const shapeStyles = circle ? 'rounded-full' : 'rounded';

  const style: React.CSSProperties = {
    width: typeof width === 'number' ? `${width}px` : width,
    height: typeof height === 'number' ? `${height}px` : height,
  };

  if (count === 1) {
    return (
      <div
        className={`${baseStyles} ${shapeStyles} ${className}`}
        style={style}
        aria-hidden="true"
      />
    );
  }

  return (
    <div className="space-y-2" aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <div
          key={index}
          className={`${baseStyles} ${shapeStyles} ${className}`}
          style={style}
        />
      ))}
    </div>
  );
};

/**
 * Skeleton text line
 */
export const SkeletonText: React.FC<{ lines?: number; className?: string }> = ({
  lines = 1,
  className = '',
}) => (
  <div className={`space-y-2 ${className}`} aria-hidden="true">
    {Array.from({ length: lines }).map((_, index) => (
      <div
        key={index}
        className={`h-4 animate-pulse rounded bg-gray-200 ${
          index === lines - 1 ? 'w-3/4' : 'w-full'
        }`}
      />
    ))}
  </div>
);

/**
 * Skeleton for table rows
 */
export const SkeletonTableRow: React.FC<{ columns?: number }> = ({
  columns = 4,
}) => (
  <tr aria-hidden="true">
    {Array.from({ length: columns }).map((_, index) => (
      <td key={index} className="px-4 py-3">
        <div className="h-4 animate-pulse rounded bg-gray-200" />
      </td>
    ))}
  </tr>
);

/**
 * Skeleton for cards
 */
export const SkeletonCard: React.FC<{ className?: string }> = ({
  className = '',
}) => (
  <div
    className={`rounded-lg border border-gray-200 bg-white p-4 ${className}`}
    aria-hidden="true"
  >
    <div className="space-y-3">
      <div className="h-5 w-3/4 animate-pulse rounded bg-gray-200" />
      <div className="h-4 w-1/2 animate-pulse rounded bg-gray-200" />
      <div className="flex items-center justify-between pt-2">
        <div className="h-6 w-20 animate-pulse rounded bg-gray-200" />
        <div className="h-8 w-16 animate-pulse rounded bg-gray-200" />
      </div>
    </div>
  </div>
);
