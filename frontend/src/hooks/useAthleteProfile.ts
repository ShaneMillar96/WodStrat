import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { athleteService, ApiException } from '../services';
import { queryKeys } from '../lib/queryKeys';
import { useAthleteContext } from '../contexts';
import type { CreateAthleteRequest, UpdateAthleteRequest } from '../types';

/**
 * Hook for fetching the current user's athlete profile
 */
export function useMyAthleteQuery() {
  return useQuery({
    queryKey: queryKeys.athletes.me(),
    queryFn: () => athleteService.getMe(),
    retry: (failureCount, error) => {
      // Don't retry on 401 (handled by api.ts) or 404
      if (error instanceof ApiException && (error.isNotFound() || error.status === 401)) {
        return false;
      }
      return failureCount < 1;
    },
  });
}

/**
 * Hook for fetching an athlete profile by ID (for backward compatibility)
 */
export function useAthleteQuery(id: number | undefined) {
  return useQuery({
    queryKey: queryKeys.athletes.detail(id!),
    queryFn: () => athleteService.getById(id!),
    enabled: !!id,
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
  const { setAthleteId } = useAthleteContext();

  return useMutation({
    mutationFn: (data: CreateAthleteRequest) => athleteService.create(data),
    onSuccess: (newAthlete) => {
      // Update the cache
      queryClient.setQueryData(queryKeys.athletes.me(), newAthlete);
      // Set athlete ID in context
      setAthleteId(newAthlete.id);
    },
  });
}

/**
 * Hook for updating the current user's athlete profile
 */
export function useUpdateAthleteMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateAthleteRequest) => athleteService.update(data),
    onSuccess: (updatedAthlete) => {
      queryClient.setQueryData(queryKeys.athletes.me(), updatedAthlete);
    },
  });
}

/**
 * Hook for deleting the current user's athlete profile
 */
export function useDeleteAthleteMutation() {
  const queryClient = useQueryClient();
  const { clearAthlete } = useAthleteContext();

  return useMutation({
    mutationFn: () => athleteService.delete(),
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: queryKeys.athletes.me() });
      clearAthlete();
    },
  });
}

/**
 * Combined hook for athlete profile operations
 * Uses session-based identification (no ID parameter needed)
 */
export function useAthleteProfile() {
  const query = useMyAthleteQuery();
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
    hasAthlete: query.data !== null && query.data !== undefined,

    // Mutations
    createAthlete: createMutation.mutateAsync,
    updateAthlete: updateMutation.mutateAsync,
    deleteAthlete: deleteMutation.mutateAsync,

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
