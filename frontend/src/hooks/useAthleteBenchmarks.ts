import { useQuery } from '@tanstack/react-query';
import { benchmarkService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type {
  AthleteBenchmark,
  BenchmarkRowData,
  BenchmarkCategory,
} from '../types';
import { useBenchmarkDefinitions } from './useBenchmarkDefinitions';

/**
 * Hook for fetching current user's benchmarks
 */
export function useMyBenchmarksQuery() {
  return useQuery({
    queryKey: queryKeys.benchmarks.myBenchmarks(),
    queryFn: () => benchmarkService.getMyBenchmarks(),
    retry: (failureCount, error) => {
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Hook for fetching current user's benchmark summary
 */
export function useMyBenchmarkSummaryQuery() {
  return useQuery({
    queryKey: queryKeys.benchmarks.mySummary(),
    queryFn: () => benchmarkService.getMySummary(),
    retry: (failureCount, error) => {
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Legacy hook for fetching athlete benchmarks by ID (for backward compatibility)
 */
export function useAthleteBenchmarksQuery(athleteId: number | undefined) {
  return useQuery({
    queryKey: queryKeys.benchmarks.athleteBenchmarks(athleteId!),
    queryFn: () => benchmarkService.getMyBenchmarks(), // Uses session-based endpoint
    enabled: !!athleteId,
    retry: (failureCount, error) => {
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Legacy hook for fetching benchmark summary by athlete ID (for backward compatibility)
 */
export function useBenchmarkSummaryQuery(athleteId: number | undefined) {
  return useQuery({
    queryKey: queryKeys.benchmarks.summary(athleteId!),
    queryFn: () => benchmarkService.getMySummary(), // Uses session-based endpoint
    enabled: !!athleteId,
    retry: (failureCount, error) => {
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Combined hook for athlete benchmarks with definitions
 * Uses session-based identification (no athleteId parameter)
 */
export function useAthleteBenchmarks(categoryFilter?: BenchmarkCategory | 'All') {
  const { allDefinitions, isLoading: definitionsLoading, error: definitionsError } =
    useBenchmarkDefinitions();
  const benchmarksQuery = useMyBenchmarksQuery();
  const summaryQuery = useMyBenchmarkSummaryQuery();

  // Create a map of benchmark definition ID to athlete benchmark
  const benchmarkMap = new Map<number, AthleteBenchmark>();
  (benchmarksQuery.data ?? []).forEach((benchmark) => {
    benchmarkMap.set(benchmark.benchmarkDefinitionId, benchmark);
  });

  // Merge definitions with athlete benchmarks
  const allBenchmarkRows: BenchmarkRowData[] = allDefinitions
    .sort((a, b) => a.displayOrder - b.displayOrder)
    .map((definition) => {
      const athleteBenchmark = benchmarkMap.get(definition.id) ?? null;
      return {
        definition,
        athleteBenchmark,
        hasValue: athleteBenchmark !== null,
      };
    });

  // Filter by category if specified
  const filteredRows =
    categoryFilter && categoryFilter !== 'All'
      ? allBenchmarkRows.filter((row) => row.definition.category === categoryFilter)
      : allBenchmarkRows;

  // Calculate recorded count
  const recordedCount = allBenchmarkRows.filter((row) => row.hasValue).length;
  const totalCount = allDefinitions.length;

  return {
    // Row data for display
    benchmarkRows: filteredRows,
    allBenchmarkRows,

    // Summary data
    summary: summaryQuery.data,
    recordedCount,
    totalCount,
    meetsMinimumRequirement: summaryQuery.data?.meetsMinimumRequirement ?? false,
    minimumRequired: summaryQuery.data?.minimumRequired ?? 3,
    benchmarksByCategory: summaryQuery.data?.benchmarksByCategory ?? {},

    // Loading states
    isLoading: definitionsLoading || benchmarksQuery.isLoading || summaryQuery.isLoading,
    isFetching: benchmarksQuery.isFetching || summaryQuery.isFetching,

    // Errors
    error: (definitionsError ?? benchmarksQuery.error ?? summaryQuery.error) as
      | ApiException
      | null,

    // Helper to get benchmark by definition ID
    getBenchmarkByDefinitionId: (definitionId: number) =>
      benchmarkMap.get(definitionId) ?? null,

    // Helper to check if a benchmark has been recorded
    hasBenchmarkForDefinition: (definitionId: number) =>
      benchmarkMap.has(definitionId),

    // Refetch functions
    refetchBenchmarks: benchmarksQuery.refetch,
    refetchSummary: summaryQuery.refetch,
    refetchAll: () => {
      benchmarksQuery.refetch();
      summaryQuery.refetch();
    },
  };
}
