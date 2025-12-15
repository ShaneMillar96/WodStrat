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
 * Hook for fetching athlete benchmarks
 */
export function useAthleteBenchmarksQuery(athleteId: string | undefined) {
  return useQuery({
    queryKey: queryKeys.benchmarks.athleteBenchmarks(athleteId!),
    queryFn: () => benchmarkService.getAthleteBenchmarks(athleteId!),
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
 * Hook for fetching benchmark summary
 */
export function useBenchmarkSummaryQuery(athleteId: string | undefined) {
  return useQuery({
    queryKey: queryKeys.benchmarks.summary(athleteId!),
    queryFn: () => benchmarkService.getSummary(athleteId!),
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
 * Provides merged view of definitions with athlete's recorded values
 */
export function useAthleteBenchmarks(
  athleteId: string | undefined,
  categoryFilter?: BenchmarkCategory | 'All'
) {
  const { allDefinitions, isLoading: definitionsLoading, error: definitionsError } =
    useBenchmarkDefinitions();
  const benchmarksQuery = useAthleteBenchmarksQuery(athleteId);
  const summaryQuery = useBenchmarkSummaryQuery(athleteId);

  // Create a map of benchmark definition ID to athlete benchmark
  const benchmarkMap = new Map<string, AthleteBenchmark>();
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
    getBenchmarkByDefinitionId: (definitionId: string) =>
      benchmarkMap.get(definitionId) ?? null,

    // Helper to check if a benchmark has been recorded
    hasBenchmarkForDefinition: (definitionId: string) =>
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
