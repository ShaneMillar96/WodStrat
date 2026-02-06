import React from 'react';
import type { KeyFocusMovement } from '../../types/strategyInsights';
import { SectionSkeleton } from '../ui';

export interface KeyFocusSectionProps {
  /** Key focus movements */
  movements?: KeyFocusMovement[];
  /** Loading state */
  isLoading?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * KeyFocusSection - Displays key focus movements to pay attention to
 */
export const KeyFocusSection: React.FC<KeyFocusSectionProps> = ({
  movements = [],
  isLoading = false,
  className = '',
}) => {
  if (isLoading) {
    return <SectionSkeleton titleWidth="w-1/3" lines={4} className={className} />;
  }

  if (movements.length === 0) {
    return null;
  }

  return (
    <div className={`rounded-lg border border-gray-200 bg-white p-5 ${className}`}>
      <div className="flex items-center gap-2 mb-4">
        <span className="text-xl" role="img" aria-label="Target">&#x1F3AF;</span>
        <h3 className="font-semibold text-gray-900">Key Focus</h3>
      </div>

      <div className="space-y-3">
        {movements
          .sort((a, b) => a.priority - b.priority)
          .map((movement, index) => (
            <div
              key={`${movement.movementName}-${index}`}
              className="flex items-start gap-3"
            >
              <span className="flex-shrink-0 w-6 h-6 rounded-full bg-primary-100 text-primary-700 text-sm font-medium flex items-center justify-center">
                {movement.priority}
              </span>
              <div className="flex-1 min-w-0">
                <h4 className="font-medium text-gray-900">{movement.movementName}</h4>
                <p className="text-sm text-gray-600 mt-0.5">{movement.reason}</p>
                <p className="text-sm text-primary-700 mt-1 font-medium">
                  {movement.recommendation}
                </p>
              </div>
            </div>
          ))}
      </div>
    </div>
  );
};
