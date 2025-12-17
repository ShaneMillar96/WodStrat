import { useQuery } from '@tanstack/react-query';
import { movementService } from '../services/workoutService';
import { queryKeys } from '../lib/queryKeys';
import { ApiException } from '../services';
import type { MovementDefinition, MovementCategory } from '../types/workout';

/**
 * Hook for fetching all movement definitions
 * @param category - Optional category filter
 */
export function useMovementsQuery(category?: MovementCategory) {
  return useQuery({
    queryKey: queryKeys.movements.list(category),
    queryFn: () => movementService.getAll(category),
    staleTime: 1000 * 60 * 60, // 1 hour - definitions rarely change
    retry: (failureCount, error) => {
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 2;
    },
  });
}

/**
 * Hook for movement definitions with filtering and helpers
 */
export function useMovements(category?: MovementCategory) {
  const query = useMovementsQuery(category);

  // Group by category
  const movementsByCategory = (query.data ?? []).reduce<
    Record<MovementCategory, MovementDefinition[]>
  >(
    (acc, movement) => {
      const cat = movement.category as MovementCategory;
      if (!acc[cat]) {
        acc[cat] = [];
      }
      acc[cat].push(movement);
      return acc;
    },
    {} as Record<MovementCategory, MovementDefinition[]>
  );

  return {
    // Query state
    movements: query.data ?? [],
    movementsByCategory,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,

    // Helper to get a movement by ID
    getMovementById: (id: number) =>
      query.data?.find((m) => m.id === id) ?? null,

    // Helper to get a movement by canonical name
    getMovementByCanonicalName: (name: string) =>
      query.data?.find((m) => m.canonicalName === name) ?? null,
  };
}
