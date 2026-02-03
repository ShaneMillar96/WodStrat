import { useQuery } from '@tanstack/react-query';
import { volumeLoadService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { WorkoutVolumeLoadResult } from '../types';

/**
 * Hook for fetching volume load analysis for a workout
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param options - Optional configuration
 */
export function useWorkoutVolumeLoad(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  options: { enabled?: boolean } = {}
) {
  const { enabled = true } = options;

  const query = useQuery({
    queryKey: queryKeys.volumeLoad.workout(athleteId ?? 0, workoutId ?? 0),
    queryFn: () => volumeLoadService.getWorkoutVolumeLoad(athleteId!, workoutId!),
    enabled: enabled && !!athleteId && !!workoutId && athleteId > 0 && workoutId > 0,
    retry: (failureCount, error) => {
      // Don't retry on 404 (no benchmark data) or 401 (unauthorized)
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 2;
    },
    // Cache for 5 minutes since volume load calculations are relatively stable
    staleTime: 5 * 60 * 1000,
  });

  return {
    volumeLoad: query.data as WorkoutVolumeLoadResult | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
    // Helper to check if we have volume data
    hasVolumeData: !!query.data && query.data.movementVolumes.length > 0,
    // Helper to check if there are any high load movements
    hasHighLoadMovements: query.data?.movementVolumes.some(m => m.loadClassification === 'High') ?? false,
  };
}
