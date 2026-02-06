import React from 'react';

export interface SectionSkeletonProps {
  /** Title placeholder width */
  titleWidth?: string;
  /** Number of content lines */
  lines?: number;
  /** Whether to show a card outline */
  showCard?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * SectionSkeleton - Loading placeholder for strategy sections
 */
export const SectionSkeleton: React.FC<SectionSkeletonProps> = ({
  titleWidth = 'w-1/3',
  lines = 3,
  showCard = true,
  className = '',
}) => {
  const content = (
    <div className="space-y-3">
      <div className={`h-5 bg-gray-200 rounded animate-pulse ${titleWidth}`} />
      {Array.from({ length: lines }).map((_, i) => (
        <div
          key={i}
          className={`h-4 bg-gray-200 rounded animate-pulse ${
            i === lines - 1 ? 'w-2/3' : 'w-full'
          }`}
        />
      ))}
    </div>
  );

  if (showCard) {
    return (
      <div className={`rounded-lg border border-gray-200 bg-white p-5 ${className}`}>
        {content}
      </div>
    );
  }

  return <div className={className}>{content}</div>;
};
