import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { athleteService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import type { CreateAthleteRequest, UpdateAthleteRequest } from '../types';

/**
 * Hook for fetching an athlete profile by ID
 */
export function useAthleteQuery(id: number | undefined) {
  return useQuery({
    queryKey: queryKeys.athletes.detail(id!),
    queryFn: () => athleteService.getById(id!),
    enabled: !!id && id > 0,
    retry: (failureCount, error) => {
      // Don't retry on 404 errors
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Hook for creating a new athlete profile
 */
export function useCreateAthleteMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAthleteRequest) => athleteService.create(data),
    onSuccess: (newAthlete) => {
      // Add the new athlete to the cache
      queryClient.setQueryData(queryKeys.athletes.detail(newAthlete.id), newAthlete);
      // Invalidate any list queries
      queryClient.invalidateQueries({ queryKey: queryKeys.athletes.lists() });
    },
  });
}

/**
 * Hook for updating an existing athlete profile
 */
export function useUpdateAthleteMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateAthleteRequest }) =>
      athleteService.update(id, data),
    onSuccess: (updatedAthlete) => {
      // Update the athlete in the cache
      queryClient.setQueryData(
        queryKeys.athletes.detail(updatedAthlete.id),
        updatedAthlete
      );
      // Invalidate any list queries
      queryClient.invalidateQueries({ queryKey: queryKeys.athletes.lists() });
    },
  });
}

/**
 * Hook for deleting an athlete profile
 */
export function useDeleteAthleteMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => athleteService.delete(id),
    onSuccess: (_, id) => {
      // Remove the athlete from the cache
      queryClient.removeQueries({ queryKey: queryKeys.athletes.detail(id) });
      // Invalidate any list queries
      queryClient.invalidateQueries({ queryKey: queryKeys.athletes.lists() });
    },
  });
}

/**
 * Combined hook for athlete profile operations
 * Provides query and mutation functions with unified state
 */
export function useAthleteProfile(id?: number) {
  const query = useAthleteQuery(id);
  const createMutation = useCreateAthleteMutation();
  const updateMutation = useUpdateAthleteMutation();
  const deleteMutation = useDeleteAthleteMutation();

  const isLoading =
    query.isLoading ||
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending;

  const error =
    query.error ||
    createMutation.error ||
    updateMutation.error ||
    deleteMutation.error;

  return {
    // Query state
    athlete: query.data,
    isLoading,
    isFetching: query.isFetching,
    error: error as ApiException | null,

    // Mutations
    createAthlete: createMutation.mutateAsync,
    updateAthlete: (data: UpdateAthleteRequest) =>
      updateMutation.mutateAsync({ id: id!, data }),
    deleteAthlete: () => deleteMutation.mutateAsync(id!),

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,

    // Success states
    createSuccess: createMutation.isSuccess,
    updateSuccess: updateMutation.isSuccess,
    deleteSuccess: deleteMutation.isSuccess,

    // Reset mutation states
    resetMutationState: () => {
      createMutation.reset();
      updateMutation.reset();
      deleteMutation.reset();
    },

    // Get created athlete ID after successful creation
    createdAthleteId: createMutation.data?.id,
  };
}
