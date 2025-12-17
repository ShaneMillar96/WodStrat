import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workoutService } from '../services/workoutService';
import { queryKeys } from '../lib/queryKeys';
import { ApiException } from '../services';
import type { CreateWorkoutRequest, UpdateWorkoutRequest } from '../types/workout';

/**
 * Hook for fetching all user workouts
 */
export function useWorkoutsQuery() {
  return useQuery({
    queryKey: queryKeys.workouts.lists(),
    queryFn: () => workoutService.getAll(),
    retry: (failureCount, error) => {
      if (error instanceof ApiException && error.isNotFound()) {
        return false;
      }
      return failureCount < 2;
    },
  });
}

/**
 * Hook for workout CRUD mutations
 */
export function useWorkoutMutations() {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateWorkoutRequest) => workoutService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workouts.lists() });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateWorkoutRequest }) =>
      workoutService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workouts.lists() });
      queryClient.invalidateQueries({ queryKey: queryKeys.workouts.detail(variables.id) });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => workoutService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workouts.lists() });
    },
  });

  return {
    // Mutation functions
    createWorkout: createMutation.mutateAsync,
    updateWorkout: (id: number, data: UpdateWorkoutRequest) =>
      updateMutation.mutateAsync({ id, data }),
    deleteWorkout: deleteMutation.mutateAsync,

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

/**
 * Combined hook for workout list with mutations
 */
export function useWorkouts() {
  const query = useWorkoutsQuery();
  const mutations = useWorkoutMutations();

  // Sort workouts by creation date (newest first)
  const sortedWorkouts = [...(query.data ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  return {
    // Query state
    workouts: sortedWorkouts,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    queryError: query.error as ApiException | null,
    refetch: query.refetch,

    // Mutations
    ...mutations,
  };
}
