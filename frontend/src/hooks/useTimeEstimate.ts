import { useQuery } from '@tanstack/react-query';
import { timeEstimateService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { TimeEstimateResult, EmomFeasibility } from '../types';

/**
 * Hook for fetching time estimate for a workout
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param options - Optional configuration
 */
export function useTimeEstimate(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  options: { enabled?: boolean } = {}
) {
  const { enabled = true } = options;

  const query = useQuery({
    queryKey: queryKeys.timeEstimate.workout(athleteId ?? 0, workoutId ?? 0),
    queryFn: () => timeEstimateService.getTimeEstimate(athleteId!, workoutId!),
    enabled: enabled && !!athleteId && !!workoutId && athleteId > 0 && workoutId > 0,
    retry: (failureCount, error) => {
      // Don't retry on 404 (no benchmark data) or 401 (unauthorized)
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 2;
    },
    // Cache for 5 minutes since time estimates are relatively stable
    staleTime: 5 * 60 * 1000,
  });

  return {
    timeEstimate: query.data as TimeEstimateResult | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
    // Helper to check if we have estimate data
    hasEstimateData: !!query.data,
    // Helper to check confidence level
    isHighConfidence: query.data?.confidenceLevel === 'High',
    // Helper to check if there are rest recommendations
    hasRestRecommendations: (query.data?.restRecommendations?.length ?? 0) > 0,
  };
}

/**
 * Hook for fetching EMOM feasibility data
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param workoutType - The workout type (only fetches for EMOM workouts)
 * @param options - Optional configuration
 */
export function useEmomFeasibility(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  workoutType: string | null | undefined,
  options: { enabled?: boolean } = {}
) {
  const { enabled = true } = options;
  const isEmom = workoutType === 'Emom';

  const query = useQuery({
    queryKey: queryKeys.timeEstimate.emomFeasibility(athleteId ?? 0, workoutId ?? 0),
    queryFn: () => timeEstimateService.getEmomFeasibility(athleteId!, workoutId!),
    enabled: enabled && isEmom && !!athleteId && !!workoutId && athleteId > 0 && workoutId > 0,
    retry: (failureCount, error) => {
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 2;
    },
    staleTime: 5 * 60 * 1000,
  });

  return {
    feasibility: query.data as EmomFeasibility[] | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
    // Helper to check if all minutes are feasible
    allFeasible: query.data?.every(m => m.isFeasible) ?? false,
    // Helper to get count of infeasible minutes
    infeasibleCount: query.data?.filter(m => !m.isFeasible).length ?? 0,
  };
}
