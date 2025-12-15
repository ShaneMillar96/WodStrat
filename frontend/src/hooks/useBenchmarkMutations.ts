import { useMutation, useQueryClient } from '@tanstack/react-query';
import { benchmarkService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { CreateBenchmarkRequest, UpdateBenchmarkRequest } from '../types';

/**
 * Hook for creating a new benchmark result
 */
export function useCreateBenchmarkMutation(athleteId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBenchmarkRequest) =>
      benchmarkService.create(athleteId, data),
    onSuccess: () => {
      // Invalidate athlete benchmarks and summary queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.athleteBenchmarks(athleteId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.summary(athleteId),
      });
    },
  });
}

/**
 * Hook for updating an existing benchmark result
 */
export function useUpdateBenchmarkMutation(athleteId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      benchmarkId,
      data,
    }: {
      benchmarkId: number;
      data: UpdateBenchmarkRequest;
    }) => benchmarkService.update(athleteId, benchmarkId, data),
    onSuccess: () => {
      // Invalidate athlete benchmarks and summary queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.athleteBenchmarks(athleteId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.summary(athleteId),
      });
    },
  });
}

/**
 * Hook for deleting a benchmark result
 */
export function useDeleteBenchmarkMutation(athleteId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (benchmarkId: number) =>
      benchmarkService.delete(athleteId, benchmarkId),
    onSuccess: () => {
      // Invalidate athlete benchmarks and summary queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.athleteBenchmarks(athleteId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.benchmarks.summary(athleteId),
      });
    },
  });
}

/**
 * Combined hook for all benchmark mutations
 * Provides create, update, and delete operations with unified state
 */
export function useBenchmarkMutations(athleteId: number) {
  const createMutation = useCreateBenchmarkMutation(athleteId);
  const updateMutation = useUpdateBenchmarkMutation(athleteId);
  const deleteMutation = useDeleteBenchmarkMutation(athleteId);

  const isLoading =
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending;

  const error =
    createMutation.error || updateMutation.error || deleteMutation.error;

  return {
    // Mutation functions
    createBenchmark: createMutation.mutateAsync,
    updateBenchmark: (benchmarkId: number, data: UpdateBenchmarkRequest) =>
      updateMutation.mutateAsync({ benchmarkId, data }),
    deleteBenchmark: deleteMutation.mutateAsync,

    // Loading states
    isLoading,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Success states
    createSuccess: createMutation.isSuccess,
    updateSuccess: updateMutation.isSuccess,
    deleteSuccess: deleteMutation.isSuccess,

    // Error state
    error: error as ApiException | null,

    // Reset mutation states
    resetMutationState: () => {
      createMutation.reset();
      updateMutation.reset();
      deleteMutation.reset();
    },
  };
}
