import React from 'react';
import { MovementCard } from './MovementCard';
import type { ParsedMovement, WorkoutMovement } from '../../types/workout';

export interface MovementListProps {
  /** Array of movements to display */
  movements: (ParsedMovement | WorkoutMovement)[];
  /** Show error indicators for unrecognized movements */
  showErrors?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * List component for displaying workout movements
 * Renders MovementCard components for each movement with dividers
 */
export const MovementList: React.FC<MovementListProps> = ({
  movements,
  showErrors = true,
  className = '',
}) => {
  if (movements.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 text-center text-gray-500">
        No movements
      </div>
    );
  }

  return (
    <div className={`rounded-lg border border-gray-200 divide-y divide-gray-200 ${className}`}>
      {movements.map((movement, index) => (
        <MovementCard
          key={'id' in movement ? movement.id : `parsed-${index}`}
          movement={movement}
          sequenceNumber={index + 1}
          showError={showErrors}
        />
      ))}
    </div>
  );
};
