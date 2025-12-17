import { useQuery } from '@tanstack/react-query';
import { workoutService } from '../services/workoutService';
import { queryKeys } from '../lib/queryKeys';
import { ApiException } from '../services';
import type { Workout } from '../types/workout';

/**
 * Hook for fetching a single workout by ID
 * @param id - The workout's unique identifier
 */
export function useWorkout(id: number | null) {
  const query = useQuery({
    queryKey: queryKeys.workouts.detail(id ?? 0),
    queryFn: () => workoutService.getById(id!),
    enabled: id !== null && id > 0,
    retry: (failureCount, error) => {
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 2;
    },
  });

  return {
    workout: query.data as Workout | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
  };
}
