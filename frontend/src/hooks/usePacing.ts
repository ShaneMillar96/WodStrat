import { useQuery } from '@tanstack/react-query';
import { pacingService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { WorkoutPacingResult } from '../types';

/**
 * Hook for fetching pacing analysis for a workout
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param options - Optional configuration
 */
export function usePacing(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  options: { enabled?: boolean } = {}
) {
  const { enabled = true } = options;

  const query = useQuery({
    queryKey: queryKeys.pacing.workout(athleteId ?? 0, workoutId ?? 0),
    queryFn: () => pacingService.getWorkoutPacing(athleteId!, workoutId!),
    enabled: enabled && !!athleteId && !!workoutId && athleteId > 0 && workoutId > 0,
    retry: (failureCount, error) => {
      // Don't retry on 404 (no benchmark data) or 401 (unauthorized)
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 2;
    },
    // Cache for 5 minutes since pacing calculations are relatively stable
    staleTime: 5 * 60 * 1000,
  });

  return {
    pacing: query.data as WorkoutPacingResult | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
    // Helper to check if we have pacing data
    hasPacingData: !!query.data && query.data.movementPacing.length > 0,
    // Helper to check if there are any heavy pacing movements
    hasHeavyPacingMovements: query.data?.movementPacing.some(m => m.pacingLevel === 'Heavy') ?? false,
  };
}
