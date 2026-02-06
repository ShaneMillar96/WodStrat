import React from 'react';
import type { MovementCardProps } from '../../../types/workoutPreview';
import { MovementCategoryIcon } from './MovementCategoryIcon';
import { WeightDisplay } from './WeightDisplay';
import { DistanceDisplay } from './DistanceDisplay';
import { CalorieDisplay } from './CalorieDisplay';
import { UnrecognizedMovementBadge } from './UnrecognizedMovementBadge';
import { Badge } from '../../ui';

/**
 * Enhanced movement display card with category icon, weight, distance, and calorie displays
 */
export const MovementCard: React.FC<MovementCardProps> = ({
  movement,
  sequenceNumber,
  repScheme,
  onEdit,
  showError = true,
  showCategoryIcon = true,
  className = '',
}) => {
  // Determine if this is an unrecognized movement
  const hasError = !movement.movementDefinitionId;

  // Get movement name (use originalText as fallback)
  const movementName = movement.movementName || movement.originalText;

  // Get category
  const category = movement.movementCategory;

  // Check if movement has any metric details
  const hasMetrics = movement.loadValue !== null ||
    movement.distanceValue !== null ||
    movement.calories !== null ||
    movement.durationFormatted !== null;

  return (
    <div
      className={`p-3 ${showError && hasError ? 'bg-red-50' : 'bg-white'} ${className}`}
      role="article"
      aria-label={`Movement ${sequenceNumber}: ${movementName}`}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-start gap-3 flex-1 min-w-0">
          {/* Sequence number */}
          <span className="text-sm text-gray-400 w-6 flex-shrink-0 pt-0.5">
            {sequenceNumber}.
          </span>

          {/* Category icon */}
          {showCategoryIcon && category && (
            <MovementCategoryIcon
              category={category}
              size="sm"
              className="flex-shrink-0 mt-0.5"
            />
          )}

          {/* Movement details */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              {/* Rep count or rep scheme pattern */}
              {movement.repCount !== null ? (
                <span className="font-semibold text-gray-900">
                  {movement.repCount}
                </span>
              ) : repScheme && repScheme.length > 0 ? (
                <span className="font-semibold text-gray-900">
                  {repScheme.join('-')}
                </span>
              ) : null}

              {/* Movement name */}
              <span className={`font-medium ${showError && hasError ? 'text-red-700' : 'text-gray-900'}`}>
                {movementName}
              </span>

              {/* Category badge */}
              {category && (
                <Badge variant="secondary" size="sm">
                  {category}
                </Badge>
              )}
            </div>

            {/* Rep scheme pattern - only show when we displayed repCount above (so pattern adds context) */}
            {repScheme && repScheme.length > 1 && movement.repCount !== null && (
              <div className="text-xs text-gray-500 mt-0.5">
                {repScheme.join('-')} reps
              </div>
            )}

            {/* Metrics row */}
            {hasMetrics && (
              <div className="flex items-center gap-3 mt-1 text-sm flex-wrap">
                {/* Weight */}
                {movement.loadValue !== null && (
                  <WeightDisplay
                    loadValue={movement.loadValue}
                    loadValueFemale={movement.loadValueFemale}
                    loadUnit={movement.loadUnit}
                  />
                )}

                {/* Distance */}
                {movement.distanceValue !== null && (
                  <DistanceDisplay
                    distanceValue={movement.distanceValue}
                    distanceUnit={movement.distanceUnit}
                  />
                )}

                {/* Calories */}
                {movement.calories !== null && (
                  <CalorieDisplay
                    calories={movement.calories}
                    caloriesFemale={movement.caloriesFemale}
                  />
                )}

                {/* Duration */}
                {movement.durationFormatted && (
                  <span className="text-gray-600">
                    {movement.durationFormatted}
                  </span>
                )}
              </div>
            )}

            {/* Notes */}
            {movement.notes && (
              <p className="text-sm text-gray-500 mt-1 italic">{movement.notes}</p>
            )}
          </div>
        </div>

        {/* Right side: badges and actions */}
        <div className="flex items-center gap-2 flex-shrink-0">
          {/* Unrecognized badge */}
          {showError && hasError && (
            <UnrecognizedMovementBadge originalText={movement.originalText} />
          )}

          {/* Edit button */}
          {onEdit && (
            <button
              type="button"
              onClick={onEdit}
              className="text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded p-1"
              aria-label={`Edit ${movementName}`}
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
              </svg>
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
