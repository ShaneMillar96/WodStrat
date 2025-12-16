import { useMutation, useQueryClient } from '@tanstack/react-query';
import { benchmarkService } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { CreateBenchmarkRequest, UpdateBenchmarkRequest } from '../types';

/**
 * Hook for benchmark CRUD mutations
 * Uses session-based identification (no athleteId parameter)
 */
export function useBenchmarkMutations() {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateBenchmarkRequest) => benchmarkService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateBenchmarkRequest }) =>
      benchmarkService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => benchmarkService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });

  return {
    // Mutation functions
    createBenchmark: createMutation.mutateAsync,
    updateBenchmark: (id: number, data: UpdateBenchmarkRequest) =>
      updateMutation.mutateAsync({ id, data }),
    deleteBenchmark: deleteMutation.mutateAsync,

    // Loading states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Success states
    createSuccess: createMutation.isSuccess,
    updateSuccess: updateMutation.isSuccess,
    deleteSuccess: deleteMutation.isSuccess,

    // Errors
    error: createMutation.error || updateMutation.error || deleteMutation.error,

    // Reset
    resetMutationState: () => {
      createMutation.reset();
      updateMutation.reset();
      deleteMutation.reset();
    },
  };
}

// Legacy exports for backward compatibility
export function useCreateBenchmarkMutation(_athleteId?: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBenchmarkRequest) => benchmarkService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });
}

export function useUpdateBenchmarkMutation(_athleteId?: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      benchmarkId,
      data,
    }: {
      benchmarkId: number;
      data: UpdateBenchmarkRequest;
    }) => benchmarkService.update(benchmarkId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });
}

export function useDeleteBenchmarkMutation(_athleteId?: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (benchmarkId: number) => benchmarkService.delete(benchmarkId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.myBenchmarks() });
      queryClient.invalidateQueries({ queryKey: queryKeys.benchmarks.mySummary() });
    },
  });
}
