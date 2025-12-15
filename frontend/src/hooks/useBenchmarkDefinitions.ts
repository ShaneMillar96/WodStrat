import { useQuery } from '@tanstack/react-query';
import { benchmarkService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { BenchmarkDefinition, BenchmarkCategory } from '../types';

/**
 * Hook for fetching all benchmark definitions
 */
export function useBenchmarkDefinitionsQuery() {
  return useQuery({
    queryKey: queryKeys.benchmarks.definitions(),
    queryFn: () => benchmarkService.getDefinitions(),
    staleTime: 1000 * 60 * 60, // 1 hour - definitions rarely change
    retry: (failureCount, error) => {
      // Don't retry on 404 errors
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 2;
    },
  });
}

/**
 * Hook for benchmark definitions with filtering by category
 */
export function useBenchmarkDefinitions(category?: BenchmarkCategory | 'All') {
  const query = useBenchmarkDefinitionsQuery();

  // Filter definitions by category if specified
  const filteredDefinitions = category && category !== 'All'
    ? query.data?.filter((def) => def.category === category) ?? []
    : query.data ?? [];

  // Sort by display order
  const sortedDefinitions = [...filteredDefinitions].sort(
    (a, b) => a.displayOrder - b.displayOrder
  );

  // Get definitions grouped by category
  const definitionsByCategory = (query.data ?? []).reduce<
    Record<BenchmarkCategory, BenchmarkDefinition[]>
  >(
    (acc, def) => {
      if (!acc[def.category]) {
        acc[def.category] = [];
      }
      acc[def.category].push(def);
      return acc;
    },
    {} as Record<BenchmarkCategory, BenchmarkDefinition[]>
  );

  return {
    // Query state
    definitions: sortedDefinitions,
    allDefinitions: query.data ?? [],
    definitionsByCategory,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error as ApiException | null,

    // Helper to get a definition by ID
    getDefinitionById: (id: string) =>
      query.data?.find((def) => def.id === id) ?? null,

    // Helper to get a definition by slug
    getDefinitionBySlug: (slug: string) =>
      query.data?.find((def) => def.slug === slug) ?? null,
  };
}
