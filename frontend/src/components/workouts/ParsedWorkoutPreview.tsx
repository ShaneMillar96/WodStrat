import React from 'react';
import { Badge, Alert } from '../ui';
import type { ParsedWorkout, ParsedMovement, ParsingError, WorkoutType } from '../../types/workout';
import { WORKOUT_TYPE_LABELS, WORKOUT_TYPE_COLORS } from '../../types/workout';
import type { BadgeVariant } from '../ui/Badge';

export interface ParsedWorkoutPreviewProps {
  parsedWorkout: ParsedWorkout;
}

/**
 * Component for previewing a parsed workout
 */
export const ParsedWorkoutPreview: React.FC<ParsedWorkoutPreviewProps> = ({
  parsedWorkout,
}) => {
  const workoutTypeColor = WORKOUT_TYPE_COLORS[parsedWorkout.workoutType as WorkoutType] as BadgeVariant;

  return (
    <div className="space-y-4">
      {/* Header with workout type */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Badge variant={workoutTypeColor} size="lg">
            {WORKOUT_TYPE_LABELS[parsedWorkout.workoutType as WorkoutType]}
          </Badge>
          {parsedWorkout.timeCapFormatted && (
            <span className="text-lg font-medium text-gray-700">
              {parsedWorkout.timeCapFormatted}
            </span>
          )}
          {parsedWorkout.roundCount && (
            <span className="text-lg font-medium text-gray-700">
              {parsedWorkout.roundCount} rounds
            </span>
          )}
          {parsedWorkout.intervalDurationFormatted && (
            <span className="text-lg font-medium text-gray-700">
              Every {parsedWorkout.intervalDurationFormatted}
            </span>
          )}
        </div>
        {parsedWorkout.isValid ? (
          <Badge variant="success" size="sm">Valid</Badge>
        ) : (
          <Badge variant="error" size="sm">Has Errors</Badge>
        )}
      </div>

      {/* Parsed description */}
      {parsedWorkout.parsedDescription && (
        <p className="text-gray-600">{parsedWorkout.parsedDescription}</p>
      )}

      {/* Parsing errors */}
      {parsedWorkout.errors.length > 0 && (
        <div className="space-y-2">
          <h4 className="font-medium text-red-700">Parsing Issues:</h4>
          {parsedWorkout.errors.map((error, index) => (
            <ParsingErrorRow key={index} error={error} />
          ))}
        </div>
      )}

      {/* Movements list */}
      <div className="space-y-2">
        <h4 className="font-medium text-gray-900">Movements:</h4>
        <div className="rounded-lg border border-gray-200 divide-y divide-gray-200">
          {parsedWorkout.movements.map((movement) => (
            <ParsedMovementRow key={movement.sequenceOrder} movement={movement} />
          ))}
        </div>
      </div>
    </div>
  );
};

/**
 * Row component for displaying a parsed movement
 */
const ParsedMovementRow: React.FC<{ movement: ParsedMovement }> = ({ movement }) => {
  const hasError = !movement.movementDefinitionId;

  return (
    <div className={`p-3 ${hasError ? 'bg-red-50' : 'bg-white'}`}>
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="text-sm text-gray-400 w-6">{movement.sequenceOrder}.</span>
          <div>
            <div className="flex items-center gap-2">
              {movement.repCount && (
                <span className="font-medium">{movement.repCount}</span>
              )}
              <span className={hasError ? 'text-red-700' : 'text-gray-900'}>
                {movement.movementName || movement.originalText}
              </span>
              {movement.movementCategory && (
                <Badge variant="secondary" size="sm">
                  {movement.movementCategory}
                </Badge>
              )}
            </div>
            {(movement.loadFormatted || movement.distanceFormatted || movement.calories || movement.durationFormatted) && (
              <div className="text-sm text-gray-500 mt-0.5">
                {movement.loadFormatted && <span className="mr-3">{movement.loadFormatted}</span>}
                {movement.distanceFormatted && <span className="mr-3">{movement.distanceFormatted}</span>}
                {movement.calories && <span className="mr-3">{movement.calories} cal</span>}
                {movement.durationFormatted && <span>{movement.durationFormatted}</span>}
              </div>
            )}
          </div>
        </div>
        {hasError && (
          <Badge variant="error" size="sm">Unrecognized</Badge>
        )}
      </div>
      {movement.notes && (
        <p className="text-sm text-gray-500 mt-1 ml-9">{movement.notes}</p>
      )}
    </div>
  );
};

/**
 * Row component for displaying a parsing error
 */
const ParsingErrorRow: React.FC<{ error: ParsingError }> = ({ error }) => (
  <Alert variant="warning">
    <div className="flex items-start gap-2">
      {error.lineNumber > 0 && (
        <span className="text-xs font-medium bg-yellow-200 text-yellow-800 px-1.5 py-0.5 rounded">
          Line {error.lineNumber}
        </span>
      )}
      <div>
        <span className="font-medium">{error.errorType}:</span> {error.message}
        {error.originalText && (
          <span className="block text-sm text-gray-600 mt-0.5">
            "{error.originalText}"
          </span>
        )}
      </div>
    </div>
  </Alert>
);
