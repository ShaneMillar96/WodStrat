import React from 'react';
import { WorkoutCard } from './WorkoutCard';
import { SkeletonCard } from '../ui';
import type { Workout } from '../../types/workout';

export interface WorkoutsListProps {
  workouts: Workout[];
  onView: (workout: Workout) => void;
  onEdit: (workout: Workout) => void;
  onDelete: (workout: Workout) => void;
  isLoading?: boolean;
  emptyMessage?: string;
}

/**
 * List component for displaying workouts
 */
export const WorkoutsList: React.FC<WorkoutsListProps> = ({
  workouts,
  onView,
  onEdit,
  onDelete,
  isLoading = false,
  emptyMessage = 'No workouts found',
}) => {
  if (isLoading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map((i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    );
  }

  if (workouts.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-gray-50 p-8 text-center">
        <p className="text-gray-500">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {workouts.map((workout) => (
        <WorkoutCard
          key={workout.id}
          workout={workout}
          onView={onView}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
    </div>
  );
};
