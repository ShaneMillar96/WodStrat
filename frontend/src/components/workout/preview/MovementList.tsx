import React from 'react';
import { MovementCard } from './MovementCard';
import type { ParsedMovement } from '../../../types/workout';
import type { RepScheme } from '../../../types/workoutPreview';

export interface MovementListProps {
  /** Array of movements to display */
  movements: ParsedMovement[];
  /** Rep scheme for the workout (used to show pattern on cards) */
  repScheme?: RepScheme | null;
  /** Show error indicators for unrecognized movements */
  showErrors?: boolean;
  /** Show category icons on movement cards */
  showCategoryIcons?: boolean;
  /** Callback for editing a movement */
  onEditMovement?: (index: number) => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Enhanced movement list component with section title
 * Displays movements with dividers and supports category icons
 */
export const MovementList: React.FC<MovementListProps> = ({
  movements,
  repScheme,
  showErrors = true,
  showCategoryIcons = true,
  onEditMovement,
  className = '',
}) => {
  if (movements.length === 0) {
    return (
      <div className={`${className}`}>
        <h4 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wide">
          Movements
        </h4>
        <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 text-center text-gray-500">
          No movements detected
        </div>
      </div>
    );
  }

  return (
    <div className={`${className}`}>
      <h4 className="text-sm font-semibold text-gray-900 mb-3 uppercase tracking-wide">
        Movements ({movements.length})
      </h4>
      <div className="rounded-lg border border-gray-200 divide-y divide-gray-200 overflow-hidden">
        {movements.map((movement, index) => (
          <MovementCard
            key={`movement-${index}`}
            movement={movement}
            sequenceNumber={index + 1}
            repScheme={repScheme?.reps}
            showError={showErrors}
            showCategoryIcon={showCategoryIcons}
            onEdit={onEditMovement ? () => onEditMovement(index) : undefined}
          />
        ))}
      </div>
    </div>
  );
};
