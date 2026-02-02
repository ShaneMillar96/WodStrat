import React from 'react';
import { Badge } from '../ui';
import type { ParsedMovement, WorkoutMovement } from '../../types/workout';

export interface MovementCardProps {
  /** Movement data (parsed or saved) */
  movement: ParsedMovement | WorkoutMovement;
  /** Sequence number (1-indexed) */
  sequenceNumber: number;
  /** Show error styling for unrecognized movements */
  showError?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Type guard to check if movement is a ParsedMovement
 */
function isParsedMovement(m: ParsedMovement | WorkoutMovement): m is ParsedMovement {
  return 'originalText' in m && 'movementCanonicalName' in m;
}

/**
 * Single movement display card with rep count, name, load, distance, and other details
 */
export const MovementCard: React.FC<MovementCardProps> = ({
  movement,
  sequenceNumber,
  showError = true,
  className = '',
}) => {
  // Determine if this is an unrecognized movement (only for parsed movements)
  const hasError = isParsedMovement(movement)
    ? !movement.movementDefinitionId
    : false;

  // Get movement name (use originalText as fallback for parsed movements)
  const movementName = isParsedMovement(movement)
    ? (movement.movementName || movement.originalText)
    : movement.movementName;

  // Get category
  const category = movement.movementCategory;

  // Get formatted values (same properties exist on both types)
  const { loadFormatted, distanceFormatted, durationFormatted, calories, notes } = movement;

  return (
    <div
      className={`p-3 ${showError && hasError ? 'bg-red-50' : 'bg-white'} ${className}`}
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="text-sm text-gray-400 w-6">{sequenceNumber}.</span>
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              {movement.repCount && (
                <span className="font-medium">{movement.repCount}</span>
              )}
              <span className={showError && hasError ? 'text-red-700' : 'text-gray-900'}>
                {movementName}
              </span>
              {category && (
                <Badge variant="secondary" size="sm">
                  {category}
                </Badge>
              )}
            </div>
            {(loadFormatted || distanceFormatted || calories || durationFormatted) && (
              <div className="text-sm text-gray-500 mt-0.5">
                {loadFormatted && <span className="mr-3">{loadFormatted}</span>}
                {distanceFormatted && <span className="mr-3">{distanceFormatted}</span>}
                {calories && <span className="mr-3">{calories} cal</span>}
                {durationFormatted && <span>{durationFormatted}</span>}
              </div>
            )}
          </div>
        </div>
        {showError && hasError && (
          <Badge variant="error" size="sm">Unrecognized</Badge>
        )}
      </div>
      {notes && (
        <p className="text-sm text-gray-500 mt-1 ml-9">{notes}</p>
      )}
    </div>
  );
};
