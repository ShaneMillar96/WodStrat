import React from 'react';
import { Badge, Button } from '../ui';
import type { Workout, WorkoutType } from '../../types/workout';
import { WORKOUT_TYPE_LABELS, WORKOUT_TYPE_COLORS } from '../../types/workout';
import type { BadgeVariant } from '../ui/Badge';

export interface WorkoutCardProps {
  workout: Workout;
  onView: (workout: Workout) => void;
  onEdit: (workout: Workout) => void;
  onDelete: (workout: Workout) => void;
}

/**
 * Card component for displaying a workout in the list
 */
export const WorkoutCard: React.FC<WorkoutCardProps> = ({
  workout,
  onView,
  onEdit,
  onDelete,
}) => {
  const workoutTypeColor = WORKOUT_TYPE_COLORS[workout.workoutType as WorkoutType] as BadgeVariant;
  const displayName = workout.name || workout.parsedDescription || 'Unnamed Workout';

  // Format date
  const formattedDate = new Date(workout.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <Badge variant={workoutTypeColor} size="sm">
              {WORKOUT_TYPE_LABELS[workout.workoutType as WorkoutType]}
            </Badge>
            {workout.timeCapFormatted && (
              <span className="text-sm text-gray-500">{workout.timeCapFormatted}</span>
            )}
            {workout.roundCount && (
              <span className="text-sm text-gray-500">{workout.roundCount} rounds</span>
            )}
          </div>

          <h3 className="font-semibold text-gray-900 mb-1">{displayName}</h3>

          <div className="text-sm text-gray-600 mb-2">
            {workout.movements.slice(0, 3).map((m, i) => (
              <span key={m.id}>
                {i > 0 && ' | '}
                {m.repCount && `${m.repCount} `}
                {m.movementName}
                {m.loadFormatted && ` (${m.loadFormatted})`}
              </span>
            ))}
            {workout.movements.length > 3 && (
              <span className="text-gray-400"> +{workout.movements.length - 3} more</span>
            )}
          </div>

          <span className="text-xs text-gray-400">{formattedDate}</span>
        </div>

        <div className="flex items-center gap-2 ml-4">
          <Button variant="outline" size="sm" onClick={() => onView(workout)}>
            View
          </Button>
          <Button variant="outline" size="sm" onClick={() => onEdit(workout)}>
            Edit
          </Button>
          <Button variant="outline" size="sm" onClick={() => onDelete(workout)}>
            Delete
          </Button>
        </div>
      </div>
    </div>
  );
};
