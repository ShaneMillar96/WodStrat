import { useQuery } from '@tanstack/react-query';
import { movementService } from '../services/movementService';
import { queryKeys } from '../lib/queryKeys';
import { ApiException } from '../services';
import type { MovementDefinition, MovementCategory, MovementLookupResult } from '../types';

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
 * Hook for fetching a single movement by canonical name
 * @param canonicalName - The canonical name to fetch
 * @param enabled - Whether the query should run
 */
export function useMovementQuery(canonicalName: string, enabled: boolean = true) {
  return useQuery({
    queryKey: queryKeys.movements.detail(canonicalName),
    queryFn: () => movementService.getByCanonicalName(canonicalName),
    enabled: enabled && !!canonicalName,
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
 * Hook for looking up a movement by alias
 * Returns 404 error if not found (use useMovementSearch for null response)
 * @param alias - The alias to lookup (e.g., "T2B")
 * @param enabled - Whether the query should run
 */
export function useMovementLookup(alias: string, enabled: boolean = true) {
  return useQuery({
    queryKey: queryKeys.movements.lookup(alias),
    queryFn: () => movementService.lookup(alias),
    enabled: enabled && !!alias.trim(),
    staleTime: 1000 * 60 * 60, // 1 hour - alias mappings rarely change
    retry: (failureCount, error) => {
      // Don't retry 404s for lookups
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 2;
    },
  });
}

/**
 * Hook for searching movements
 * Returns null if no match found (doesn't throw 404)
 * Debounced search typically handled by component
 * @param query - The search query
 * @param enabled - Whether the query should run
 */
export function useMovementSearch(query: string, enabled: boolean = true) {
  return useQuery<MovementLookupResult, ApiException>({
    queryKey: queryKeys.movements.search(query),
    queryFn: () => movementService.search(query),
    enabled: enabled && !!query.trim(),
    staleTime: 1000 * 60 * 5, // 5 minutes for search results
    retry: false,
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

    // Refetch function
    refetch: query.refetch,

    // Helper to get a movement by ID
    getMovementById: (id: number) =>
      query.data?.find((m) => m.id === id) ?? null,

    // Helper to get a movement by canonical name
    getMovementByCanonicalName: (name: string) =>
      query.data?.find((m) => m.canonicalName === name) ?? null,

    // Helper to find movement by alias (local search in loaded data)
    findByAlias: (alias: string): MovementDefinition | null => {
      const lowerAlias = alias.toLowerCase();
      return (
        query.data?.find(
          (m) =>
            m.canonicalName.toLowerCase() === lowerAlias ||
            m.displayName.toLowerCase() === lowerAlias ||
            m.aliases.some((a) => a.toLowerCase() === lowerAlias)
        ) ?? null
      );
    },
  };
}
