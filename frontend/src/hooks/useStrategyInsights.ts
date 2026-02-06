import { useQuery } from '@tanstack/react-query';
import { strategyInsightsService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { StrategyInsightsResult } from '../types';

/**
 * Hook for fetching strategy insights for a workout
 * @param athleteId - The athlete's unique identifier
 * @param workoutId - The workout's unique identifier
 * @param options - Optional configuration
 */
export function useStrategyInsights(
  athleteId: number | null | undefined,
  workoutId: number | null | undefined,
  options: { enabled?: boolean } = {}
) {
  const { enabled = true } = options;

  const query = useQuery({
    queryKey: queryKeys.strategyInsights.workout(athleteId ?? 0, workoutId ?? 0),
    queryFn: () => strategyInsightsService.getStrategyInsights(athleteId!, workoutId!),
    enabled: enabled && !!athleteId && !!workoutId && athleteId > 0 && workoutId > 0,
    retry: (failureCount, error) => {
      // Don't retry on 404 (no benchmark data) or 401 (unauthorized)
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 2;
    },
    // Cache for 5 minutes since insights are relatively stable
    staleTime: 5 * 60 * 1000,
  });

  return {
    insights: query.data as StrategyInsightsResult | undefined,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,
    refetch: query.refetch,
    // Helper to check if we have insights data
    hasInsightsData: !!query.data,
    // Helper to check if there are high severity alerts
    hasCriticalAlerts: query.data?.riskAlerts.some(a => a.severity === 'High') ?? false,
    // Helper to get key focus count
    keyFocusCount: query.data?.keyFocusMovements.length ?? 0,
  };
}
